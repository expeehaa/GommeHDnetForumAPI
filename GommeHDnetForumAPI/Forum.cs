using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Authentication;
using System.Threading.Tasks;
using CloudflareSolverRe;
using GommeHDnetForumAPI.Exceptions;
using GommeHDnetForumAPI.GraphQL;
using GommeHDnetForumAPI.Models;
using GommeHDnetForumAPI.Models.Collections;
using GommeHDnetForumAPI.Models.Entities;
using GommeHDnetForumAPI.Parser;
using GommeHDnetForumAPI.Parser.NodeListParser;
using GraphQL;
using GraphQL.Client.Abstractions;
using GraphQL.Client.Http;
using GraphQL.Client.Serializer.Newtonsoft;
using HtmlAgilityPack;

namespace GommeHDnetForumAPI {
	public class Forum {
		private string _email;
		private string _password;

		private string _token;

		public bool HasCredentials => !string.IsNullOrWhiteSpace(_email) && !string.IsNullOrWhiteSpace(_password);
		public bool LoggedIn => SelfUser != null;
		public UserInfo SelfUser { get; private set; }
		public MasterForumInfo MasterForum { get; private set; }

		public string UserAgent {
			get => _httpClient.DefaultRequestHeaders.UserAgent.ToString();
			set => _httpClient.DefaultRequestHeaders.Add("User-Agent", value);
		}

		private CookieContainer   _cookieContainer;
		private HttpClientHandler _httpClientHandler;
		private ClearanceHandler  _clearanceHandler;
		private HttpClient        _httpClient;

		private GraphQLHttpClient _graphQLHttpClient;

		public Forum() : this(null, null) { }

		public Forum(string email, string password) {
			InitHttpClient();
			ChangeCredentials(email, password).GetAwaiter().GetResult();
		}

		public async Task<bool> ChangeCredentials(string email, string password) {
			_email = email;
			_password = password;
			if(!HasCredentials)
				return false;
			var success = await Login();
			if(!success)
				return false;
			var doc = await GetHtmlDocument(ForumPaths.ForumPath);
			MasterForum = new MasterForumParser(this).Parse(doc.DocumentNode);
			return true;
		}

		private void InitHttpClient() {
			ResetCookies();
			_httpClientHandler = new HttpClientHandler {
				CookieContainer = _cookieContainer,
				AllowAutoRedirect = true,
				UseCookies = true,
				ServerCertificateCustomValidationCallback = (message, certificate2, arg3, arg4) => true,
				SslProtocols = SslProtocols.Tls12
			};
			_clearanceHandler = new ClearanceHandler(_httpClientHandler) {
				MaxTries = 5,
				MaxCaptchaTries = 3,
				ClearanceDelay = 3000,
			};
			_httpClient = new HttpClient(_clearanceHandler) { BaseAddress = new Uri(ForumPaths.BaseUrl) };
			_httpClient.DefaultRequestHeaders.Add("Cache-Control", "no-cache");
			_httpClient.DefaultRequestHeaders.Add("Referer", ForumPaths.BaseUrl);
			_httpClient.Timeout = TimeSpan.FromSeconds(10);
			UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64; rv:92.0) Gecko/20100101 Firefox/92.0";

			_graphQLHttpClient = new GraphQLHttpClient(ForumPaths.GraphQLApiUrl, new NewtonsoftJsonSerializer());
		}

		private void ResetCookies() {
			_cookieContainer = new CookieContainer();

			if(_httpClientHandler is not null) {
				_httpClientHandler.CookieContainer = _cookieContainer;
			}
		}

		private async Task<bool> Login() {
			if(!HasCredentials)
				throw new CredentialsRequiredException();

			ResetCookies();

			var forumResp = await GetData(ForumPaths.ForumLoginPath);

			var userMutationResponse = await _graphQLHttpClient.SendMutationAsync(new GraphQLRequest {
				OperationName = "login",
				Query = @"
					mutation login($email: String!, $password: String!, $captcha: String!) {
						user {
							login(user: $email, password: $password, captcha: $captcha) {
								success
								requireTwoFa
								needToMigrate
								token
								error
								__typename
							}
							__typename
						}
					}
				",
				Variables = new {
					email = _email,
					password = _password,
					captcha = "Please don't block this ^_^",
				},
			}, () => new { user = new UserMutation() });

			_token = userMutationResponse.Data.user.Login.Token;

			var forumAuthResponse = await _graphQLHttpClient.SendMutationAsync(new AuthorizedRequest {
				OperationName = "forumAuth",
				Query = @"
					mutation forumAuth($redirect: String!, $darkMode: Boolean) {
						user {
							forumAuth(redirect: $redirect, darkMode: $darkMode) {
								success
								error
								__typename
							}
						__typename
						}
					}
				",
				Variables = new {
					darkMode = true,
					redirect = ForumPaths.BaseUrl + ForumPaths.ForumConnectedAccountPath,
				},
				Authorization = _token,
			}, () => new { user = new UserMutation() });

			_cookieContainer.Add(new Uri("https://www.gommehd.net"), new CookieCollection() {
				new Cookie("user-token", _token),
				new Cookie("i18n_redirected", "de_DE"),
			});

			await GetData($"{ForumPaths.ForumConnectedAccountPath}?code={forumAuthResponse.Data.user.ForumAuth.ForumToken}");

			var forumDoc = await GetHtmlDocument(ForumPaths.ForumPath);
			var selfUserId = forumDoc.DocumentNode.SelectSingleNode("//div[contains(concat(' ', normalize-space(@class), ' '), ' p-header-content ')]//div[contains(concat(' ', normalize-space(@class), ' '), ' p-account ')]//span[contains(concat(' ', normalize-space(@class), ' '), ' avatar ')]").GetAttributeValue("data-user-id", 0);
			SelfUser = await GetUserInfo(selfUserId);

			return true;
		}

		/// <summary>
		/// HTTP post request to forum without login routing.
		/// </summary>
		/// <param name="path">Path after ForumPaths.BaseUrl.</param>
		/// <param name="body">ForumUrlEncoded body.</param>
		/// <param name="checkSuccess">Calls EnsureSuccessStatusCode on the response if true.</param>
		/// <exception cref="HttpRequestException">Thrown if status code != 200</exception>
		/// <returns>HttpResponseMessage</returns>
		public async Task<HttpResponseMessage> PostData(string path, List<KeyValuePair<string, string>> body = null, bool checkSuccess = true) {
			var fuec = new FormUrlEncodedContent(body);
			fuec.Headers.ContentType = MediaTypeHeaderValue.Parse("application/x-www-form-urlencoded");
			var response = await _httpClient.PostAsync(ForumPaths.BaseUrl + path, fuec).ConfigureAwait(false);
			return checkSuccess ? response.EnsureSuccessStatusCode() : response;
		}

		/// <summary>
		/// HTTP get request to forum
		/// </summary>
		/// <param name="path">Path after ForumPaths.BaseUrl</param>
		/// <param name="checkSuccess">Calls EnsureSuccessStatusCode on the response if true.</param>
		/// <param name="addBaseUrl">Prepends ForumPaths.BaseUrl to <paramref name="path"/> if true.</param>
		/// <exception cref="HttpRequestException">Thrown if status code != 200</exception>
		/// <returns>HttpResponseMessage</returns>
		public async Task<HttpResponseMessage> GetData(string path, bool checkSuccess = true, bool addBaseUrl = true) {
			var response = await _httpClient.GetAsync(addBaseUrl ? ForumPaths.BaseUrl + path : path).ConfigureAwait(false);
			return checkSuccess ? response.EnsureSuccessStatusCode() : response;
		}

		public async Task<HtmlDocument> GetHtmlDocument(string path) {
			var response = await GetData(path);
			var doc = new HtmlDocument();
			doc.LoadHtml(await response.Content.ReadAsStringAsync());
			return doc;
		}

		public async Task<List<ConversationInfo>> GetConversations(int startPage = 0, int pageCount = 0) {
			startPage = Math.Max(1, startPage);

			var doc                  = await GetHtmlDocument($"{ForumPaths.ConversationsPath}page-{startPage}");
			var lastPageNumberString = doc.DocumentNode.SelectSingleNode("(//div[contains(concat(' ', normalize-space(@class), ' '), ' pageNav ')]/ul/li)[last()]")?.InnerText;
			var lastPageNumber       = !string.IsNullOrWhiteSpace(lastPageNumberString) ? int.Parse(lastPageNumberString) : 1;
			var pageMax              = pageCount <= 0 ? lastPageNumber : Math.Min(startPage+pageCount-1, lastPageNumber);
			var docs                 = new List<HtmlDocument>{ doc };

			for(var i = startPage + 1; i <= pageMax; i++) {
				docs.Add(await GetHtmlDocument($"{ForumPaths.ConversationsPath}page-{i}"));
			}

			var parser = new ConversationsNodeListParser(this);
			return docs.SelectMany(d => parser.Parse(d.DocumentNode)).ToList();
		}

		public async Task<User> GetSelfUser() {
			var authResponse = await _graphQLHttpClient.SendQueryAsync(new AuthorizedRequest {
				OperationName = "auth",
				Query = @"
					query auth($token: String) {
						auth(token: $token) {
							success
							forceTwoFa
							user {
								id
								email
								hasTwoFa
								locale
								emailVerified
								minecraft {
									uuid
									name
									__typename
								}
								discord {
									id
									name
									hash
									avatar
									__typename
								}
								teamspeak
								twitch {
									channel
									avatar
									__typename
								}
								youtube {
									channel
									name
									avatar
									__typename
								}
								twitter {
									id
									name
									handle
									avatar
									__typename
								}
								__typename
							}
							__typename
						}
					}
				",
				Variables = new {
					token = _token,
				},
				Authorization = _token,
			}, () => new { auth = new AuthResponse() });

			return authResponse.Data.auth.User;
		}

		/// <summary>
		/// Create a new conversation.
		/// </summary>
		/// <param name="participants">Conversation participants.</param>
		/// <param name="title">Desired conversation title.</param>
		/// <param name="message">Message to send.</param>
		/// <param name="openInvite">Participants may invite other users to the conversation if true.</param>
		/// <returns>ConversationInfo describing the created conversation.</returns>
		public async Task<ConversationInfo> CreateConversation(UserCollection participants, string title, string message, bool openInvite = false)
			=> await CreateConversation((from r in participants select r.Username).ToArray(), title, message, openInvite).ConfigureAwait(false);

		public string GetConversationCreationUrl(UserCollection participants)
			=> GetConversationCreationUrl(participants.Select(p => p.Username).ToArray());

		public string GetConversationCreationUrl(string[] participants)
			=> $"{ForumPaths.ConversationsUrl}add?to={string.Join(",", participants)}";

		/// <summary>
		/// Create a new conversation.
		/// </summary>
		/// <param name="participants">Conversation participants.</param>
		/// <param name="title">Desired conversation title.</param>
		/// <param name="message">Message to send.</param>
		/// <param name="openInvite">Participants may invite other users to the conversation if true.</param>
		/// <returns>ConversationInfo describing the created conversation.</returns>
		public async Task<ConversationInfo> CreateConversation(string[] participants, string title, string message, bool openInvite = false) {
			if(!LoggedIn)
				throw new LoginRequiredException("Log in to create a new conversation!");

			var doc     = await GetHtmlDocument($"{ForumPaths.ConversationsPath}add");
			var xftoken = doc.DocumentNode.GetInputValueByName("_xfToken");

			var kvlist = new List<KeyValuePair<string, string>> {
				new KeyValuePair<string, string>("recipients",   string.Join(',', participants)),
				new KeyValuePair<string, string>("title",        title),
				new KeyValuePair<string, string>("message_html", message),
				new KeyValuePair<string, string>("open_invite",  openInvite ? "1" : "0"),
				new KeyValuePair<string, string>("_xfToken",     xftoken)
			};
			var hrm = await PostData($"{ForumPaths.ConversationsPath}add", kvlist).ConfigureAwait(false);
			doc.LoadHtml(await hrm.Content.ReadAsStringAsync().ConfigureAwait(false));

			return new ConversationInfoParser(this).Parse(doc.DocumentNode);
		}

		public async Task<UserInfo> GetUserInfo(long userId) {
			var hrm = await GetData($"{ForumPaths.MembersPath}{userId}", checkSuccess: false);

			if(hrm.IsSuccessStatusCode) {
				var doc = new HtmlDocument();
				doc.LoadHtml(await hrm.Content.ReadAsStringAsync());
				return new UserInfoParser(this).Parse(doc.DocumentNode);
			} else {
				return null;
			}
		}

		public async Task<UserInfo> GetUserInfo(string username) {
			var doc     = await GetHtmlDocument(ForumPaths.MembersPath);
			var xftoken = doc.DocumentNode.SelectSingleNode("//form[@action='/forum/members/']/input[@name='_xfToken']").GetAttributeValue("value", "");

			var hrm = await PostData(ForumPaths.MembersPath, new List<KeyValuePair<string, string>> {
				new KeyValuePair<string, string>("username", username),
				new KeyValuePair<string, string>("_xfToken", xftoken)
			}, false).ConfigureAwait(false);

			if(hrm.IsSuccessStatusCode) {
				try {
					doc.LoadHtml(await hrm.Content.ReadAsStringAsync().ConfigureAwait(false));
					return new UserInfoParser(this).Parse(doc.DocumentNode);
				} catch(NodeNotFoundException e) {
					throw new UserNotFoundException(null, e);
				}
			} else {
				return null;
			}
		}

		public async Task<UserCollection> GetMembersList(MembersListType type) {
			var hrm = await GetData(ForumPaths.GetMembersListTypePath(type)).ConfigureAwait(false);
			var doc = new HtmlDocument();
			doc.LoadHtml(await hrm.Content.ReadAsStringAsync().ConfigureAwait(false));
			var users = new MembersListNodeListParser(this).Parse(doc.DocumentNode);
			return users.ToUserCollection();
		}

		public async Task<int> GetOnlineUserCount() {
			var response = await _graphQLHttpClient.SendQueryAsync(new GraphQLRequest {
				Query = "{ playerCount }"
			}, () => new { PlayerCount = 0 });
			return response.Data.PlayerCount;
		}
	}
}
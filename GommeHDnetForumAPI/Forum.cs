﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Authentication;
using System.Threading.Tasks;
using CloudflareSolverRe;
using GommeHDnetForumAPI.Exceptions;
using GommeHDnetForumAPI.Models;
using GommeHDnetForumAPI.Models.Collections;
using GommeHDnetForumAPI.Models.Entities;
using GommeHDnetForumAPI.Parser;
using GommeHDnetForumAPI.Parser.LiNodeParser;
using HtmlAgilityPack;

namespace GommeHDnetForumAPI {
	public class Forum {
		private string _username;
		private string _password;

		public bool            HasCredentials => !string.IsNullOrWhiteSpace(_username) && !string.IsNullOrWhiteSpace(_password);
		public bool            LoggedIn       => SelfUser != null;
		public UserInfo        SelfUser       { get; private set; }
		public MasterForumInfo MasterForum    { get; private set; }

		public string UserAgent {
			get => _httpClient.DefaultRequestHeaders.UserAgent.ToString();
			set => _httpClient.DefaultRequestHeaders.Add("User-Agent", value);
		}

		private CookieContainer   _cookieContainer;
		private HttpClientHandler _httpClientHandler;
		private ClearanceHandler  _clearanceHandler;
		private HttpClient        _httpClient;

		public Forum() : this(null, null) { }

		public Forum(string username, string password) {
			InitHttpClient();
			ChangeCredentials(username, password).GetAwaiter().GetResult();
		}

		public async Task<bool> ChangeCredentials(string username, string password) {
			_username = username;
			_password = password;
			ResetCookies();
			if (!HasCredentials) return false;
			var success = await Login().ConfigureAwait(false);
			if (!success) return false;
			var doc = await GetHtmlDocument(ForumPaths.ForumPath);
			MasterForum = new MasterForumParser(this).Parse(doc.DocumentNode);
			return true;
		}

		private void InitHttpClient() {
			ResetCookies();
			_httpClientHandler = new HttpClientHandler {
				CookieContainer                           = _cookieContainer,
				AllowAutoRedirect                         = true,
				UseCookies                                = true,
				ServerCertificateCustomValidationCallback = (message, certificate2, arg3, arg4) => true,
				SslProtocols                              = SslProtocols.Tls12
			};
			_clearanceHandler = new ClearanceHandler(_httpClientHandler) {
				MaxTries        = 5,
				MaxCaptchaTries = 3,
				ClearanceDelay  = 3000,
			};
			_httpClient = new HttpClient(_clearanceHandler) {BaseAddress = new Uri(ForumPaths.BaseUrl)};
			_httpClient.DefaultRequestHeaders.Add("Cache-Control", "no-cache");
			_httpClient.DefaultRequestHeaders.Add("Referer",       ForumPaths.BaseUrl);
			_httpClient.Timeout = TimeSpan.FromSeconds(10);
			UserAgent           = "Mozilla/5.0 (Windows NT 10.0; Win64; x64; rv:80.0) Gecko/20100101 Firefox/80.0";
		}

		private void ResetCookies() {
			_cookieContainer = new CookieContainer();
		}

		private async Task<bool> Login() {
			if (!HasCredentials) throw new CredentialsRequiredException();
			var hrm = await GetData("login").ConfigureAwait(false);
			var doc = new HtmlDocument();
			doc.LoadHtml(await hrm.Content.ReadAsStringAsync().ConfigureAwait(false));
			var csrf = doc.DocumentNode.GetInputValueByName("_csrf");

			if (string.IsNullOrWhiteSpace(csrf)) return false;

			var list = new List<KeyValuePair<string, string>> {
				new KeyValuePair<string, string>("LoginForm[username]", _username),
				new KeyValuePair<string, string>("LoginForm[password]", _password),
				new KeyValuePair<string, string>("_csrf",               csrf)
			};
			var fuec = new FormUrlEncodedContent(list);
			fuec.Headers.ContentType = MediaTypeHeaderValue.Parse("application/x-www-form-urlencoded");
			var response = await _httpClient.PostAsync(ForumPaths.BaseUrl + "login", fuec).ConfigureAwait(false);
			if (!response.IsSuccessStatusCode) {
				return false;
			}

			var html = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
			doc.LoadHtml(html);
			var urlpath = doc.DocumentNode.SelectSingleNode("//div[@class='userbar']//a[@class='btn btn-link profile']").GetAttributeValue("href", "");
			var selfuserDoc = await GetHtmlDocument(urlpath);
			SelfUser = new UserInfoParser(this).Parse(selfuserDoc.DocumentNode);
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

		public async Task<HtmlDocument> GetHtmlDocument(string path, bool addBaseUrl = true) {
			var response = await GetData(path, true, addBaseUrl);
			var doc = new HtmlDocument();
			doc.LoadHtml(await response.Content.ReadAsStringAsync());
			return doc;
		}

		public async Task<List<ConversationInfo>> GetConversations(int startPage = 0, int pageCount = 0) {
			startPage          = Math.Max(1, startPage);
			
			var doc            = await GetHtmlDocument($"{ForumPaths.ConversationsPath}?page={startPage}");
			var lastPageNumber = doc.DocumentNode.SelectSingleNode("//div[@class='PageNav']")?.GetAttributeValue("data-last", 0) ?? 1;
			var pageMax        = pageCount <= 0 ? lastPageNumber : Math.Min(startPage+pageCount-1, lastPageNumber);
			var docs           = new List<HtmlDocument>{ doc };

			for(var i = startPage; i <= pageMax; i++) {
				docs.Add(await GetHtmlDocument($"{ForumPaths.ConversationsPath}?page={i}"));
			}

			var parser = new ConversationLiNodeParser(this);
			return docs.SelectMany(d => parser.Parse(d.DocumentNode)).ToList();
		}

		public async Task<HttpResponseMessage> GetMainForum()
			=> await GetData("forum").ConfigureAwait(false);

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
			if (!LoggedIn) throw new LoginRequiredException("Log in to create a new conversation!");
			var h   = await GetData($"{ForumPaths.ConversationsPath}add").ConfigureAwait(false);
			var doc = new HtmlDocument();
			doc.LoadHtml(await h.Content.ReadAsStringAsync().ConfigureAwait(false));
			var xftoken = doc.DocumentNode.GetInputValueByName("_xfToken");

			var kvlist = new List<KeyValuePair<string, string>> {
				new KeyValuePair<string, string>("recipients",   string.Join(',', participants)),
				new KeyValuePair<string, string>("title",        title),
				new KeyValuePair<string, string>("message_html", message),
				new KeyValuePair<string, string>("open_invite",  openInvite ? "1" : "0"),
				new KeyValuePair<string, string>("_xfToken",     xftoken)
			};
			var hrm = await PostData($"{ForumPaths.ConversationsPath}insert", kvlist).ConfigureAwait(false);
			doc.LoadHtml(await hrm.Content.ReadAsStringAsync().ConfigureAwait(false));

			return new ConversationInfoParser(this).Parse(doc.DocumentNode);
		}

		public async Task<UserInfo> GetUserInfo(long userId) {
			var doc = await GetHtmlDocument($"{ForumPaths.MembersPath}{userId}");
			return new UserInfoParser(this).Parse(doc.DocumentNode);
		}

		//todo: throw exceptions
		public async Task<UserInfo> GetUserInfo(string username) {
			var h   = await GetData(ForumPaths.MembersPath).ConfigureAwait(false);
			var doc = new HtmlDocument();
			doc.LoadHtml(await h.Content.ReadAsStringAsync().ConfigureAwait(false));
			var xftoken = doc.DocumentNode.SelectSingleNode("//form[@action='members/']/input[@name='_xfToken']").GetAttributeValue("value", "");

			var hrm = await PostData(ForumPaths.MembersPath, new List<KeyValuePair<string, string>> {
				new KeyValuePair<string, string>("username", username),
				new KeyValuePair<string, string>("_xfToken", xftoken)
			}, false).ConfigureAwait(false);
			if (!hrm.IsSuccessStatusCode) return null;
			try {
				doc.LoadHtml(await hrm.Content.ReadAsStringAsync().ConfigureAwait(false));
				return new UserInfoParser(this).Parse(doc.DocumentNode);
			} catch (NodeNotFoundException) {
				throw new UserNotFoundException();
			}
		}

		public async Task<UserCollection> GetMembersList(MembersListType type) {
			var hrm = await GetData(ForumPaths.GetMembersListTypePath(type)).ConfigureAwait(false);
			var doc = new HtmlDocument();
			doc.LoadHtml(await hrm.Content.ReadAsStringAsync().ConfigureAwait(false));
			var users = new MembersListLiNodeParser(this).Parse(doc.DocumentNode);
			return users.ToUserCollection();
		}

		public async Task<int> GetOnlineUserCount() {
			var hrm = await GetData(ForumPaths.StatsUsersOnlinePath).ConfigureAwait(false);
			var doc = new HtmlDocument();
			doc.LoadHtml(await hrm.Content.ReadAsStringAsync().ConfigureAwait(false));
			var numberString = string.Join("", doc.DocumentNode.SelectNodes("//dd/span").Select(node => node.InnerText));
			return int.Parse(numberString);
		}

		public async Task<string> GetNotificationText() {
			var hrm = await GetData(ForumPaths.NotificationsPath).ConfigureAwait(false);
			var doc = new HtmlDocument();
			doc.LoadHtml(await hrm.Content.ReadAsStringAsync().ConfigureAwait(false));

			var notificationsContainer = doc.DocumentNode.SelectSingleNode("//div[contains(concat(' ', normalize-space(@class), ' '), ' notificationsContainer ')]");
			if(notificationsContainer != null) {
				foreach(var button in notificationsContainer.SelectNodes("//button") ?? new HtmlNodeCollection(notificationsContainer)) {
					button.Remove();
				}

				return notificationsContainer.InnerText.Trim();
			} else {
				return null;
			}
		}
	}
}
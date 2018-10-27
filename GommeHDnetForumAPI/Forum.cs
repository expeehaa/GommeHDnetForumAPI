using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Authentication;
using System.Threading.Tasks;
using CloudFlareUtilities;
using GommeHDnetForumAPI.DataModels;
using GommeHDnetForumAPI.DataModels.Collections;
using GommeHDnetForumAPI.DataModels.Entities;
using GommeHDnetForumAPI.DataModels.Exceptions;
using GommeHDnetForumAPI.Parser;
using GommeHDnetForumAPI.Parser.LiNodeParser;
using HtmlAgilityPack;

namespace GommeHDnetForumAPI
{
    public class Forum
    {
        private string _username;
        private string _password;

        /// <summary>
        /// True, if neither username nor password are null, empty or whitespace.
        /// </summary>
        public bool HasCredentials => !string.IsNullOrWhiteSpace(_username) && !string.IsNullOrWhiteSpace(_password);

        /// <summary>
        /// True if login was successfull.
        /// </summary>
        public bool LoggedIn => SelfUser != null;

        /// <summary>
        /// UserInfo instance of the logged in user.
        /// </summary>
        public UserInfo SelfUser { get; private set; }

        /// <summary>
        /// Forum info of the top level forum located at https://www.gommehd.net/forum/
        /// </summary>
        public MasterForumInfo MasterForum { get; private set; }

        /// <summary>
        /// Set or get the User-Agent header for all HttpRequests.
        /// </summary>
        public string UserAgent
        {
            get => _httpClient.DefaultRequestHeaders.UserAgent.ToString();
            set => _httpClient.DefaultRequestHeaders.Add("User-Agent", value);
        }

        private CookieContainer _cookieContainer;
        private HttpClientHandler _httpClientHandler;
        private ClearanceHandler _clearanceHandler;
        private HttpClient _httpClient;

        /// <inheritdoc />
        /// <summary>
        /// Constructor without credentials
        /// </summary>
        public Forum() : this(null, null) {}

        /// <summary>
        /// Constructor with credentials
        /// </summary>
        /// <param name="username"></param>
        /// <param name="password"></param>
        public Forum(string username, string password) {
            InitHttpClient();
            ChangeCredentials(username, password).GetAwaiter().GetResult();
        }

        /// <summary>
        /// Change credentials and reload login session.
        /// </summary>
        /// <param name="username">Username</param>
        /// <param name="password">Password</param>
        public async Task<bool> ChangeCredentials(string username, string password)
        {
            _username = username;
            _password = password;
            ResetCookies();
            if (!HasCredentials) return false;
            var success = await Login().ConfigureAwait(false);
            if (!success) return false;
            MasterForum = await new MasterForumParser(this).ParseAsync().ConfigureAwait(false);
            return true;
        }

        /// <summary>
        /// Initiate HttpClient
        /// </summary>
        private void InitHttpClient()
        {
            ResetCookies();
            _httpClientHandler = new HttpClientHandler {
                CookieContainer = _cookieContainer,
                AllowAutoRedirect = true,
                UseCookies = true,
                ServerCertificateCustomValidationCallback = (message, certificate2, arg3, arg4) => true,
                SslProtocols = SslProtocols.Tls12
            };
            _clearanceHandler = new ClearanceHandler(_httpClientHandler) {
                MaxRetries = 5
            };
            _httpClient = new HttpClient(_clearanceHandler) { BaseAddress = new Uri(ForumPaths.BaseUrl) };
            _httpClient.DefaultRequestHeaders.Add("Cache-Control", "no-cache");
            _httpClient.DefaultRequestHeaders.Add("Referer", ForumPaths.BaseUrl);
            _httpClient.Timeout = TimeSpan.FromSeconds(10);
            UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/60.0.3112.101 Safari/537.36";
        }

        private void ResetCookies() {
            _cookieContainer = new CookieContainer();
        }

        /// <summary>
        /// Login method
        /// </summary>
        /// <returns>bool indicating wether login was successful or not</returns>
        private async Task<bool> Login() {
            if (!HasCredentials) throw new CredentialsRequiredException();
            var hrm = await GetData("login").ConfigureAwait(false);
            var doc = new HtmlDocument();
            doc.LoadHtml(await hrm.Content.ReadAsStringAsync().ConfigureAwait(false));
            var csrf = doc.DocumentNode.SelectSingleNode("//input[@name='_csrf']")?.GetAttributeValue("value", "");

            if (string.IsNullOrWhiteSpace(csrf)) return false;

            var list = new List<KeyValuePair<string, string>> {
                new KeyValuePair<string, string>("LoginForm[username]", _username),
                new KeyValuePair<string, string>("LoginForm[password]", _password),
                new KeyValuePair<string, string>("_csrf", csrf)
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
            SelfUser = await new UserInfoParser(this, urlpath, false).ParseAsync().ConfigureAwait(false);
            return true;
        }

        /// <summary>
        /// HTTP post request to forum without login routing.
        /// </summary>
        /// <param name="path">Path after ForumPaths.BaseUrl</param>
        /// <param name="body">ForumUrlEncoded body</param>
        /// <param name="checkSuccess">Wether to call EnsureSuccessStatusCode or not on response</param>
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
        /// <param name="path">Path (after ForumPaths.BaseUrl)</param>
        /// <param name="checkSuccess">Wether to call EnsureSuccessStatusCode or not on response</param>
        /// <param name="addBaseUrl">If true ForumPaths.BaseUrl is prepended to <paramref name="path"/></param>
        /// <exception cref="HttpRequestException">Thrown if status code != 200</exception>
        /// <returns>HttpResponseMessage</returns>
        public async Task<HttpResponseMessage> GetData(string path, bool checkSuccess = true, bool addBaseUrl = true) {
            var response = await _httpClient.GetAsync(addBaseUrl ? ForumPaths.BaseUrl + path : path).ConfigureAwait(false);
            return checkSuccess ? response.EnsureSuccessStatusCode() : response;
        }

        /// <summary>
        /// Get all conversations.
        /// </summary>
        /// <param name="startPage"></param>
        /// <param name="pageCount"></param>
        /// <returns>Object containing all conversations the user had.</returns>
        public async Task<ThreadCollection<ConversationInfo>> GetConversations(int startPage = 0, int pageCount = 0) 
            => await new ConversationsParser(this, startPage, pageCount).ParseAsync().ConfigureAwait(false);

        /// <summary>
        /// Get main forum. WIP
        /// </summary>
        /// <returns>Main forum</returns>
        public async Task<HttpResponseMessage> GetMainForum()
            => await GetData("forum").ConfigureAwait(false);

        /// <summary>
        /// Create a new conversation
        /// </summary>
        /// <param name="participants">Conversation participants as UserCollection</param>
        /// <param name="title">Title of the conversation</param>
        /// <param name="message">First message</param>
        /// <param name="openInvite">Bool indicating wether participants can invite others or not</param>
        /// <returns>ConversationInfo corresponding to the created conversation.</returns>
        public async Task<ConversationInfo> CreateConversation(UserCollection participants, string title, string message, bool openInvite = false)
            => await CreateConversation((from r in participants select r.Username).ToArray(), title, message, openInvite).ConfigureAwait(false);

        /// <summary>
        /// Returns an Url as a string to create a new conversation with the given <paramref name="participants"/> in a browser window.
        /// </summary>
        /// <param name="participants">Conversation participants as UserCollection</param>
        /// <returns>string containing the Url to creat a conversation.</returns>
        public string GetConversationCreationUrl(UserCollection participants) 
            => GetConversationCreationUrl(participants.Select(p => p.Username).ToArray());

        /// <summary>
        /// Returns an Url as a string to create a new conversation with the given <paramref name="participants"/> in a browser window.
        /// </summary>
        /// <param name="participants">Conversation participants as string[]</param>
        /// <returns>string containing the Url to creat a conversation.</returns>
        public string GetConversationCreationUrl(string[] participants) 
            => $"{ForumPaths.ConversationsUrl}add?to={string.Join(",", participants)}";

        /// <summary>
        /// Create a new conversation
        /// </summary>
        /// <param name="participants">Conversation participants as string[]</param>
        /// <param name="title">Title of the conversation</param>
        /// <param name="message">First message</param>
        /// <param name="openInvite">Bool indicating wether participants can invite others or not</param>
        /// <returns>ConversationInfo corresponding to the created conversation.</returns>
        public async Task<ConversationInfo> CreateConversation(string[] participants, string title, string message, bool openInvite = false)
        {
            if(!LoggedIn) throw new LoginRequiredException("Log in to create a new conversation!");
            var h = await GetData($"{ForumPaths.ConversationsPath}add").ConfigureAwait(false);
            var doc = new HtmlDocument();
            doc.LoadHtml(await h.Content.ReadAsStringAsync().ConfigureAwait(false));
            var xftoken = doc.DocumentNode.SelectSingleNode("//input[@name='_xfToken']").GetAttributeValue("value", "");

            var kvlist = new List<KeyValuePair<string, string>>
            {
                new KeyValuePair<string, string>("recipients", string.Join(',', participants)),
                new KeyValuePair<string, string>("title", title),
                new KeyValuePair<string, string>("message_html", message),
                new KeyValuePair<string, string>("open_invite", openInvite ? "1" : "0"),
                new KeyValuePair<string, string>("_xfToken", xftoken)
            };
            var hrm = await PostData($"{ForumPaths.ConversationsPath}insert", kvlist).ConfigureAwait(false);
            return await new ConversationInfoParser(this, await hrm.Content.ReadAsStringAsync().ConfigureAwait(false)).ParseAsync().ConfigureAwait(false);
        }

        /// <summary>
        /// Get information about the user with the specified ID.
        /// </summary>
        /// <param name="userId">The ID of the user</param>
        /// <returns>Corresponding UserInfo object to the <paramref name="userId"/> or null.</returns>
        public async Task<UserInfo> GetUserInfo(long userId) 
            => await new UserInfoParser(this, userId).ParseAsync().ConfigureAwait(false);

        //todo: throw exceptions
        public async Task<UserInfo> GetUserInfo(string username) {
            var h = await GetData(ForumPaths.MembersPath).ConfigureAwait(false);
            var doc = new HtmlDocument();
            doc.LoadHtml(await h.Content.ReadAsStringAsync().ConfigureAwait(false));
            var xftoken = doc.DocumentNode.SelectSingleNode("//form[@action='members/']/input[@name='_xfToken']").GetAttributeValue("value", "");

            var hrm = await PostData(ForumPaths.MembersPath, new List<KeyValuePair<string, string>> {
                new KeyValuePair<string, string>("username", username),
                new KeyValuePair<string, string>("_xfToken", xftoken)
            }, false).ConfigureAwait(false);
            if (!hrm.IsSuccessStatusCode) return null;
            try {
                return await new UserInfoParser(this, await hrm.Content.ReadAsStringAsync().ConfigureAwait(false)).ParseAsync().ConfigureAwait(false);
            }
            catch (NodeNotFoundException) {
                throw new UserNotFoundException();
            }
        }

        /// <summary>
        /// Returns a UserCollection object containing all UserInfo's from the forum's members list of the specified type.
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public async Task<UserCollection> GetMembersList(MembersListType type)
        {
            var hrm = await GetData(ForumPaths.GetMembersListTypePath(type)).ConfigureAwait(false);
            var doc = new HtmlDocument();
            doc.LoadHtml(await hrm.Content.ReadAsStringAsync().ConfigureAwait(false));
            var liNodes = doc.DocumentNode.SelectNodes(".//div[@class='section']/ol[@class='memberList']/li");
            var users = await new MembersListLiNodeParser(this, liNodes).ParseAsync().ConfigureAwait(false);
            return users.ToUserCollection();
        }

        /// <summary>
        /// Returns the amount of users currently being on the server.
        /// </summary>
        /// <returns></returns>
        public async Task<int> GetOnlineUserCount()
        {
            var hrm = await GetData(ForumPaths.StatsUsersOnlinePath).ConfigureAwait(false);
            var doc = new HtmlDocument();
            doc.LoadHtml(await hrm.Content.ReadAsStringAsync().ConfigureAwait(false));
            var numberString = string.Join("", doc.DocumentNode.SelectNodes("//dd/span").Select(node => node.InnerText));
            return int.Parse(numberString);
        }
    }
}

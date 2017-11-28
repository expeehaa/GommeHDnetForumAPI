using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Authentication;
using System.Threading.Tasks;
using GommeHDnetForumAPI.DataModels;
using GommeHDnetForumAPI.DataModels.Collections;
using GommeHDnetForumAPI.DataModels.Entities;
using GommeHDnetForumAPI.DataModels.Exceptions;
using GommeHDnetForumAPI.Parser;
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
        /// Base URL equals to https://www.gommehd.net/
        /// </summary>
        public static string BaseUrl => "https://www.gommehd.net/";
        /// <summary>
        /// Forum URL equals to https://www.gommehd.net/forum/
        /// </summary>
        public static string ForumUrl => BaseUrl + "forum/";

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
            if(HasCredentials) return await Login().ConfigureAwait(false);
            return false;
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
            _httpClient = new HttpClient(_httpClientHandler) { BaseAddress = new Uri(BaseUrl) };
            _httpClient.DefaultRequestHeaders.Add("Cache-Control", "no-cache");
            _httpClient.DefaultRequestHeaders.Add("Referer", BaseUrl);
            _httpClient.Timeout = TimeSpan.FromMinutes(1);
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
            var response = await _httpClient.PostAsync(BaseUrl + "login", fuec).ConfigureAwait(false);
            if (!response.IsSuccessStatusCode) return false;
            var html = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
            doc.LoadHtml(html);
            var urlpath = new ForumUrlPathString(doc.DocumentNode.SelectSingleNode("//div[@class='userbar']//a[@class='btn btn-link profile']").GetAttributeValue("href", ""));
            SelfUser = await new UserInfoParser(this, urlpath, true).ParseAsync().ConfigureAwait(false);
            return true;
        }

        /// <summary>
        /// HTTP post request to forum without login routing.
        /// </summary>
        /// <param name="path">Path after BaseURL</param>
        /// <param name="body">ForumUrlEncoded body</param>
        /// <param name="checkSuccess">Wether to call EnsureSuccessStatusCode or not on response</param>
        /// <exception cref="HttpRequestException">Thrown if status code != 200</exception>
        /// <returns>HttpResponseMessage</returns>
        public async Task<HttpResponseMessage> PostData(string path, List<KeyValuePair<string, string>> body = null, bool checkSuccess = true) {
            var fuec = new FormUrlEncodedContent(body);
            fuec.Headers.ContentType = MediaTypeHeaderValue.Parse("application/x-www-form-urlencoded");
            var response = await _httpClient.PostAsync(BaseUrl + path, fuec).ConfigureAwait(false);
            return checkSuccess ? response.EnsureSuccessStatusCode() : response;
        }

        /// <summary>
        /// HTTP get request to forum
        /// </summary>
        /// <param name="path">Path after BaseURL</param>
        /// <param name="checkSuccess">Wether to call EnsureSuccessStatusCode or not on response</param>
        /// <exception cref="HttpRequestException">Thrown if status code != 200</exception>
        /// <returns>HttpResponseMessage</returns>
        public async Task<HttpResponseMessage> GetData(string path, bool checkSuccess = true) {
            var response = await _httpClient.GetAsync(BaseUrl + path);
            return checkSuccess ? response.EnsureSuccessStatusCode() : response;
        }
        
        /// <summary>
        /// Get all conversations.
        /// </summary>
        /// <returns>Object containing all conversations the user had.</returns>
        public async Task<Conversations> GetConversations()
            => await new ConversationsParser(this, 0, 0).ParseAsync().ConfigureAwait(false);

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
            => GetConversationCreationUrl((from p in participants select p.Username).ToArray());

        /// <summary>
        /// Returns an Url as a string to create a new conversation with the given <paramref name="participants"/> in a browser window.
        /// </summary>
        /// <param name="participants">Conversation participants as string[]</param>
        /// <returns>string containing the Url to creat a conversation.</returns>
        public string GetConversationCreationUrl(string[] participants) 
            => ForumUrl + "conversations/add?to=" + participants.Aggregate("", (s, u) => $"{s}{u},", s => s.Length > 0 ? s.Substring(0, s.Length - 1) : s);

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
            var h = await GetData("forum/conversations/add").ConfigureAwait(false);
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
            var hrm = await PostData("forum/conversations/insert", kvlist).ConfigureAwait(false);
            return await new ConversationInfoParser(this, await hrm.Content.ReadAsStringAsync().ConfigureAwait(false)).ParseAsync().ConfigureAwait(false);
        }

        /// <summary>
        /// Get information about the user with the specified ID.
        /// </summary>
        /// <param name="userId">The ID of the user</param>
        /// <returns>Corresponding UserInfo object to the <paramref name="userId"/> or null.</returns>
        public async Task<UserInfo> GetUserInfo(long userId)
            => await new UserInfoParser(this, userId).ParseAsync().ConfigureAwait(false);

        public async Task<UserInfo> GetUserInfo(string username) {
            var h = await GetData("forum/members/").ConfigureAwait(false);
            var doc = new HtmlDocument();
            doc.LoadHtml(await h.Content.ReadAsStringAsync().ConfigureAwait(false));
            var xftoken = doc.DocumentNode.SelectSingleNode("//form[@action='members/']/input[@name='_xfToken']").GetAttributeValue("value", "");

            var hrm = await PostData("forum/members/", new List<KeyValuePair<string, string>> {
                new KeyValuePair<string, string>("username", username),
                new KeyValuePair<string, string>("_xfToken", xftoken)
            }, false).ConfigureAwait(false);
            if (!hrm.IsSuccessStatusCode) return null;
            return await new UserInfoParser(this, await hrm.Content.ReadAsStringAsync().ConfigureAwait(false)).ParseAsync().ConfigureAwait(false);
        }
    }
}

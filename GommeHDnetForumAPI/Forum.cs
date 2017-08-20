using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Authentication;
using System.Threading.Tasks;

namespace GommeHDnetForumAPI
{
    public class Forum
    {
        private readonly string _username;
        private readonly string _password;
        public string BaseUrl => "https://www.gommehd.net/";
        public string ForumUrl => BaseUrl + "forum/";

        public string UserAgent
        {
            get => _httpClient.DefaultRequestHeaders.UserAgent.ToString();
            set => _httpClient.DefaultRequestHeaders.Add("User-Agent", value);
        }

        private CookieContainer _cookieContainer;
        private HttpClientHandler _httpClientHandler;
        private HttpClient _httpClient;

        public Forum() : this(null, null) {}

        public Forum(string username, string password) {
            _username = username;
            _password = password;
            _cookieContainer = new CookieContainer();
            _httpClientHandler = new HttpClientHandler
            {
                CookieContainer = _cookieContainer,
                AllowAutoRedirect = true,
                UseCookies = true,
                ServerCertificateCustomValidationCallback = (message, certificate2, arg3, arg4) => true,
                SslProtocols = SslProtocols.Tls12
            };
            _httpClient = new HttpClient(_httpClientHandler) {BaseAddress = new Uri(BaseUrl)};
            _httpClient.DefaultRequestHeaders.Add("Cache-Control", "no-cache");
            _httpClient.DefaultRequestHeaders.Add("Referer", BaseUrl);
            UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/60.0.3112.101 Safari/537.36";
            GetHttpResponseMessage("forum", false).GetAwaiter().GetResult();
        }

        public bool HasCredentials() => _username != null && _password != null;

        public async Task<Conversations> GetConversations() => await ConversationsParser.ParseConversationsAsync(this);

        public async Task<HttpResponseMessage> GetBaseForum(bool withLogin = true) 
            => await GetHttpResponseMessage("forum", withLogin);

        public async Task<HttpResponseMessage> GetHttpResponseMessage(string redirect, bool withLogin = true) {
            if (!withLogin) return await _httpClient.GetAsync(BaseUrl + redirect);

            if (!HasCredentials()) throw new ArgumentException("One or both credentials missing!");
            var list = new List<KeyValuePair<string, string>>
            {
                new KeyValuePair<string, string>("login", _username),
                new KeyValuePair<string, string>("password", _password),
                new KeyValuePair<string, string>("cookie_check", "1"),
                new KeyValuePair<string, string>("_xfToken", ""),
                new KeyValuePair<string, string>("redirect", redirect)
            };
            var fuec = new FormUrlEncodedContent(list);
            fuec.Headers.ContentType = MediaTypeHeaderValue.Parse("application/x-www-form-urlencoded");
            return await _httpClient.PostAsync(ForumUrl + "login/login", fuec);
        }
    }
}

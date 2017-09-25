namespace GommeHDnetForumAPI.DataModels
{
    public class ForumRedirectUrlString
    {
        //private static string UrlPattern => "(\\S+)\\/(\\S+)\\/?";

        public string RedirectUrl { get; }

        public ForumRedirectUrlString(string url)
        {
            //if (Regex.IsMatch(url, UrlPattern))
            //    throw new ArgumentException($"Argument 'url' not matching pattern '{UrlPattern}'!");
            RedirectUrl = url;
        }

        public static implicit operator ForumRedirectUrlString(string url) => new ForumRedirectUrlString(url);
        public static implicit operator string(ForumRedirectUrlString frust) => frust.RedirectUrl;

        public override string ToString() 
            => RedirectUrl;
    }
}
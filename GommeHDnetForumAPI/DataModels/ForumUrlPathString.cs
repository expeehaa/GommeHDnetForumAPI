namespace GommeHDnetForumAPI.DataModels
{
    public class ForumUrlPathString
    {
        //private static string UrlPattern => "(\\S+)\\/(\\S+)\\/?";

        public string RedirectUrl { get; }

        public ForumUrlPathString(string url)
        {
            //if (Regex.IsMatch(url, UrlPattern))
            //    throw new ArgumentException($"Argument 'url' not matching pattern '{UrlPattern}'!");
            RedirectUrl = url;
        }

        public static implicit operator ForumUrlPathString(string url) => new ForumUrlPathString(url);
        public static implicit operator string(ForumUrlPathString frust) => frust.RedirectUrl;

        public override string ToString() 
            => RedirectUrl;
    }
}
namespace GommeHDnetForumAPI.DataModels.Entities
{
    public class UrlEntity
    {
        protected Forum Forum { get; }
        protected string UrlPath { get; }

        internal UrlEntity(Forum forum, string urlPath) {
            Forum = forum;
            UrlPath = urlPath;
        }

        public override string ToString() 
            => $"Url: \"{UrlPath}\"";
    }
}

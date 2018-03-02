using GommeHDnetForumAPI.DataModels.Entities.Interfaces;

namespace GommeHDnetForumAPI.DataModels.Entities
{
    public class UrlEntity : IUrlPath
    {
        public Forum Forum { get; }
        public string UrlPath { get; }

        internal UrlEntity(Forum forum, string urlPath) {
            Forum = forum;
            UrlPath = urlPath;
        }

        public override string ToString() 
            => $"Url: \"{UrlPath}\"";
    }
}

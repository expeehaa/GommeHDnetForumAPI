namespace GommeHDnetForumAPI.DataModels.Entities
{
    public class Entity
    {
        public long Id { get; }
        public ForumUrlPathString UrlPath { get; }

        public Entity(long id, ForumUrlPathString urlPath)
        {
            Id = id;
            UrlPath = urlPath;
        }

        public override string ToString() 
            => $"Id: {Id}, Url: \"{UrlPath}\"";
    }
}

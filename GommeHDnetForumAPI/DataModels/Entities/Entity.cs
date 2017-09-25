namespace GommeHDnetForumAPI.DataModels.Entities
{
    public class Entity
    {
        public long Id { get; }
        public ForumRedirectUrlString Url { get; }

        public Entity(long id, ForumRedirectUrlString url)
        {
            Id = id;
            Url = url;
        }

        public override string ToString() 
            => $"Id: {Id}, Url: \"{Url}\"";
    }
}

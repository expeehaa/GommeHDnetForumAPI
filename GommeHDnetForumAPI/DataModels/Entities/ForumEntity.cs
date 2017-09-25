namespace GommeHDnetForumAPI.DataModels.Entities
{
    public class ForumEntity : Entity
    {
        protected Forum Forum { get; }

        public ForumEntity(Forum forum, long id, ForumRedirectUrlString url) : base(id, url)
        {
            Forum = forum;
        }
    }
}

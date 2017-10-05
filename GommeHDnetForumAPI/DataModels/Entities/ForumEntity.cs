namespace GommeHDnetForumAPI.DataModels.Entities
{
    public class ForumEntity : Entity
    {
        protected Forum Forum { get; }

        public ForumEntity(Forum forum, long id, ForumUrlPathString urlPath) : base(id, urlPath)
        {
            Forum = forum;
        }
    }
}

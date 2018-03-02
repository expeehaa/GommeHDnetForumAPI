using GommeHDnetForumAPI.DataModels.Entities.Interfaces;

namespace GommeHDnetForumAPI.DataModels.Entities
{
    public class ForumPost : IndexedEntity, IPost<ForumThread>
    {
        public UserInfo Author { get; }
        public string Content { get; }
        public ForumThread Parent { get; }
        public string UrlPath => $"{Parent.UrlPath}#post-{Id}";

        internal ForumPost(Forum forum, long id, UserInfo author, string content, ForumThread parent) : base(forum, id) {
            Author = author;
            Content = content;
            Parent = parent;
        }

        public override string ToString()
            => $"Id: {Id} | Author: ({Author}) | Content: {Content}";
    }
}
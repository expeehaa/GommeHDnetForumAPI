using GommeHDnetForumAPI.DataModels.Entities.Interfaces;

namespace GommeHDnetForumAPI.DataModels.Entities
{
    public class ConversationMessage : IndexedEntity, IPost<ConversationInfo>
    {
        public UserInfo Author { get; }
        public string Content { get; }
        public ConversationInfo Parent { get; }
        public string UrlPath => $"{Parent.UrlPath}message?message_id={Id}";

        internal ConversationMessage(Forum forum, long id, UserInfo author, string content, ConversationInfo parent) : base(forum, id)
        {
            Author = author;
            Content = content;
            Parent = parent;
        }

        public override string ToString() 
            => $"Id: {Id} | MsgAuthor: {Author.Username} | Content: \"{Content}\"";
    }
}

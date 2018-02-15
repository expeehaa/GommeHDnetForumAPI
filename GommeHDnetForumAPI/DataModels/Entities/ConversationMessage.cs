namespace GommeHDnetForumAPI.DataModels.Entities
{
    public class ConversationMessage : IndexedEntity
    {
        public UserInfo Author { get; }
        public string Content { get; }

        internal ConversationMessage(Forum forum, long id, UserInfo author, string content) : base(forum, id)
        {
            Author = author;
            Content = content;
        }

        public override string ToString() 
            => $"Id: {Id} | MsgAuthor: {Author.Username} | Content: \"{Content}\"";
    }
}

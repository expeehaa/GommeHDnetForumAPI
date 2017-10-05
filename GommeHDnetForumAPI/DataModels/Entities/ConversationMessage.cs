namespace GommeHDnetForumAPI.DataModels.Entities
{
    public class ConversationMessage
    {
        public long Id { get; }
        public UserInfo Author { get; }
        public string Content { get; }

        public ConversationMessage(long id, UserInfo author, string content)
        {
            Id = id;
            Author = author;
            Content = content;
        }

        public override string ToString() 
            => $"Id: {Id} | MsgAuthor: {Author.Username} | Content: \"{Content}\"";
    }
}

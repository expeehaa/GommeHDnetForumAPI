namespace GommeHDnetForumAPI.Conversation
{
    public class ConversationMessage
    {
        public long Id { get; }
        public string Author { get; }
        public string Content { get; }

        public ConversationMessage(long id, string author, string content)
        {
            Id = id;
            Author = author;
            Content = content;
        }

        public override string ToString()
        {
            return $"Id: {Id} | MsgAuthor: {Author} | Content: {Content}";
        }
    }
}

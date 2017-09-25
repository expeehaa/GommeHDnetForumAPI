using System.Threading.Tasks;
using GommeHDnetForumAPI.DataModels.Collections;
using GommeHDnetForumAPI.Parser;

namespace GommeHDnetForumAPI.DataModels.Entities
{
    public class ConversationInfo : ForumEntity
    {
        /// <summary>
        /// Conversation title
        /// </summary>
        public string Title { get; }
        /// <summary>
        /// Conversation author
        /// </summary>
        public UserInfo Author { get; }
        /// <summary>
        /// Conversation members (including author)
        /// </summary>
        public UserCollection Members { get; }
        /// <summary>
        /// Number of answers
        /// </summary>
        public uint AnswerCount { get; }
        /// <summary>
        /// Message collection
        /// </summary>
        public ConversationMessages Messages { get; private set; }

        /// <summary>
        /// Internal constructor
        /// </summary>
        /// <param name="forum">Forum instance this object is assigned to.</param>
        /// <param name="id">Conversation ID</param>
        /// <param name="title">Conversation title</param>
        /// <param name="url">Conversation url relative to Forum.BaseUrl</param>
        /// <param name="author">Conversation author</param>
        /// <param name="members">Conversation members (including author)</param>
        /// <param name="answerCount">Number of answers</param>
        internal ConversationInfo(Forum forum, long id, string title, string url, UserInfo author, UserCollection members, uint answerCount) : base(forum, id, url)
        {
            Title = title;
            Author = author;
            Members = members;
            AnswerCount = answerCount;
            Messages = null;
        }

        /// <summary>
        /// Downloads all messages in a conversation.
        /// </summary>
        public async Task DownloadMessagesAsync() => await DownloadMessagesAsync(1);

        /// <summary>
        /// Downloads conversation messages in a range of pages.
        /// </summary>
        /// <param name="startPage">First page, starting with 1.</param>
        /// <param name="pageCount">Number of pages. If 0 or less all pages from <paramref name="startPage"/> to last will be downloaded.</param>
        public async Task DownloadMessagesAsync(int startPage, int pageCount = 0)
        {
            Messages = await new ConversationMessageParser(Forum, Url, startPage, pageCount).ParseAsync();
        }

        public override string ToString() {
            return $"Id: {Id} | Title: {Title} | Author: {Author} | Members: ({string.Join(",", Members)}) | Answers: {AnswerCount}";
        }
    }
}

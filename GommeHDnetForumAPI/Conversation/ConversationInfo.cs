using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GommeHDnetForumAPI.Parser;

namespace GommeHDnetForumAPI.Conversation
{
    public class ConversationInfo
    {
        private Forum Forum { get; }
        public long Id { get; }
        public string Title { get; }
        public string Url { get; }
        public string Author { get; }
        public IEnumerable<string> Members { get; }
        public uint AnswerCount { get; }
        public ConversationMessages Messages { get; private set; }
        private bool _downloadLocked;

        internal ConversationInfo(Forum forum, long id, string title, string url, string author, IEnumerable<string> members, uint answerCount)
        {
            Forum = forum;
            Id = id;
            Title = title;
            Url = url;
            Author = author;
            Members = members;
            AnswerCount = answerCount;
            Messages = null;
            _downloadLocked = false;
        }

        private ConversationInfo(ConversationInfo ci) : this(ci.Forum, ci.Id, ci.Title, ci.Url, ci.Author, ci.Members, ci.AnswerCount)
        {}

        public async Task<ConversationMessages> DownloadMessagesAsync(int startPage = 0, int pageOffset = 0)
        {
            return _downloadLocked ? null : (Messages = await new ConversationMessageParser(Forum, CopyAndLockDownload()).ParseAsync());
        }

        private ConversationInfo CopyAndLockDownload()
        {
            var ci = new ConversationInfo(this) {_downloadLocked = true};
            return ci;
        }

        public override string ToString() {
            return $"Id: {Id} | Title: {Title} | Author: {Author} | Members: ({string.Join(",", Members)}) | Answers: {AnswerCount}";
        }

        
    }
}

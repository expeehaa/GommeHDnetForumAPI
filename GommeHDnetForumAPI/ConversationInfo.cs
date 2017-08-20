using System.Collections.Generic;

namespace GommeHDnetForumAPI
{
    public class ConversationInfo
    {
        public long Id { get; }
        public string Title { get; }
        public string Url { get; }
        public string Author { get; }
        public IEnumerable<string> Members { get; }
        public uint AnswerCount { get; }

        public ConversationInfo(long id, string title, string url, string author, IEnumerable<string> members, uint answerCount) {
            Id = id;
            Title = title;
            Url = url;
            Author = author;
            Members = members;
            AnswerCount = answerCount;
        }

        public override string ToString() {
            return $"Id: {Id} | Title: {Title} | Author: {Author} | Members: ({string.Join(",", Members)}) | Answers: {AnswerCount}";
        }
    }
}

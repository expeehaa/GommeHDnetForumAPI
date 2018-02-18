using System.Collections.Generic;
using System.Threading.Tasks;

namespace GommeHDnetForumAPI.DataModels.Entities
{
    public class ForumThread : IndexedEntity, IThread<ForumPost>
    {
        public string Title { get; }
        public UserInfo Author { get; }
        public IEnumerable<ForumPost> Messages { get; private set; }
        public ThreadPrefix Prefix { get; }
        public SubForum Parent { get; }
        public string UrlPath => $"{ForumPaths.ForumThreadsPath}{Id}/";

        internal ForumThread(Forum forum, long id, string title, UserInfo author, SubForum parent, ThreadPrefix prefix = null) : base(forum, id) {
            Title = title;
            Author = author;
            Parent = parent;
            Prefix = prefix;
        }

        public Task DownloadMessagesAsync()
        {
            throw new System.NotImplementedException();
        }

        public override string ToString()
            => $"Id: {Id} | {(Prefix == null ? $"Title: {Title}" : $"(P)Title: ({Prefix}){Title}")} | Author: {Author.Username} | Parent: {Parent.Title}";
    }
}
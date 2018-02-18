using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GommeHDnetForumAPI.DataModels.Collections;
using GommeHDnetForumAPI.Parser;

namespace GommeHDnetForumAPI.DataModels.Entities
{
    public class SubForum : IndexedEntity, ISubForum
    {
        public string Title { get; private set; }
        public string Description { get; private set; }
        public IForum Parent { get; }
        public long? PostCount { get; private set; }
        public IEnumerable<ISubForum> SubForums { get; internal set; }
        public ForumThreadCollection Threads { get; internal set; }
        public IEnumerable<ThreadPrefix> Prefixes { get; internal set; }
        public long? ThreadCount => Threads?.Count;
        public long? SubForumCount => SubForums?.Count();

        public string UrlPath => $"{ForumPaths.ForumsPath}{Id}/";
        public string RssFeed => $"{UrlPath}index.rss";
        public IEnumerable<SubForum> RealSubForums => SubForums?.Select(sf => sf as SubForum).Where(sf => sf != null);
        public IEnumerable<SubLink> SubLinks => SubForums?.Select(sl => sl as SubLink).Where(sl => sl != null);
        
        internal SubForum(Forum forum, long id, IForum parent, string title, string description, long? postCount) : base(forum, id) {
            Title = title;
            Description = description;
            PostCount = postCount;
            Parent = parent;
        }

        public async Task DownloadDataAsync()
            => await DownloadDataAsync(1).ConfigureAwait(false);

        public async Task DownloadDataAsync(int startPage, int pageCount = 0) {
            var sf = await new SubForumParser(this, 1, 0).ParseAsync().ConfigureAwait(false);
            Title = sf.Title;
            Description = sf.Description;
            PostCount = sf.PostCount;
            SubForums = sf.SubForums;
            Threads = sf.Threads;
            Prefixes = sf.Prefixes;
        }

        public override string ToString()
            => $"({Title}({Id}) | {ThreadCount} | {PostCount} | {SubForumCount})";
    }
}
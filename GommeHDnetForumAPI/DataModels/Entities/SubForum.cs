namespace GommeHDnetForumAPI.DataModels.Entities
{
    public class SubForum : IndexedEntity, ISubForum
    {
        public string Title { get; }
        public string Description { get; }
        public MasterForumCategoryInfo ParentCategory { get; }
        public long? ThreadCount { get; }
        public long? PostCount { get; }
        public long SubForumCount { get; }

        public string UrlPath => $"{ForumPaths.ForumPath}{Id}/";
        public string RssFeed => $"{UrlPath}index.rss";

        internal SubForum(Forum forum, long id, MasterForumCategoryInfo parentCategory, string title, string description, long? threadCount, long? postCount, long subForumCount) : base(forum, id) {
            Title = title;
            Description = description;
            ThreadCount = threadCount;
            PostCount = postCount;
            SubForumCount = subForumCount;
            ParentCategory = parentCategory;
        }

        public override string ToString()
            => $"({Title}({Id}) | {ThreadCount} | {PostCount} | {SubForumCount})";
    }
}
namespace GommeHDnetForumAPI.DataModels.Entities
{
    public class SubLink : IndexedEntity, ISubForum
    {
        /// <summary>
        /// Title of the SubLink
        /// </summary>
        public string Title { get; }

        /// <summary>
        /// Description of the SubLink
        /// </summary>
        public string Description { get; }

        public MasterForumCategoryInfo ParentCategory { get; }

        /// <summary>
        /// Urlpath to sublink
        /// </summary>
        public string UrlPath => $"{ForumPaths.LinkForumsPath}{Id}/";

        internal SubLink(Forum forum, long id, MasterForumCategoryInfo parentCategory, string title, string description) : base(forum, id) {
            Title = title;
            Description = description;
            ParentCategory = parentCategory;
        }

        public override string ToString()
            => $"({Title}({Id}) | {Description})";
    }
}
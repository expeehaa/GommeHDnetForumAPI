using GommeHDnetForumAPI.DataModels.Collections;

namespace GommeHDnetForumAPI.DataModels.Entities
{
    public class MasterForumCategoryInfo : IndexedEntity
    {
        public string Title { get; }
        public string Description { get; }
        public DataModelCollection<ISubForum> SubForums { get; internal set; }

        internal MasterForumCategoryInfo(Forum forum, long id, string title, string description, DataModelCollection<ISubForum> subForums = null) : base(forum, id) {
            Title = title;
            Description = description;
            SubForums = subForums ?? new DataModelCollection<ISubForum>();
        }

        public override string ToString()
            => $"{Title}: {string.Join(", ", SubForums)}";
    }
}
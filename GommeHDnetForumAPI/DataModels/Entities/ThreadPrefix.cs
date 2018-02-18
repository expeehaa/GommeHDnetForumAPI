namespace GommeHDnetForumAPI.DataModels.Entities
{
    public class ThreadPrefix : IndexedEntity
    {
        public string Name { get; }

        internal ThreadPrefix(Forum forum, long id, string name) : base(forum, id) {
            Name = name;
        }
    }
}
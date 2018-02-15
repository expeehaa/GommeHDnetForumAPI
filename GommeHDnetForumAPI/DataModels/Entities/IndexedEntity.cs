namespace GommeHDnetForumAPI.DataModels.Entities
{
    public class IndexedEntity
    {
        protected Forum Forum { get; }
        protected long Id { get; }

        internal IndexedEntity(Forum forum, long id) {
            Forum = forum;
            Id = id;
        }

        public override string ToString()
            => $"Id: {Id}";
    }
}

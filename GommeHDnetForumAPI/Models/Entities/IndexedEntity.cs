namespace GommeHDnetForumAPI.Models.Entities {
	public class IndexedEntity {
		public Forum Forum { get; }
		public long  Id    { get; }

		internal IndexedEntity(Forum forum, long id) {
			Forum = forum;
			Id    = id;
		}

		public override string ToString()
			=> $"Id: {Id}";
	}
}
using GommeHDnetForumAPI.Models.Entities.Interfaces;

namespace GommeHDnetForumAPI.Models.Entities {
	public class SubLink : IndexedEntity, ISubForum {
		public string Title       { get; }
		public string Description { get; }
		public IForum Parent      { get; }
		public string UrlPath     => $"{ForumPaths.LinkForumsPath}{Id}/";

		internal SubLink(Forum forum, long id, IForum parent, string title, string description) : base(forum, id) {
			Title       = title;
			Description = description;
			Parent      = parent;
		}

		public override string ToString()
			=> $"({Title}({Id}) | {Description})";
	}
}
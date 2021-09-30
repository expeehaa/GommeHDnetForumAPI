using GommeHDnetForumAPI.Models.Entities.Interfaces;

namespace GommeHDnetForumAPI.Models.Entities {
	public class SubLink : IndexedEntity, ISubForum {
		public string Title       { get; }
		public IForum Parent      { get; }
		public string UrlPath     => $"{ForumPaths.LinkForumsPath}{Id}/";
		// link-forums do not have (visible) descriptions anymore.
		public string Description => null;

		internal SubLink(Forum forum, long id, IForum parent, string title) : base(forum, id) {
			Title       = title;
			Parent      = parent;
		}

		public override string ToString()
			=> $"({Title}({Id}) | {UrlPath})";
	}
}
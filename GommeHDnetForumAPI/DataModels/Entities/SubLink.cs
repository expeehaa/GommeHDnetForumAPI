using GommeHDnetForumAPI.DataModels.Entities.Interfaces;

namespace GommeHDnetForumAPI.DataModels.Entities {
	public class SubLink : IndexedEntity, ISubForum {
		/// <summary>
		/// Title of the SubLink
		/// </summary>
		public string Title { get; }

		/// <summary>
		/// Description of the SubLink
		/// </summary>
		public string Description { get; }

		/// <summary>
		/// Parent implementing IForum
		/// </summary>
		public IForum Parent { get; }

		/// <summary>
		/// Urlpath to sublink
		/// </summary>
		public string UrlPath => $"{ForumPaths.LinkForumsPath}{Id}/";

		internal SubLink(Forum forum, long id, IForum parent, string title, string description) : base(forum, id) {
			Title       = title;
			Description = description;
			Parent      = parent;
		}

		public override string ToString()
			=> $"({Title}({Id}) | {Description})";
	}
}
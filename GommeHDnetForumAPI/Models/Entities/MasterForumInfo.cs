using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GommeHDnetForumAPI.Parser;

namespace GommeHDnetForumAPI.Models.Entities {
	public class MasterForumInfo : UrlEntity {
		public IEnumerable<MasterForumCategoryInfo> Categories   { get; private set; }
		public IEnumerable<SubForum>                SubForums    => Categories?.SelectMany(c => c.RealSubForums).ToList();
		public IEnumerable<SubLink>                 SubLinks     => Categories?.SelectMany(c => c.SubLinks).ToList();
		public IEnumerable<SubForum>                AllSubForums => Categories?.SelectMany(c => c.AllRealSubForums).ToList();

		internal MasterForumInfo(Forum forum) : base(forum, ForumPaths.ForumUrl) { }

		internal MasterForumInfo(Forum forum, IEnumerable<MasterForumCategoryInfo> categories) : this(forum) {
			Categories = categories;
		}

		public async Task DownloadData() {
			var doc = await Forum.GetHtmlDocument(ForumPaths.ForumPath);
			var mfi = new MasterForumParser(Forum).Parse(doc.DocumentNode);

			Categories = mfi.Categories;
		}

		public override string ToString()
			=> string.Join("\n", Categories);
	}
}
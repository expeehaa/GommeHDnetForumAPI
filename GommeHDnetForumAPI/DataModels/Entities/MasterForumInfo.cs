using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GommeHDnetForumAPI.Parser;

namespace GommeHDnetForumAPI.DataModels.Entities
{
    public class MasterForumInfo : UrlEntity
    {
        public IEnumerable<MasterForumCategoryInfo> Categories { get; private set; }
        public IEnumerable<SubForum> SubForums => Categories?.SelectMany(c => c.RealSubForums);
        public IEnumerable<SubLink> SubLinks => Categories?.SelectMany(c => c.SubLinks);

        internal MasterForumInfo(Forum forum) : base(forum, ForumPaths.ForumUrl) { }

        internal MasterForumInfo(Forum forum, IEnumerable<MasterForumCategoryInfo> categories) : this(forum) {
            Categories = categories;
        }

        public async Task DownloadData() {
            var mfi = await new MasterForumParser(Forum).ParseAsync().ConfigureAwait(false);
            Categories = mfi.Categories;
        }

        public override string ToString()
            => string.Join("\n", Categories);
    }
}
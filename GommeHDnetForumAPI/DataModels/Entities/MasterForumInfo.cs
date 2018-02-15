using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GommeHDnetForumAPI.DataModels.Collections;
using GommeHDnetForumAPI.Parser;

namespace GommeHDnetForumAPI.DataModels.Entities
{
    public class MasterForumInfo : UrlEntity
    {
        public DataModelCollection<MasterForumCategoryInfo> Categories { get; private set; }
        public IEnumerable<ISubForum> SubForums => Categories.SelectMany(c => c.SubForums);

        internal MasterForumInfo(Forum forum) : base(forum, ForumPaths.ForumUrl) { }

        internal MasterForumInfo(Forum forum, DataModelCollection<MasterForumCategoryInfo> categories) : this(forum) {
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
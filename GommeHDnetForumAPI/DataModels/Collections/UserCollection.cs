using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GommeHDnetForumAPI.DataModels.Entities;

namespace GommeHDnetForumAPI.DataModels.Collections
{
    public class UserCollection : List<UserInfo>
    {
        public UserCollection()
        {
        }

        public UserCollection(IEnumerable<UserInfo> collection) : base(collection)
        {
        }

        public UserCollection(int capacity) : base(capacity)
        {
        }

        public async Task DownloadDataAsync()
        {
            foreach (var userInfo in this)
                await userInfo.DownloadDataAsync().ConfigureAwait(false);
        }

        public override string ToString()
            => string.Join(", ", this.Select(u => $"({u.ToString()})"));
    }
}

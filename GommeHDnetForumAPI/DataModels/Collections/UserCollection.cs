using System.Collections.Generic;
using System.Threading.Tasks;
using GommeHDnetForumAPI.DataModels.Entities;

namespace GommeHDnetForumAPI.DataModels.Collections
{
    public class UserCollection : DataModelCollection<UserInfo>
    {
        public async Task DownloadDataAsync()
        {
            foreach (var userInfo in Items)
            {
                await userInfo.DownloadDataAsync();
            }
        }

        public UserCollection() { }
        public UserCollection(DataModelCollection<UserInfo> items) : base(items) { }
        public UserCollection(IEnumerable<UserInfo> items) : base(items) { }
    }
}

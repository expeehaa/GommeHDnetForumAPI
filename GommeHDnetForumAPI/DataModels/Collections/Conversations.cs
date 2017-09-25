using System.Threading.Tasks;
using GommeHDnetForumAPI.DataModels.Entities;

namespace GommeHDnetForumAPI.DataModels.Collections
{
    public class Conversations : DataModelCollection<ConversationInfo>
    {
        public async Task DownloadDataAsync()
        {
            foreach (var conversationInfo in Items)
            {
                await conversationInfo.DownloadMessagesAsync();
            }
        }
    }
}

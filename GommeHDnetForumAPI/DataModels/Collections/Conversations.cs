﻿using System.Collections.Generic;
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
                await conversationInfo.DownloadMessagesAsync().ConfigureAwait(false);
            }
        }

        public Conversations() { }
        public Conversations(DataModelCollection<ConversationInfo> items) : base(items) { }
        public Conversations(IEnumerable<ConversationInfo> items) : base(items) { }
    }
}

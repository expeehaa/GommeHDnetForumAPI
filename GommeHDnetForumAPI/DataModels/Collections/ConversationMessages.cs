using System.Collections.Generic;
using GommeHDnetForumAPI.DataModels.Entities;

namespace GommeHDnetForumAPI.DataModels.Collections
{
    public class ConversationMessages : DataModelCollection<ConversationMessage>
    {
        public ConversationMessages() { }
        public ConversationMessages(DataModelCollection<ConversationMessage> items) : base(items) { }
        public ConversationMessages(IEnumerable<ConversationMessage> items) : base(items) { }
    }
}

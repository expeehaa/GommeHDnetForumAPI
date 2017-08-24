﻿using System.Collections.ObjectModel;

namespace GommeHDnetForumAPI.Conversation
{
    public class ConversationMessages : Collection<ConversationMessage>
    {
        public override string ToString()
        {
            var s = "";
            for (var i = 0; i < Count; i++) {
                s += i + ": (" + Items[i] + ")\n";
            }
            return s;
        }
    }
}

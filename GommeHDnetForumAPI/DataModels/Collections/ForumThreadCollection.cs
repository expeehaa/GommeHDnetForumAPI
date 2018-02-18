using System.Collections.Generic;
using System.Linq;
using GommeHDnetForumAPI.DataModels.Entities;

namespace GommeHDnetForumAPI.DataModels.Collections
{
    public class ForumThreadCollection : List<ForumThread>
    {
        public ForumThreadCollection()
        {
        }

        public ForumThreadCollection(IEnumerable<ForumThread> collection) : base(collection)
        {
        }

        public ForumThreadCollection(int capacity) : base(capacity)
        {
        }

        public ForumThreadCollection FilterByPrefix(ThreadPrefix prefix) {
            return prefix.Id == 0 ? this : new ForumThreadCollection(this.Where(ft => ft.Prefix.Id == prefix.Id));
        }

        public override string ToString()
            => string.Join(", ", this.Select(t => $"({t.ToString()})"));
    }
}
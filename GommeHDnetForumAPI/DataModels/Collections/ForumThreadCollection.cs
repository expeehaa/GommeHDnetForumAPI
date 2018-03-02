using System.Collections.Generic;
using System.Linq;
using GommeHDnetForumAPI.DataModels.Entities;

namespace GommeHDnetForumAPI.DataModels.Collections
{
    public static class ForumThreadCollection
    {

        public static IEnumerable<ForumThread> FilterByPrefix(this IEnumerable<ForumThread> threads, ThreadPrefix prefix) {
            return prefix.Id == 0 ? threads : threads.Where(ft => ft.Prefix.Id == prefix.Id);
        }

        public static string ThreadsToString(this IEnumerable<ForumThread> threads)
            => string.Join(", ", threads.Select(t => $"({t.ToString()})"));
    }
}
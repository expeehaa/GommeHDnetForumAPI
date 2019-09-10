using System.Collections.Generic;
using System.Linq;
using GommeHDnetForumAPI.DataModels.Entities;

namespace GommeHDnetForumAPI.DataModels.Collections {
	public static class CollectionExtensions {
		public static IEnumerable<ForumThread> FilterByPrefix(this IEnumerable<ForumThread> threads, ThreadPrefix prefix)
			=> prefix.Id == 0 ? threads : threads.Where(ft => ft.Prefix.Id == prefix.Id);

		public static string ThreadsToString(this IEnumerable<ForumThread> threads)
			=> string.Join(", ", threads.Select(t => $"({t})"));

		public static UserCollection ToUserCollection(this IEnumerable<UserInfo> userInfos)
			=> new UserCollection(userInfos);
	}
}
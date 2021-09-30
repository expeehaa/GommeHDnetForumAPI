using GommeHDnetForumAPI.Models;

namespace GommeHDnetForumAPI {
	public static class ForumPaths {
		public const string BaseUrl       = "https://www.gommehd.net/";
		public const string GraphQLApiUrl = "https://api.gommehd.net/graphql";

		public const string ForumPath                 = "forum/";
		public const string ForumLoginPath            = "forum/login/";
		public const string ForumConnectedAccountPath = "forum/connected_account.php";
		public const string ConversationsPath         = "forum/conversations/";
		public const string MembersPath               = "forum/members/";
		public const string LinkForumsPath            = "forum/link-forums/";
		public const string ForumsPath                = "forum/forums/";
		public const string ForumThreadsPath          = "forum/threads/";
		public const string StatsUsersOnlinePath      = "stats/users-online/";
		public const string NotificationsPath         = "xen-forum/notifications";

		public const string ForumUrl                 = BaseUrl + ForumPath;
		public const string ConversationsUrl         = BaseUrl + ConversationsPath;
		public const string MembersUrl               = BaseUrl + MembersPath;

		public static string GetMembersListTypePath(MembersListType type)
			=> $"{MembersPath}?key={type.GetStringValue()}";
	}
}
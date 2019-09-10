using GommeHDnetForumAPI.DataModels;

namespace GommeHDnetForumAPI {
	public static class ForumPaths {
		public const string BaseUrl = "https://www.gommehd.net/";

		public const string ForumPath            = "forum/";
		public const string ConversationsPath    = "forum/conversations/";
		public const string MembersPath          = "forum/members/";
		public const string LinkForumsPath       = "forum/link-forums/";
		public const string ForumsPath           = "forum/forums/";
		public const string ForumThreadsPath     = "forum/threads/";
		public const string StatsUsersOnlinePath = "stats/users-online/";

		public const string ForumUrl         = BaseUrl + ForumPath;
		public const string ConversationsUrl = BaseUrl + ConversationsPath;
		public const string MembersUrl       = BaseUrl + MembersPath;

		public static string GetMembersListTypePath(MembersListType type)
			=> $"{MembersPath}?type={type.ToString().ToLower()}";
	}
}
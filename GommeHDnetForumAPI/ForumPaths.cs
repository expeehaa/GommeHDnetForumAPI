﻿using GommeHDnetForumAPI.DataModels;

namespace GommeHDnetForumAPI
{
    public static class ForumPaths
    {
        /// <summary>
        /// Base URL equals to https://www.gommehd.net/
        /// </summary>
        public const string BaseUrl = "https://www.gommehd.net/";

        /// <summary>
        /// Forum URL equals to https://www.gommehd.net/forum/
        /// </summary>
        public const string ForumUrl = BaseUrl + ForumPath;

        /// <summary>
        /// Conversations URL equals to https://www.gommehd.net/forum/conversations/
        /// </summary>
        public const string ConversationsUrl = BaseUrl + ConversationsPath;

        /// <summary>
        /// Members Url equals to https://www.gommehd.net/forum/members/
        /// </summary>
        public const string MembersUrl = BaseUrl + MembersPath;

        /// <summary>
        /// Forum Path equals to forum/
        /// </summary>
        public const string ForumPath = "forum/";

        /// <summary>
        /// Conversations Path equals to forum/conversations/
        /// </summary>
        public const string ConversationsPath = ForumPath + "conversations/";

        /// <summary>
        /// Members path equals to forum/members/
        /// </summary>
        public const string MembersPath = ForumPath + "members/";

        /// <summary>
        /// Link Forums Path equals to forum/link-forums/
        /// </summary>
        public const string LinkForumsPath = ForumPath + "link-forums/";

        /// <summary>
        /// Forums path equals to forum/forums/
        /// </summary>
        public const string ForumsPath = ForumPath + "forums/";

        /// <summary>
        /// Forum threads path equals to forum/threads/
        /// </summary>
        public const string ForumThreadsPath = ForumPath + "threads/";
        
        /// <summary>
        /// Returns the members list path with appended type query
        /// </summary>
        /// <param name="type">The type of the members list</param>
        /// <returns>String representing the members list path with specified type query.</returns>
        public static string GetMembersListTypePath(MembersListType type) 
            => $"{MembersPath}?type={type.ToString().ToLower()}";

        /// <summary>
        /// Users online stats path equals to stats/users-online/
        /// </summary>
        public const string StatsUsersOnlinePath = "stats/users-online/";
    }
}
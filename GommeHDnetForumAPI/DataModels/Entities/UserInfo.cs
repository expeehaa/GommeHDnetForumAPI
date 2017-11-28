﻿using System;
using System.Threading.Tasks;
using GommeHDnetForumAPI.Parser;

namespace GommeHDnetForumAPI.DataModels.Entities
{
    public class UserInfo : ForumEntity
    {
        public string Username { get; internal set; }
        public string AvatarUrl { get; internal set; }
        public DateTime? TimeRegistered { get; internal set; }
        public int? PostCount { get; internal set; }
        public int? LikeCount { get; internal set; }
        public string Location { get; internal set; }
        public string Status { get; internal set; }
        public Gender Gender { get; internal set; } = Gender.Unknown;
        public bool? Verified { get; internal set; }
        public int? Trophies { get; internal set; }

        /// <summary>
        /// Internal constructor
        /// </summary>
        /// <param name="forum">Forum instance</param>
        /// <param name="id">User ID</param>
        internal UserInfo(Forum forum, long id) : base(forum, id, new ForumUrlPathString(Forum.ForumUrl + "members/" + id)) {
        }

        internal UserInfo(Forum forum, long id, string username) : base(forum, id, new ForumUrlPathString(Forum.ForumUrl + "members/" + id)) {
            Username = username;
        }

        /// <summary>
        /// Downloads available user data
        /// </summary>
        public async Task DownloadDataAsync()
        {
            var nInfo = await new UserInfoParser(Forum, Id).ParseAsync().ConfigureAwait(false);
            if (nInfo == null) return;
            Username = nInfo.Username;
            AvatarUrl = nInfo.AvatarUrl;
            TimeRegistered = nInfo.TimeRegistered;
            PostCount = nInfo.PostCount;
            LikeCount = nInfo.LikeCount;
            Location = nInfo.Location;
            Status = nInfo.Status;
            Gender = nInfo.Gender;
            Verified = nInfo.Verified;
            Trophies = nInfo.Trophies;
        }

        public override string ToString() 
            => $"Id: {Id}{(string.IsNullOrWhiteSpace(Username) ? "" : $" | Username: \"{Username}\"")}";
    }
}

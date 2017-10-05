using System;
using System.Threading.Tasks;
using GommeHDnetForumAPI.Parser;

namespace GommeHDnetForumAPI.DataModels.Entities
{
    public class UserInfo : ForumEntity
    {
        public string Username { get; private set; }
        public string AvatarUrl { get; private set; }
        public DateTime? TimeRegistered { get; private set; }
        public int? PostCount { get; private set; }
        public int? LikeCount { get; private set; }
        public string Location { get; private set; }
        public string Status { get; private set; }
        public Gender Gender { get; private set; } = Gender.Unknown;

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
            var nInfo = await new UserInfoParser(Forum, Id).ParseAsync();
            Username = nInfo.Username;
            AvatarUrl = nInfo.AvatarUrl;
            TimeRegistered = nInfo.TimeRegistered;
            PostCount = nInfo.PostCount;
            LikeCount = nInfo.LikeCount;
            Location = nInfo.Location;
            Status = nInfo.Status;
            Gender = nInfo.Gender;
        }

        public override string ToString() 
            => $"Id: {Id}{(string.IsNullOrWhiteSpace(Username) ? "" : $" | Username: \"{Username}\"")}";
    }
}

using System;
using System.Threading.Tasks;
using GommeHDnetForumAPI.Parser;

namespace GommeHDnetForumAPI.DataModels.Entities
{
    public class UserInfo : ForumEntity
    {
        public string Username { get; }
        public string AvatarUrl { get; private set; }
        public DateTime? TimeRegistered { get; private set; }
        public int? PostCount { get; private set; }
        public int? LikeCount { get; private set; }
        public string Location { get; private set; }
        public string Status { get; private set; }
        public Gender Gender { get; private set; }
        
        /// <summary>
        /// Internal base Constructor
        /// </summary>
        internal UserInfo(Forum forum, long id, string username, string url) : this(forum, id, username, url, null, null, null, null, null, null, Gender.Unknown)
        {
        }

        /// <summary>
        /// Internal extended constructor
        /// </summary>
        internal UserInfo(Forum forum, long id, string username, string url, string avatarUrl, DateTime? timeRegistered, int? postCount, int? likeCount, string location, string status, Gender gender) : base(forum, id, url)
        {
            Username = username;
            AvatarUrl = avatarUrl;
            TimeRegistered = timeRegistered;
            PostCount = postCount;
            LikeCount = likeCount;
            Location = location;
            Status = status;
            Gender = gender;
        }

        /// <summary>
        /// Downloads extended user data
        /// </summary>
        public async Task DownloadDataAsync()
        {
            var nInfo = await new UserInfoParser(Forum, this).ParseAsync();
            AvatarUrl = nInfo.AvatarUrl;
            TimeRegistered = nInfo.TimeRegistered;
            PostCount = nInfo.PostCount;
            LikeCount = nInfo.LikeCount;
            Location = nInfo.Location;
            Status = nInfo.Status;
            Gender = nInfo.Gender;
        }
    }
}

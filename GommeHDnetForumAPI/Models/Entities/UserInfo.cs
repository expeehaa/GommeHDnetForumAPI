using System;
using System.Threading.Tasks;
using GommeHDnetForumAPI.Models.Entities.Interfaces;
using GommeHDnetForumAPI.Parser;

namespace GommeHDnetForumAPI.Models.Entities {
	public class UserInfo : IndexedEntity, IUserInfo {
		public string    Username       { get; internal set; }
		public string    AvatarUrl      { get; internal set; }
		public DateTime? TimeRegistered { get; internal set; }
		public int?      PostCount      { get; internal set; }
		public int?      LikeCount      { get; internal set; }
		public string    Location       { get; internal set; }
		public string    Status         { get; internal set; }
		public Gender    Gender         { get; internal set; } = Gender.Unknown;
		public bool?     Verified       => string.IsNullOrWhiteSpace(Username) ? null : (bool?) (Username.Length <= 16);
		public int?      Trophies       { get; internal set; }
		public string    UserTitle      { get; internal set; }

		public string UrlPath => $"{ForumPaths.ForumUrl}members/{Id}";

		public UserInfo(Forum forum, long id) : base(forum, id) { }

		internal UserInfo(Forum forum, long id, string username) : base(forum, id) {
			Username = username;
		}

		public async Task DownloadDataAsync() {
			var doc   = await Forum.GetHtmlDocument(UrlPath);
			var nInfo = new UserInfoParser(Forum).Parse(doc.DocumentNode);
			if (nInfo == null) return;

			Username       = nInfo.Username;
			AvatarUrl      = nInfo.AvatarUrl;
			TimeRegistered = nInfo.TimeRegistered;
			PostCount      = nInfo.PostCount;
			LikeCount      = nInfo.LikeCount;
			Location       = nInfo.Location;
			Status         = nInfo.Status;
			Gender         = nInfo.Gender;
			Trophies       = nInfo.Trophies;
			UserTitle      = nInfo.UserTitle;
		}

		public override string ToString()
			=> $"Id: {Id}{(string.IsNullOrWhiteSpace(Username) ? "" : $" | Username: \"{Username}\"")}";
	}
}
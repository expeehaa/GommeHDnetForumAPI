using System;
using System.Threading.Tasks;
using GommeHDnetForumAPI.Parser;

namespace GommeHDnetForumAPI.Models.Entities {
	public class UserInfo : IndexedEntity {
		public string    Username         { get; internal set; }
		public string    AvatarPath       { get; internal set; }
		public DateTime? TimeRegistered   { get; internal set; }
		public DateTime? TimeLastActivity { get; internal set; }
		public int?      PostCount        { get; internal set; }
		public int?      LikeCount        { get; internal set; }
		public string    Location         { get; internal set; }
		public bool?     Verified         => string.IsNullOrWhiteSpace(Username) ? null : Username.Length <= 16;
		public int?      Trophies         { get; internal set; }
		public string    UserTitle        { get; internal set; }

		public string UrlPath => $"{ForumPaths.ForumPath}members/{Id}";
		public string Url     => $"{ForumPaths.BaseUrl}{UrlPath}";

		public UserInfo(Forum forum, long id) : base(forum, id) { }

		internal UserInfo(Forum forum, long id, string username) : base(forum, id) {
			Username = username;
		}

		public async Task DownloadDataAsync() {
			var doc   = await Forum.GetHtmlDocument(UrlPath);
			var nInfo = new UserInfoParser(Forum).Parse(doc.DocumentNode);
			if(nInfo == null)
				return;

			Username         = nInfo.Username;
			AvatarPath       = nInfo.AvatarPath;
			TimeRegistered   = nInfo.TimeRegistered;
			TimeLastActivity = nInfo.TimeLastActivity;
			PostCount        = nInfo.PostCount;
			LikeCount        = nInfo.LikeCount;
			Trophies         = nInfo.Trophies;
			UserTitle        = nInfo.UserTitle;
		}

		public override string ToString()
			=> $"Id: {Id}{(string.IsNullOrWhiteSpace(Username) ? "" : $" | Username: \"{Username}\"")}";
	}
}
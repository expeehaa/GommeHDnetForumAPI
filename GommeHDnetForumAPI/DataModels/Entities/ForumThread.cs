using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GommeHDnetForumAPI.DataModels.Entities.Interfaces;
using GommeHDnetForumAPI.Parser;

namespace GommeHDnetForumAPI.DataModels.Entities {
	public class ForumThread : IndexedEntity, IThread<ForumPost> {
		public string                 Title    { get; }
		public UserInfo               Author   { get; }
		public IEnumerable<ForumPost> Messages { get; private set; }
		public ThreadPrefix           Prefix   { get; }
		public SubForum               Parent   { get; }
		public string                 UrlPath  => $"{ForumPaths.ForumThreadsPath}{Id}/";

		internal ForumThread(Forum forum, long id, string title, UserInfo author, SubForum parent, ThreadPrefix prefix = null) : base(forum, id) {
			Title  = title;
			Author = author;
			Parent = parent;
			Prefix = prefix;
		}

		public async Task DownloadMessagesAsync()
			=> await DownloadMessagesAsync(1).ConfigureAwait(false);

		public async Task DownloadMessagesAsync(int startPage, int pageCount = 0)
			=> Messages = (await new ForumThreadMessageParser(Forum, this, startPage, pageCount).ParseAsync().ConfigureAwait(false)).ToList();

		public override string ToString()
			=> $"Id: {Id} | {(Prefix == null ? $"Title: {Title}" : $"(P)Title: ({Prefix}){Title}")} | Author: {Author.Username} | Parent: {Parent.Title}";
	}
}
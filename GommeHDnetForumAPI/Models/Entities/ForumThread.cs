using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GommeHDnetForumAPI.Models.Entities.Interfaces;
using GommeHDnetForumAPI.Parser.LiNodeParser;
using HtmlAgilityPack;

namespace GommeHDnetForumAPI.Models.Entities {
	public class ForumThread : IndexedEntity, IThread<ForumPost> {
		public string                 Title    { get; }
		public IUserInfo              Author   { get; }
		public IEnumerable<ForumPost> Messages { get; private set; }
		public ThreadPrefix           Prefix   { get; }
		public SubForum               Parent   { get; }
		public string                 UrlPath  => $"{ForumPaths.ForumThreadsPath}{Id}/";

		internal ForumThread(Forum forum, long id, string title, IUserInfo author, SubForum parent, ThreadPrefix prefix = null) : base(forum, id) {
			Title  = title;
			Author = author;
			Parent = parent;
			Prefix = prefix;
		}

		public async Task DownloadMessagesAsync()
			=> await DownloadMessagesAsync(1).ConfigureAwait(false);

		public async Task DownloadMessagesAsync(int startPage, int pageCount = 0) {
			startPage          = Math.Max(1, startPage);

			var doc            = await Forum.GetHtmlDocument($"{UrlPath}?page={startPage}");
			var lastPageNumber = doc.DocumentNode.SelectSingleNode("//div[@class='PageNav']")?.GetAttributeValue("data-last", 0) ?? 1;
			var pageMax        = pageCount <= 0 ? lastPageNumber : Math.Min(startPage+pageCount-1, lastPageNumber);
			var docs           = new List<HtmlDocument>{ doc };

			for(var i = startPage; i <= pageMax; i++) {
				docs.Add(await Forum.GetHtmlDocument($"{UrlPath}?page={i}"));
			}

			var parser = new ThreadMessagesLiNodeParser(Forum, this);
			Messages   = docs.SelectMany(d => parser.Parse(d.DocumentNode)).ToList();
		}

		public override string ToString()
			=> $"Id: {Id} | {(Prefix == null ? $"Title: {Title}" : $"(P)Title: ({Prefix}){Title}")} | Author: {Author} | Parent: {Parent.Title}";
	}
}
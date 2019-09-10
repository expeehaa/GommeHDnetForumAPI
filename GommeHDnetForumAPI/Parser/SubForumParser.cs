using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GommeHDnetForumAPI.DataModels;
using GommeHDnetForumAPI.DataModels.Entities;
using GommeHDnetForumAPI.DataModels.Entities.Interfaces;
using GommeHDnetForumAPI.Exceptions;
using GommeHDnetForumAPI.Parser.LiNodeParser;
using HtmlAgilityPack;

namespace GommeHDnetForumAPI.Parser {
	internal class SubForumParser : Parser<SubForum> {
		private readonly SubForum _subForum;
		private readonly int      _startPage;
		private readonly int      _pageCount;

		public SubForumParser(SubForum subforum, int startPage, int pageCount) : base(subforum.Forum, new BasicUrl(subforum.UrlPath)) {
			_subForum  = subforum;
			_startPage = startPage < 1 ? 1 : startPage;
			_pageCount = pageCount;
		}

		public override async Task<SubForum> ParseAsync() {
			//get doc
			var doc = await GetDoc().ConfigureAwait(false);
			//parse subforums
			var subforumlinodes = doc.DocumentNode.SelectNodes("//ol[@id='forums']/li");
			var subforums       = subforumlinodes != null && subforumlinodes.Any() ? await new SubForumLiNodeParser(Forum, subforumlinodes, _subForum).ParseAsync().ConfigureAwait(false) : new List<ISubForum>();
			//get title bar node
			var titlebarnode = doc.DocumentNode.SelectSingleNode(".//div[@id='content']/div/div/div[@class='titleBar']");
			//get subforum title if possible
			var title = titlebarnode?.SelectSingleNode("./h1")?.InnerText ?? string.Empty;
			if (string.IsNullOrWhiteSpace(title)) throw new NodeNotFoundException("div[@class='titleBar'] or its subnode h1 could not be found but is essential for SubForum parsing.");
			//get page description
			var desc = doc.GetElementbyId("pageDescription")?.InnerText ?? string.Empty;

			//threads parsing
			var threads = new List<ForumThread>();
			var pages   = doc.DocumentNode.SelectSingleNode("//div[@class='PageNav']")?.GetAttributeValue("data-last", 0) ?? 1;
			if (pages >= _startPage) {
				var pageMax = _pageCount <= 0 ? pages : (_startPage + _pageCount - 1 >= pages ? pages : _startPage + _pageCount - 1);

				var liNodes = new List<HtmlNode>();
				//Parallel.For(_startPage, pageMax + 1, async i
				//    => liNodes.AddRange((await GetDoc(url: $"{Url}page-{i}").ConfigureAwait(false)).DocumentNode.SelectNodes("//ol[@class='discussionListItems']/li").ToList()));
				for (var i = _startPage; i <= pageMax; i++) {
					liNodes.AddRange((await GetDoc(url: $"{Url}page-{i}").ConfigureAwait(false)).DocumentNode.SelectNodes("//ol[@class='discussionListItems']/li").ToList());
				}

				threads.AddRange(await new ThreadsLiNodeParser(Forum, liNodes, _subForum).ParseAsync().ConfigureAwait(false));
			}

			//prefix parsing
			var prefixes = new List<ThreadPrefix>();
			foreach (var o in doc.DocumentNode.SelectNodes(".//option").ToList()) {
				var value = o.GetAttributeValue("value", "");
				if (!long.TryParse(value, out var id)) continue;
				prefixes.Add(new ThreadPrefix(Forum, id, o.InnerText));
			}

			return new SubForum(Forum, _subForum.Id, _subForum.Parent, title, desc, _subForum.PostCount) {
				SubForums = subforums,
				Threads   = threads,
				Prefixes  = prefixes
			};
		}
	}
}
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GommeHDnetForumAPI.DataModels;
using GommeHDnetForumAPI.DataModels.Collections;
using GommeHDnetForumAPI.DataModels.Entities;
using GommeHDnetForumAPI.Parser.LiNodeParser;
using HtmlAgilityPack;

namespace GommeHDnetForumAPI.Parser {
	internal class ConversationsParser : Parser<ThreadCollection<ConversationInfo>> {
		private          int _startPage;
		private readonly int _pageCount;

		public ConversationsParser(Forum forum, int startPage, int pageCount) : base(forum, new BasicUrl(ForumPaths.ConversationsPath)) {
			_startPage = startPage < 1 ? 1 : startPage;
			_pageCount = pageCount;
		}

		public override async Task<ThreadCollection<ConversationInfo>> ParseAsync() {
			var doc   = await GetDoc().ConfigureAwait(false);
			var pages = doc.DocumentNode.SelectSingleNode("//div[@class='PageNav']")?.GetAttributeValue("data-last", 0) ?? 1;

			if (_startPage < 1) _startPage = 1;
			if (pages < _startPage) return new ThreadCollection<ConversationInfo>();
			var pageMax = _pageCount <= 0 ? pages : _startPage + _pageCount - 1 >= pages ? pages : _startPage + _pageCount - 1;

			var liNodes = new List<HtmlNode>();
			for (var i = _startPage; i <= pageMax; i++) {
				liNodes.AddRange((await GetDoc(url: $"{Url}?page={i}").ConfigureAwait(false)).DocumentNode.SelectNodes("//ol[@class='discussionListItems']/li").ToList());
			}

			return new ThreadCollection<ConversationInfo>(await new ConversationLiNodeParser(Forum, liNodes).ParseAsync().ConfigureAwait(false));
		}
	}
}
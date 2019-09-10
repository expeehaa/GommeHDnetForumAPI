using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GommeHDnetForumAPI.DataModels;
using GommeHDnetForumAPI.DataModels.Entities;
using GommeHDnetForumAPI.Parser.LiNodeParser;
using HtmlAgilityPack;

namespace GommeHDnetForumAPI.Parser {
	internal class ForumThreadMessageParser : Parser<IEnumerable<ForumPost>> {
		private readonly int         _startPage;
		private readonly int         _pageCount;
		private readonly ForumThread _parent;

		public ForumThreadMessageParser(Forum forum, ForumThread parent, int startPage, int pageCount) : base(forum, new BasicUrl(parent.UrlPath)) {
			_startPage = startPage < 1 ? 1 : startPage;
			_pageCount = pageCount;
			_parent    = parent;
		}

		public override async Task<IEnumerable<ForumPost>> ParseAsync() {
			var doc   = await GetDoc().ConfigureAwait(false);
			var pages = doc.DocumentNode.SelectSingleNode("//div[@class='PageNav']")?.GetAttributeValue("data-last", 0) ?? 1;
			if (pages < _startPage) return new List<ForumPost>();
			var pageMax = _pageCount <= 0 ? pages : _startPage + _pageCount - 1 >= pages ? pages : _startPage + _pageCount - 1;

			var liNodes = new List<HtmlNode>();
			//Parallel.For(_startPage, pageMax + 1, async i 
			//    => liNodes.AddRange((await GetDoc(url: $"{Url}page-{i}").ConfigureAwait(false)).DocumentNode.SelectNodes("//ol[@id='messageList']/li").ToList()));
			for (var i = _startPage; i <= pageMax; i++) {
				liNodes.AddRange((await GetDoc(url: $"{Url}page-{i}").ConfigureAwait(false)).DocumentNode.SelectNodes("//ol[@id='messageList']/li").ToList());
			}

			return await new ThreadMessagesLiNodeParser(Forum, liNodes, _parent).ParseAsync().ConfigureAwait(false);
		}
	}
}
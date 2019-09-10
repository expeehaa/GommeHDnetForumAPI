using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GommeHDnetForumAPI.DataModels;
using GommeHDnetForumAPI.DataModels.Entities;
using GommeHDnetForumAPI.Exceptions;
using GommeHDnetForumAPI.Parser.LiNodeParser;
using HtmlAgilityPack;

namespace GommeHDnetForumAPI.Parser {
	internal class ConversationMessageParser : Parser<IEnumerable<ConversationMessage>> {
		private readonly int              _startPage;
		private readonly int              _pageCount;
		private readonly ConversationInfo _parent;

		public ConversationMessageParser(Forum forum, ConversationInfo parent, int startPage, int pageCount) : base(forum, new BasicUrl(parent.UrlPath)) {
			_startPage = startPage < 1 ? 1 : startPage;
			_pageCount = pageCount;
			_parent    = parent;
		}

		public ConversationMessageParser(Forum forum, ConversationInfo parent, string html) : base(forum, html) {
			_parent = parent;
		}

		public override async Task<IEnumerable<ConversationMessage>> ParseAsync() {
			HtmlDocument doc;
			switch (Content) {
				case ParserContent.Html:
					doc = new HtmlDocument();
					doc.LoadHtml(Html);
					break;
				case ParserContent.Url:
					doc = await GetDoc().ConfigureAwait(false);
					break;
				default:
					throw new ParserContentNotSupportedException(null, Content);
			}

			if (Content != ParserContent.Url)
				return await new ConversationMessagesLiNodeParser(Forum, doc.DocumentNode.SelectNodes("//ol[@id='messageList']/li").ToList(), _parent).ParseAsync().ConfigureAwait(false);

			var pages = doc.DocumentNode.SelectSingleNode("//div[@class='PageNav']")?.GetAttributeValue("data-last", 0) ?? 1;
			if (pages < _startPage) return new List<ConversationMessage>();
			var pageMax = _pageCount <= 0 ? pages : (_startPage + _pageCount - 1 >= pages ? pages : _startPage + _pageCount - 1);

			var liNodes = new List<HtmlNode>();
			//Parallel.For(_startPage, pageMax + 1, async i 
			//    => liNodes.AddRange((await GetDoc(url: $"{Url}page-{i}").ConfigureAwait(false)).DocumentNode.SelectNodes("//ol[@id='messageList']/li").ToList()));
			for (var i = _startPage; i <= pageMax; i++) {
				liNodes.AddRange((await GetDoc(url: $"{Url}page-{i}").ConfigureAwait(false)).DocumentNode.SelectNodes("//ol[@id='messageList']/li").ToList());
			}

			return await new ConversationMessagesLiNodeParser(Forum, liNodes, _parent).ParseAsync().ConfigureAwait(false);
		}
	}
}
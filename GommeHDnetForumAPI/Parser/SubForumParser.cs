using System.Collections.Generic;
using System.Linq;
using GommeHDnetForumAPI.Models.Entities;
using GommeHDnetForumAPI.Parser.NodeListParser;
using HtmlAgilityPack;

namespace GommeHDnetForumAPI.Parser {
	internal class SubForumParser : Parser<SubForum> {
		private readonly SubForum _subForum;

		public SubForumParser(SubForum subforum) : base(subforum.Forum) {
			_subForum  = subforum;
		}

		public override SubForum Parse(HtmlNode node) {
			var subforums    = new SubForumListParser(Forum, _subForum).Parse(node);
			var titlebarnode = node.SelectSingleNode(".//div[@id='content']/div/div/div[@class='titleBar']");
			var title        = titlebarnode?.SelectSingleNode("./h1")?.InnerText ?? string.Empty;
			
			if (string.IsNullOrWhiteSpace(title)) throw new NodeNotFoundException("Subforum title node could not be found.");
			var desc = node.OwnerDocument.GetElementbyId("pageDescription")?.InnerText ?? string.Empty;

			var threadPrefixes = new List<ThreadPrefix>();
			foreach(var o in node.OwnerDocument.GetElementbyId("ctrl_prefix_id").SelectNodes(".//option").ToList()) {
				var value = o.GetAttributeValue("value", "");
				if(!long.TryParse(value, out var id))
					continue;
				threadPrefixes.Add(new ThreadPrefix(Forum, id, o.InnerText));
			}

			var threads = new ThreadsLiNodeParser(Forum, _subForum).Parse(node);

			return new SubForum(Forum, _subForum.Id, _subForum.Parent, title, desc, _subForum.PostCount) {
				SubForums = subforums,
				Threads   = threads,
				Prefixes  = threadPrefixes
			};
		}
	}
}
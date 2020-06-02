using System.Collections.Generic;
using System.Linq;
using HtmlAgilityPack;

namespace GommeHDnetForumAPI.Parser.LiNodeParser {
	public abstract class LiNodeParser<T, TO> : Parser<IEnumerable<T>> {
		protected string LiNodeSelector;
		protected readonly TO Parent;

		protected LiNodeParser(Forum forum, TO parent, string liNodeSelector) : base(forum) {
			Parent         = parent;
			LiNodeSelector = liNodeSelector;
		}

		public override IEnumerable<T> Parse(HtmlNode node)
			=> node.SelectNodes(LiNodeSelector)?.Select(ParseElement).Where(t => t != null) ?? Enumerable.Empty<T>();

		protected abstract T ParseElement(HtmlNode node);
	}
}
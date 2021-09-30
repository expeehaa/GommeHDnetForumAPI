using System.Collections.Generic;
using System.Linq;
using HtmlAgilityPack;

namespace GommeHDnetForumAPI.Parser.NodeListParser {
	public abstract class NodeListParser<T, TO> : Parser<IEnumerable<T>> {
		protected string NodeListSelector;
		protected readonly TO Parent;

		protected NodeListParser(Forum forum, TO parent, string nodeListSelector) : base(forum) {
			Parent           = parent;
			NodeListSelector = nodeListSelector;
		}

		public override IEnumerable<T> Parse(HtmlNode node)
			=> node.SelectNodes(NodeListSelector)?.Select(ParseElement).Where(t => t != null) ?? Enumerable.Empty<T>();

		protected abstract T ParseElement(HtmlNode node);
	}
}
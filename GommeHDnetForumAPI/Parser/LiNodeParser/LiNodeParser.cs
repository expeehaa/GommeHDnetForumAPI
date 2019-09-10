using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HtmlAgilityPack;

namespace GommeHDnetForumAPI.Parser.LiNodeParser {
	internal abstract class LiNodeParser<T, TO> : Parser<IEnumerable<T>> {
		protected readonly IEnumerable<HtmlNode> LiNodes;
		protected readonly TO                    Parent;

		protected LiNodeParser(Forum forum, IEnumerable<HtmlNode> liNodes, TO parent) : base(forum) {
			LiNodes = liNodes;
			Parent  = parent;
		}

		public override Task<IEnumerable<T>> ParseAsync()
			=> Task.FromResult(LiNodes.Select(ParseElement).Where(t => t != null).AsEnumerable());

		protected abstract T ParseElement(HtmlNode node);
	}
}
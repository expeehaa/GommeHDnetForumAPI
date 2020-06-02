using HtmlAgilityPack;

namespace GommeHDnetForumAPI.Parser {
	public abstract class Parser<T> {
		protected Forum Forum { get; }

		protected Parser(Forum forum) {
			Forum = forum;
		}

		public abstract T Parse(HtmlNode node);
	}
}
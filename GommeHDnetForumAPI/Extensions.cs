using HtmlAgilityPack;

namespace GommeHDnetForumAPI {
	public static class Extensions {
		public static string GetInputValueByName(this HtmlNode node, string name)
			=> node.SelectSingleNode($".//input[@name='{name}']").GetAttributeValue("value", "");
	}
}

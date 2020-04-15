using System.Collections.Generic;
using System.Net;
using System.Text.RegularExpressions;
using GommeHDnetForumAPI.Models.Entities;
using GommeHDnetForumAPI.Parser.LiNodeParser;
using HtmlAgilityPack;

namespace GommeHDnetForumAPI.Parser {
	internal class MasterForumParser : Parser<MasterForumInfo> {
		public MasterForumParser(Forum forum) : base(forum) { }

		public override MasterForumInfo Parse(HtmlNode node) {
			var categories = new List<MasterForumCategoryInfo>();

			foreach (var li in node.SelectNodes("//ol[@id='forums']/li")) {
				var nullableid = GetIdFromNodeClass(li);
				if (nullableid == null) continue;
				var id        = nullableid.Value;
				var titlenode = li.SelectSingleNode(".//div[@class='categoryText']/h3/a");
				if (titlenode == null) continue;
				var title       = WebUtility.HtmlDecode(titlenode.InnerText);
				var href        = titlenode.GetAttributeValue("href", "");
				var description = WebUtility.HtmlDecode(node.OwnerDocument.GetElementbyId($"nodeDescription-{id}")?.InnerText ?? string.Empty);
				var mfci        = new MasterForumCategoryInfo(Forum, id, title, description, href);
				mfci.SubForums = new SubForumLiNodeParser(Forum, mfci).Parse(li);
				categories.Add(mfci);
			}

			return new MasterForumInfo(Forum, categories);
		}

		private static long? GetIdFromNodeClass(HtmlNode node) {
			var classes = node.GetAttributeValue("class", "");
			var regex   = Regex.Match(classes, "node_(\\d+)");
			return Regex.IsMatch(classes, "node_(\\d+)")
				? (long?) long.Parse(regex.Groups[1].Value)
				: null;
		}
	}
}
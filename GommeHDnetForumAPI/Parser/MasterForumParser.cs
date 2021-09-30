using System.Collections.Generic;
using System.Net;
using System.Text.RegularExpressions;
using GommeHDnetForumAPI.Models.Entities;
using GommeHDnetForumAPI.Parser.NodeListParser;
using HtmlAgilityPack;

namespace GommeHDnetForumAPI.Parser {
	internal class MasterForumParser : Parser<MasterForumInfo> {
		public MasterForumParser(Forum forum) : base(forum) { }

		public override MasterForumInfo Parse(HtmlNode node) {
			var categories = new List<MasterForumCategoryInfo>();

			foreach (var forumCategoryNode in node.SelectNodes("//div[@class='uix_nodeList block']/div")) {
				var categoryId = GetIdFromNodeClass(forumCategoryNode);
				var titleNode = forumCategoryNode.SelectSingleNode("./div/h2//a[@class='uix_categoryTitle']");
				var title       = WebUtility.HtmlDecode(titleNode.InnerText);
				var href        = titleNode.GetAttributeValue("href", "");
				var description = WebUtility.HtmlDecode(forumCategoryNode.SelectSingleNode("./div/h2//div[@class='node-description']")?.InnerText ?? string.Empty);
				var mfci        = new MasterForumCategoryInfo(Forum, categoryId, title, description, href);
				mfci.SubForums = new SubForumListParser(Forum, mfci).Parse(forumCategoryNode);
				categories.Add(mfci);
			}

			return new MasterForumInfo(Forum, categories);
		}

		private static long GetIdFromNodeClass(HtmlNode node) {
			var classes = node.GetAttributeValue("class", "");
			var regex   = Regex.Match(classes, "block--category(\\d+)");

			return long.Parse(regex.Groups[1].Value);
		}
	}
}
using System.Net;
using System.Text.RegularExpressions;
using GommeHDnetForumAPI.Models.Entities;
using GommeHDnetForumAPI.Models.Entities.Interfaces;
using HtmlAgilityPack;

namespace GommeHDnetForumAPI.Parser.NodeListParser {
	internal class SubForumListParser : NodeListParser<ISubForum, IForum> {
		public SubForumListParser(Forum forum, IForum parent) : base(forum, parent, ".//div[@class='block-body']/div") { }

		protected override ISubForum ParseElement(HtmlNode node) {
			var titleNode = node.SelectSingleNode(".//h3[@class='node-title']/a");
			var title     = WebUtility.HtmlDecode(titleNode.InnerText);
			var classes   = node.GetAttributeValue("class", "");
			var idRegex   = Regex.Match(classes, "node--id(\\d+)");
			var id        = long.Parse(idRegex.Groups[1].Value);

			return classes switch {
				string link when link.Contains("node--link") => new SubLink(Forum, id, Parent, title),
				string forum when forum.Contains("node--forum") => new SubForum(Forum, id, Parent, title, null, null),
				_ => null,
			};
		}
	}
}
using System;
using System.Collections.Generic;
using System.Linq;
using GommeHDnetForumAPI.Models.Collections;
using GommeHDnetForumAPI.Models.Entities;
using HtmlAgilityPack;

namespace GommeHDnetForumAPI.Parser.NodeListParser {
	public class ConversationsNodeListParser : NodeListParser<ConversationInfo, object> {
		public ConversationsNodeListParser(Forum forum) : base(forum, null, "//div[contains(concat(' ', normalize-space(@class), ' '), ' structItemContainer ')]/div") { }

		protected override ConversationInfo ParseElement(HtmlNode node) {
			var idNode    = node.SelectSingleNode(".//ul[contains(concat(' ', normalize-space(@class), ' '), ' structItem-extraInfo ')]//label[contains(concat(' ', normalize-space(@class), ' '), ' iconic ')]/input");
			var id        = idNode.GetAttributeValue("value", 0);
			var title     = node.SelectSingleNode(".//a[contains(concat(' ', normalize-space(@class), ' '), ' structItem-title ')]").InnerText;
			var members   = new UserCollection(from memberNode in node.SelectNodes(".//ul[contains(concat(' ', normalize-space(@class), ' '), ' structItem-parts ')]//a[contains(concat(' ', normalize-space(@class), ' '), ' username ')]")
											   select new UserInfo(Forum, memberNode.GetAttributeValue("data-user-id", 0), memberNode.InnerText));
			var author    = members.First();
			var infoNodes = node.SelectNodes(".//div[contains(concat(' ', normalize-space(@class), ' '), ' structItem-cell--meta ')]/dl");
			var infos     = (from infoNode in infoNodes
							 select new KeyValuePair<string, HtmlNode>(infoNode.SelectSingleNode("dt").InnerText.Trim(), infoNode.SelectSingleNode("dd"))).ToList();
			var _         = uint.TryParse(infos.FirstOrDefault(p => p.Key.Equals("antworten", StringComparison.OrdinalIgnoreCase)).Value?.InnerText.Replace(".", ""), out var answerCount);

			return new ConversationInfo(Forum, id, title, author, members, answerCount);
		}
	}
}
using System;
using System.Collections.Generic;
using System.Linq;
using GommeHDnetForumAPI.Models.Collections;
using GommeHDnetForumAPI.Models.Entities;
using HtmlAgilityPack;

namespace GommeHDnetForumAPI.Parser {
	internal class ConversationInfoParser : Parser<ConversationInfo> {
		public ConversationInfoParser(Forum forum) : base(forum) { }

		public override ConversationInfo Parse(HtmlNode node) {
			var id         = node.SelectSingleNode("//div[contains(concat(' ', normalize-space(@class), ' '), ' block-container lbContainer ')]").GetAttributeValue("data-lb-id", 0);
			var title      = node.SelectSingleNode("//div[contains(concat(' ', normalize-space(@class), ' '), ' p-title ')]/h1").InnerText;
			var recipients = new UserCollection(from li in node.SelectNodes("//div[contains(concat(' ', normalize-space(@class), ' '), ' p-body-sidebar ')]//ol[contains(concat(' ', normalize-space(@class), ' '), ' block-body ')]/li")
												let a = li.SelectSingleNode(".//a[contains(concat(' ', normalize-space(@class), ' '), ' username ')]")
												select new UserInfo(Forum, a.GetAttributeValue("data-user-id", 0), a.InnerText) {
													AvatarPath = li.SelectSingleNode(".//a[contains(concat(' ', normalize-space(@class), ' '), ' avatar ')]/img")?.GetAttributeValue("src", ""),
													UserTitle = li.SelectSingleNode(".//span[contains(concat(' ', normalize-space(@class), ' '), ' userTitle ')]").InnerText,
												});
			var author     = recipients.First();
			var infoNodes  = node.SelectNodes(".//div[contains(concat(' ', normalize-space(@class), ' '), ' p-body-sidebar ')]//dl");
			var infos      = (from infoNode in infoNodes
								  select new KeyValuePair<string, HtmlNode>(infoNode.SelectSingleNode("dt").InnerText.Trim(), infoNode.SelectSingleNode("dd"))).ToList();
			var _          = uint.TryParse(infos.FirstOrDefault(p => p.Key.Equals("antworten", StringComparison.OrdinalIgnoreCase)).Value?.InnerText.Replace(".", ""), out var answerCount);

			return new ConversationInfo(Forum, id, title, author, recipients, answerCount);
		}
	}
}
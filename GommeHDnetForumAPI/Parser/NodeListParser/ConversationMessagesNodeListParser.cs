using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using GommeHDnetForumAPI.Models.Entities;
using HtmlAgilityPack;

namespace GommeHDnetForumAPI.Parser.NodeListParser {
	internal class ConversationMessagesNodeListParser : NodeListParser<ConversationMessage, ConversationInfo> {
		public ConversationMessagesNodeListParser(Forum forum, ConversationInfo parent) : base(forum, parent, "//div[contains(concat(' ', normalize-space(@class), ' '), ' block-container lbContainer ')]//article[contains(concat(' ', normalize-space(@class), ' '), ' message ')]") { }

		protected override ConversationMessage ParseElement(HtmlNode node) {
			var messageContentNode = node.SelectSingleNode(".//div[contains(concat(' ', normalize-space(@class), ' '), ' message-userContent ')]");
			var id                 = long.Parse(Regex.Match(messageContentNode.GetAttributeValue("data-lb-id", ""), "message-(\\d+)").Groups[1].Value);
			var content            = messageContentNode.InnerText.Replace("&nbsp;", " ").Trim();

			var authorNode       = node.SelectSingleNode(".//section[contains(concat(' ', normalize-space(@class), ' '), ' message-user ')]");
			var usernameNode     = authorNode.SelectSingleNode(".//a[contains(concat(' ', normalize-space(@class), ' '), ' username ')]");
			var authorUsername   = usernameNode.InnerText.Trim();
			var authorId         = usernameNode.GetAttributeValue("data-user-id", 0);
			var authorTitle      = authorNode.SelectSingleNode(".//h5[contains(concat(' ', normalize-space(@class), ' '), ' userTitle ')]").InnerText.Trim();
			var authorAvatarPath = authorNode.SelectSingleNode(".//a[contains(concat(' ', normalize-space(@class), ' '), ' avatar ')]/img")?.GetAttributeValue("src", "");

			var infoNodes      = authorNode.SelectNodes(".//div[contains(concat(' ', normalize-space(@class), ' '), ' message-userExtras ')]/dl");
			var infos          = (from infoNode in infoNodes
								  select new KeyValuePair<string, HtmlNode>(infoNode.SelectSingleNode("dt/span").GetAttributeValue("title", ""), infoNode.SelectSingleNode("dd"))).ToList();
			var hasPostCount   = int.TryParse(infos.FirstOrDefault(p => p.Key.Equals("beiträge"         , StringComparison.OrdinalIgnoreCase)).Value?.InnerText.Replace(".", ""), out var posts   );
			var hasLikeCount   = int.TryParse(infos.FirstOrDefault(p => p.Key.Equals("punkte reaktionen", StringComparison.OrdinalIgnoreCase)).Value?.InnerText.Replace(".", ""), out var likes   );
			var hasTrophyCount = int.TryParse(infos.FirstOrDefault(p => p.Key.Equals("punkte"           , StringComparison.OrdinalIgnoreCase)).Value?.InnerText.Replace(".", ""), out var trophies);
			var location       =              infos.FirstOrDefault(p => p.Key.Equals("ort"              , StringComparison.OrdinalIgnoreCase)).Value?.InnerText.Replace(".", "").Trim();

			var author = new UserInfo(Forum, authorId, authorUsername) {
				AvatarPath = authorAvatarPath,
				PostCount = hasPostCount ? posts : null,
				LikeCount = hasLikeCount ? likes : null,
				Trophies = hasTrophyCount ? trophies : null,
				Location = location,
			};

			return new ConversationMessage(Forum, id, author, content, Parent);
		}
	}
}
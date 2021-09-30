using GommeHDnetForumAPI.Models.Entities;
using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Linq;

namespace GommeHDnetForumAPI.Parser.NodeListParser {
	internal class MembersListNodeListParser : NodeListParser<UserInfo, object> {
		public MembersListNodeListParser(Forum forum) : base(forum, null, ".//div[contains(concat(' ', normalize-space(@class), ' '), ' p-body-main ')]//div[contains(concat(' ', normalize-space(@class), ' '), ' p-body-pageContent ')]//ol[contains(concat(' ', normalize-space(@class), ' '), ' block-body ')]/li") { }

		protected override UserInfo ParseElement(HtmlNode node) {
			var usernameNode = node.SelectSingleNode(".//a[contains(concat(' ', normalize-space(@class), ' '), ' username ')]");
			var userId       = usernameNode.GetAttributeValue("data-user-id", 0);
			var username     = usernameNode.InnerText;
			var userTitle    = node.SelectSingleNode(".//span[contains(concat(' ', normalize-space(@class), ' '), ' userTitle ')]").InnerText;
			var avatarPath   = node.SelectSingleNode(".//a[contains(concat(' ', normalize-space(@class), ' '), ' avatar ')]/img")?.GetAttributeValue("src", "");
			var location     = node.SelectSingleNode(".//div[contains(concat(' ', normalize-space(@class), ' '), ' contentRow-lesser ')]/a[contains(concat(' ', normalize-space(@class), ' '), ' u-concealed ')]")?.InnerText;

			var infoNodes      = node.SelectNodes(".//div[contains(concat(' ', normalize-space(@class), ' '), ' contentRow-minor ')]//dl");
			var infos          = (from infoNode in infoNodes
								  select new KeyValuePair<string, HtmlNode>(infoNode.SelectSingleNode("dt").InnerText.Trim(), infoNode.SelectSingleNode("dd"))).ToList();
			var hasPostCount   = int.TryParse(infos.FirstOrDefault(p => p.Key.Equals("beiträge"         , StringComparison.OrdinalIgnoreCase)).Value?.InnerText.Replace(".", ""), out var posts   );
			var hasLikeCount   = int.TryParse(infos.FirstOrDefault(p => p.Key.Equals("punkte reaktionen", StringComparison.OrdinalIgnoreCase)).Value?.InnerText.Replace(".", ""), out var likes   );
			var hasTrophyCount = int.TryParse(infos.FirstOrDefault(p => p.Key.Equals("punkte"           , StringComparison.OrdinalIgnoreCase)).Value?.InnerText.Replace(".", ""), out var trophies);

			return new UserInfo(Forum, userId, username) {
				AvatarPath = avatarPath,
				PostCount = hasPostCount ? posts : null,
				LikeCount = hasLikeCount ? likes : null,
				Trophies = hasTrophyCount ? trophies : null,
				Location = location,
				UserTitle = userTitle,
			};
		}
	}
}
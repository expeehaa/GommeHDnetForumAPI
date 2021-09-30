using System;
using System.Collections.Generic;
using System.Linq;
using GommeHDnetForumAPI.Models.Entities;
using HtmlAgilityPack;

namespace GommeHDnetForumAPI.Parser {
	internal class UserInfoParser : Parser<UserInfo> {
		public UserInfoParser(Forum forum) : base(forum) { }

		public override UserInfo Parse(HtmlNode node) {
			var memberHeaderNode = node.SelectSingleNode("//div[contains(concat(' ', normalize-space(@class), ' '), ' memberHeader ')]");
			if(memberHeaderNode == null)
				throw new NodeNotFoundException("memberHeader node not found!");

			var usernameNode = node.SelectSingleNode(".//h1[contains(concat(' ', normalize-space(@class), ' '), ' memberHeader-name ')]//span[contains(concat(' ', normalize-space(@class), ' '), ' username ')]");
			var userId       = usernameNode.GetAttributeValue("data-user-id", 0);
			var username     = usernameNode.InnerText.Trim();
			var avatarPath   = memberHeaderNode.SelectSingleNode(".//span[@class='avatarWrapper']//img")?.GetAttributeValue("src", "");
			var location     = memberHeaderNode.SelectSingleNode(".//div[contains(concat(' ', normalize-space(@class), ' '), ' memberHeader-blurb ')]/a[contains(concat(' ', normalize-space(@class), ' '), ' u-concealed ')]")?.InnerText.Trim();

			var infoNodes      = memberHeaderNode.SelectNodes(".//div[contains(concat(' ', normalize-space(@class), ' '), ' memberHeader-stats ')]//dl");
			var infos          = (from infoNode in infoNodes
								  select new KeyValuePair<string, HtmlNode>(infoNode.SelectSingleNode("dt").InnerText.Trim(), infoNode.SelectSingleNode("dd"))).ToList();
			var hasPostCount   = int.TryParse(infos.FirstOrDefault(p => p.Key.Equals("beiträge"         , StringComparison.OrdinalIgnoreCase)).Value?.InnerText.Replace(".", ""), out var posts   );
			var hasLikeCount   = int.TryParse(infos.FirstOrDefault(p => p.Key.Equals("punkte reaktionen", StringComparison.OrdinalIgnoreCase)).Value?.InnerText.Replace(".", ""), out var likes   );
			var hasTrophyCount = int.TryParse(infos.FirstOrDefault(p => p.Key.Equals("punkte"           , StringComparison.OrdinalIgnoreCase)).Value?.InnerText.Replace(".", ""), out var trophies);

			var userTitleNode = memberHeaderNode.SelectSingleNode(".//span[@class='userTitle']");
			var userTitle     = userTitleNode.InnerText.Trim();

			var extraInfoNodes      = memberHeaderNode.SelectNodes(".//div[contains(concat(' ', normalize-space(@class), ' '), ' uix_memberHeader__extra ')]//dl");
			var extraInfos          = (from extraInfoNode in extraInfoNodes
									   select new KeyValuePair<string, HtmlNode>(extraInfoNode.SelectSingleNode("dt").InnerText, extraInfoNode.SelectSingleNode("dd"))).ToList();
			var hasTimeRegistered   = DateTime.TryParse(extraInfos.FirstOrDefault(p => p.Key.Contains("registriert",      StringComparison.OrdinalIgnoreCase)).Value?.SelectSingleNode("time").GetAttributeValue("datetime", ""), out var timeRegistered  );
			var hasTimeLastActivity = DateTime.TryParse(extraInfos.FirstOrDefault(p => p.Key.Contains("letzte aktivität", StringComparison.OrdinalIgnoreCase)).Value?.SelectSingleNode("time").GetAttributeValue("datetime", ""), out var timeLastActivity);

			return new UserInfo(Forum, userId, username) {
				AvatarPath = avatarPath,
				PostCount = hasPostCount ? posts : null,
				LikeCount = hasLikeCount ? likes : null,
				Trophies = hasTrophyCount ? trophies : null,
				Location = location,
				UserTitle = userTitle,
				TimeRegistered = hasTimeRegistered ? timeRegistered : null,
				TimeLastActivity = hasTimeLastActivity ? timeLastActivity : null,
			};
		}
	}
}
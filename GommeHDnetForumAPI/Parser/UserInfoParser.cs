using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using GommeHDnetForumAPI.Models;
using GommeHDnetForumAPI.Models.Entities;
using HtmlAgilityPack;

namespace GommeHDnetForumAPI.Parser {
	internal class UserInfoParser : Parser<UserInfo> {
		public UserInfoParser(Forum forum) : base(forum) { }

		public override UserInfo Parse(HtmlNode node) {
			var profilePage = node.SelectSingleNode("//div[@class='profilePage']");
			if (profilePage == null) throw new NodeNotFoundException("ProfilePage node not found!");
			var userId    = node.OwnerDocument.GetElementbyId("get-premium").GetAttributeValue("data-user-id", 0);
			var username  = profilePage.SelectSingleNode(".//h1[@class='username']").InnerText.Trim();
			var avatarUrl = profilePage.SelectSingleNode(".//div[@class='avatarScaler']/img").GetAttributeValue("src", "");
			var status    = WebUtility.HtmlDecode(node.OwnerDocument.GetElementbyId("UserStatus")?.FirstChild.InnerText.Trim() ?? "");
			var infoNodes = profilePage.SelectNodes(".//div[@class='section infoBlock']//dl");
			var infos = (from infoNode in infoNodes
				where infoNode.FirstChild.Name.Equals("dt", StringComparison.OrdinalIgnoreCase) && infoNode.LastChild.Name.Equals("dd", StringComparison.OrdinalIgnoreCase)
				select new KeyValuePair<string, HtmlNode>(infoNode.FirstChild.InnerText, infoNode.LastChild)).ToList();
			var gotPosts    = int.TryParse(infos.FirstOrDefault(p => p.Key.ToLower().Contains("beiträge")).Value?.InnerText.Replace(".", ""),     out var posts);
			var gotLikes    = int.TryParse(infos.FirstOrDefault(p => p.Key.ToLower().Contains("zustimmungen")).Value?.InnerText.Replace(".", ""), out var likes);
			var gotTrophies = int.TryParse(infos.FirstOrDefault(p => p.Key.ToLower().Contains("erfolge")).Value?.InnerText.Replace(".", ""),      out var trophies);
			var location    = WebUtility.HtmlDecode(infos.FirstOrDefault(p => p.Key.ToLower().Contains("ort")).Value?.FirstChild?.InnerText.Trim() ?? "");
			var gender      = GenderParser.Parse(infos.FirstOrDefault(p => p.Value.GetAttributeValue("itemprop", "").Equals("gender", StringComparison.OrdinalIgnoreCase)).Value?.InnerText);
			var maintextnode = profilePage.SelectSingleNode(".//div[@class='mainText secondaryContent']");
			var customTitleNode = maintextnode.ChildAttributes("class").FirstOrDefault(ha => ha.Value.StartsWith("custom-title-", StringComparison.OrdinalIgnoreCase))?.OwnerNode;
			string userTitle = null;
			if (customTitleNode == null) {
				var userTitleNode = profilePage.SelectSingleNode(".//p[@class='userBlurb']/span[@class='userTitle']");
				if (userTitleNode != null)
					userTitle = userTitleNode.InnerText;
			} else userTitle = customTitleNode.InnerText;

			return new UserInfo(Forum, userId, username) {
				AvatarUrl = avatarUrl,
				Status    = status,
				PostCount = gotPosts ? (int?) posts : null,
				LikeCount = gotLikes ? (int?) likes : null,
				Trophies  = gotTrophies ? (int?) trophies : null,
				Location  = location,
				Gender    = gender,
				UserTitle = userTitle
			};
		}
	}
}
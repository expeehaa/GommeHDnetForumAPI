using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using GommeHDnetForumAPI.DataModels;
using GommeHDnetForumAPI.DataModels.Entities;
using GommeHDnetForumAPI.Exceptions;
using HtmlAgilityPack;

namespace GommeHDnetForumAPI.Parser {
	internal class UserInfoParser : Parser<UserInfo> {
		private readonly bool _ignoreLoginCheck;

		public UserInfoParser(Forum forum, string urlpath, bool ignoreLoginCheck = false) : base(forum, new BasicUrl(urlpath)) {
			_ignoreLoginCheck = ignoreLoginCheck;
		}

		public UserInfoParser(Forum forum, long userId) : base(forum, new BasicUrl($"{ForumPaths.MembersPath}{userId}/")) { }

		public UserInfoParser(Forum forum, string html) : base(forum, html) { }

		public override async Task<UserInfo> ParseAsync() {
			HtmlDocument doc;
			switch (Content) {
				case ParserContent.Html:
					doc = new HtmlDocument();
					doc.LoadHtml(Html);
					break;
				case ParserContent.Url:
					try {
						doc = await GetDoc(_ignoreLoginCheck, CheckHttpResponseMessage).ConfigureAwait(false);
					} catch (ReturnNullException) {
						return null;
					}

					break;
				case ParserContent.HttpResponseMessage:
					try {
						CheckHttpResponseMessage(HttpResponse);
					} catch (ReturnNullException) {
						return null;
					}

					doc = new HtmlDocument();
					doc.LoadHtml(await HttpResponse.Content.ReadAsStringAsync().ConfigureAwait(false));
					break;
				default:
					throw new ParserContentNotSupportedException(null, Content);
			}

			var profilePage = doc.DocumentNode.SelectSingleNode("//div[@class='profilePage']");
			if (profilePage == null) throw new NodeNotFoundException("ProfilePage node not found!");
			var userId    = doc.GetElementbyId("get-premium").GetAttributeValue("data-user-id", 0);
			var username  = profilePage.SelectSingleNode(".//h1[@class='username']").InnerText.Trim();
			var avatarUrl = profilePage.SelectSingleNode(".//div[@class='avatarScaler']/img").GetAttributeValue("src", "");
			var status    = WebUtility.HtmlDecode(doc.GetElementbyId("UserStatus")?.FirstChild.InnerText.Trim() ?? "");
			var infoNodes = profilePage.SelectNodes(".//div[@class='section infoBlock']//dl");
			var infos = (from node in infoNodes
				where node.FirstChild.Name.Equals("dt", StringComparison.OrdinalIgnoreCase) && node.LastChild.Name.Equals("dd", StringComparison.OrdinalIgnoreCase)
				select new KeyValuePair<string, HtmlNode>(node.FirstChild.InnerText, node.LastChild)).ToList();
			var gotPosts    = int.TryParse(infos.FirstOrDefault(p => p.Key.ToLower().Contains("beiträge")).Value?.InnerText.Replace(".", ""),     out var posts);
			var gotLikes    = int.TryParse(infos.FirstOrDefault(p => p.Key.ToLower().Contains("zustimmungen")).Value?.InnerText.Replace(".", ""), out var likes);
			var gotTrophies = int.TryParse(infos.FirstOrDefault(p => p.Key.ToLower().Contains("erfolge")).Value?.InnerText.Replace(".", ""),      out var trophies);
			var location    = WebUtility.HtmlDecode(infos.FirstOrDefault(p => p.Key.ToLower().Contains("ort")).Value?.FirstChild?.InnerText.Trim() ?? "");
			var gender      = GenderParser.Parse(infos.FirstOrDefault(p => p.Value.GetAttributeValue("itemprop", "").Equals("gender", StringComparison.OrdinalIgnoreCase)).Value?.InnerText);
			var maintextnode = profilePage.SelectSingleNode(".//div[@class='mainText secondaryContent']");
			var customTitleNode = maintextnode.ChildAttributes("class").FirstOrDefault(ha
																							=> ha.Value.StartsWith("custom-title-", StringComparison.OrdinalIgnoreCase))?.OwnerNode;
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

		private static void CheckHttpResponseMessage(HttpResponseMessage hrm) {
			if (hrm.StatusCode == HttpStatusCode.Forbidden) throw new UserProfileAccessException();
			if (hrm.StatusCode == HttpStatusCode.NotFound) throw new UserNotFoundException();
			if (!hrm.IsSuccessStatusCode) throw new ReturnNullException();
		}
	}
}
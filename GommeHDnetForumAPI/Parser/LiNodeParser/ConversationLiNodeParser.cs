﻿using System.Linq;
using System.Text.RegularExpressions;
using GommeHDnetForumAPI.Models.Collections;
using GommeHDnetForumAPI.Models.Entities;
using HtmlAgilityPack;

namespace GommeHDnetForumAPI.Parser.LiNodeParser {
	public class ConversationLiNodeParser : LiNodeParser<ConversationInfo, object> {
		public ConversationLiNodeParser(Forum forum) : base(forum, null, "//ol[@class='discussionListItems']/li") { }

		protected override ConversationInfo ParseElement(HtmlNode node) {
			if (!Regex.IsMatch(node.Id, "conversation-([0-9]+)")) return null;
			var conid    = long.Parse(Regex.Match(node.Id, "conversation-([0-9]+)").Groups[1].Value);
			var contitle = node.SelectSingleNode(".//h3[@class='title']/a").InnerText;
			var members = new UserCollection(from mnode in node.SelectNodes(".//div[@class='secondRow']/div/a[@class='username']")
											let murl = ForumPaths.ForumPath + mnode.GetAttributeValue("href", "")
											let mid = long.Parse(Regex.Match(murl, ".+\\.([0-9]+)").Groups[1].Value)
											select new UserInfo(Forum, mid, mnode.InnerText));
			var conauthorname = node.GetAttributeValue("data-author", "");
			var conauthor     = members.First(m => m.Username == conauthorname);
			var conanswers    = uint.Parse(node.SelectSingleNode("div/dl[@class='major']/dd").InnerText);
			return new ConversationInfo(Forum, conid, contitle, conauthor, members, conanswers);
		}
	}
}
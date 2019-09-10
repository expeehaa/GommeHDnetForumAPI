using System.Collections.Generic;
using System.Text.RegularExpressions;
using GommeHDnetForumAPI.DataModels.Entities;
using HtmlAgilityPack;

namespace GommeHDnetForumAPI.Parser.LiNodeParser {
	internal class ConversationMessagesLiNodeParser : LiNodeParser<ConversationMessage, ConversationInfo> {
		public ConversationMessagesLiNodeParser(Forum forum, IEnumerable<HtmlNode> liNodes, ConversationInfo parent) : base(forum, liNodes, parent) { }

		protected override ConversationMessage ParseElement(HtmlNode node) {
			if (!Regex.IsMatch(node.Id, "message-([0-9]+)")) return default;
			var id         = long.Parse(Regex.Match(node.Id, "message-([0-9]+)").Groups[1].Value);
			var authornode = node.SelectSingleNode(".//h3[@class='userText']/a[@class='username']");
			var authorname = authornode.InnerText;
			var authorurl  = authornode.GetAttributeValue("href", "");
			var authorid   = string.IsNullOrWhiteSpace(authorurl) ? 0 : long.Parse(authorurl.Split('.')[authorurl.Split('.').Length - 1].TrimEnd('/'));
			var content    = node.SelectSingleNode(".//div[@class='messageContent']/article/blockquote").InnerText.Replace("&nbsp;", " ").Trim();
			return new ConversationMessage(Forum, id, new UserInfo(Forum, authorid, authorname), content, Parent);
		}
	}
}
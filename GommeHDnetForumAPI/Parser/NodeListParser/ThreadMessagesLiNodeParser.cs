﻿using System.Text.RegularExpressions;
using GommeHDnetForumAPI.Models.Entities;
using HtmlAgilityPack;

namespace GommeHDnetForumAPI.Parser.NodeListParser {
	internal class ThreadMessagesLiNodeParser : NodeListParser<ForumPost, ForumThread> {
		public ThreadMessagesLiNodeParser(Forum forum, ForumThread parent) : base(forum, parent, "//ol[@id='messageList']/li") { }

		protected override ForumPost ParseElement(HtmlNode node) {
			if (!Regex.IsMatch(node.Id, "post-([0-9]+)")) return default;
			var id         = long.Parse(Regex.Match(node.Id, "post-([0-9]+)").Groups[1].Value);
			var authornode = node.SelectSingleNode(".//h3[@class='userText']/a[@class='username']");
			var authorname = authornode.InnerText;
			var authorurl  = authornode.GetAttributeValue("href", "");
			var authorid   = string.IsNullOrWhiteSpace(authorurl) ? 0 : long.Parse(authorurl.Split('.')[authorurl.Split('.').Length - 1].TrimEnd('/'));
			var content    = node.SelectSingleNode(".//div[@class='messageContent']/article/blockquote").InnerText.Replace("&nbsp;", " ").Trim();
			return new ForumPost(Forum, id, new UserInfo(Forum, authorid, authorname), content, Parent);
		}
	}
}
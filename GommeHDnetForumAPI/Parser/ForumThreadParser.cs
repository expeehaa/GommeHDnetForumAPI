﻿using System.Linq;
using GommeHDnetForumAPI.Models.Entities;
using HtmlAgilityPack;

namespace GommeHDnetForumAPI.Parser {
	internal class ForumThreadParser : Parser<ForumThread> {
		public ForumThreadParser(Forum forum, long id) : base(forum) { }

		public override ForumThread Parse(HtmlNode node) {
			var titleBarNode = node.SelectSingleNode("//div[@class='titleBar']");
			if (titleBarNode == null) throw new NodeNotFoundException();
			var h1         = titleBarNode.SelectSingleNode("./h1");
			var prefixnode = h1.SelectSingleNode("./span[@class='prefix']");
			var prefixName = prefixnode.InnerText;
			var title      = h1.LastChild.InnerText;


			long parentforumId = 0;
			var  parent        = Forum.MasterForum.Categories.SelectMany(c => c.AllRealSubForums).FirstOrDefault(sf => sf.Id == parentforumId);
			//var thread = new ForumThread(Forum, Id, title, new UserInfo(Forum, ), parent);
			//return thread;
			return null;
		}
	}
}
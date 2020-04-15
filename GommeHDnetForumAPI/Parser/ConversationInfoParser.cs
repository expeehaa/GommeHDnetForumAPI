using System;
using System.Linq;
using GommeHDnetForumAPI.Models.Collections;
using GommeHDnetForumAPI.Models.Entities;
using HtmlAgilityPack;

namespace GommeHDnetForumAPI.Parser {
	internal class ConversationInfoParser : Parser<ConversationInfo> {
		public ConversationInfoParser(Forum forum) : base(forum) { }

		public override ConversationInfo Parse(HtmlNode node) {
			var container = node.SelectSingleNode("//div[@class='mainContainer']");

			var idstr = container?.SelectSingleNode("//div[@class='pageNavLinkGroup']/div[@class='linkGroup']/a")?.GetAttributeValue("href", "");
			if (string.IsNullOrWhiteSpace(idstr)) return null;
			var idstrar    = idstr.Split('/');
			var id         = long.Parse(idstrar[idstrar.Length - 2].Substring(idstrar[idstrar.Length - 2].LastIndexOf(".", StringComparison.OrdinalIgnoreCase) + 1));
			var title      = container.SelectSingleNode("//div[@class='titleBar']/h1").InnerText;
			var recipients = new UserCollection(from li in container.SelectNodes("//ul[@id='ConversationRecipients']/li")
			                                        let a = li.SelectSingleNode("//a[@class='username']")
			                                        let username = a.InnerText
			                                        let userurl = a.GetAttributeValue("href", "")
			                                        let userid = long.Parse(userurl.Split('.')[userurl.Split('.').Length - 1].TrimEnd('/'))
			                                        select new UserInfo(Forum, userid));
			var author     = recipients.First();
			var answers    = uint.Parse(container.SelectNodes("//div[@class='secondaryContent']/div[@class='pairsJustified']/dl")[1].InnerText);
			
			return new ConversationInfo(Forum, id, title, author, recipients, answers);
		}
	}
}
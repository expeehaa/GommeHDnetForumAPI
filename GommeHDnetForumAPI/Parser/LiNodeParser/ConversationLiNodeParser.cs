using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using GommeHDnetForumAPI.DataModels.Collections;
using GommeHDnetForumAPI.DataModels.Entities;
using HtmlAgilityPack;

namespace GommeHDnetForumAPI.Parser.LiNodeParser
{
    internal class ConversationLiNodeParser : LiNodeParser<ConversationInfo>
    {
        public ConversationLiNodeParser(Forum forum, IEnumerable<HtmlNode> liNodes, IForum parent) : base(forum, liNodes, parent) { }

        protected override ConversationInfo ParseElement(HtmlNode node) {
            if (!Regex.IsMatch(node.Id, "conversation-([0-9]+)")) return null;
            var conid = long.Parse(Regex.Match(node.Id, "conversation-([0-9]+)").Groups[1].Value);
            var contitle = node.SelectSingleNode(".//h3[@class='title']/a").InnerText;
            var members = new UserCollection(from mnode in node.SelectNodes(".//div[@class='secondRow']/div/a[@class='username']")
                let murl = ForumPaths.ForumPath + mnode.GetAttributeValue("href", "")
                let mid = long.Parse(Regex.Match(murl, ".+\\.([0-9]+)").Groups[1].Value)
                select new UserInfo(Forum, mid, mnode.InnerText));
            var conauthorname = node.GetAttributeValue("data-author", "");
            var conauthor = members.First(m => m.Username == conauthorname);
            var conanswers = uint.Parse(node.SelectSingleNode("div/dl[@class='major']/dd").InnerText);
            return new ConversationInfo(Forum, conid, contitle, conauthor, members, conanswers);
        }
    }
}
using System.Collections.Generic;
using System.Net;
using System.Text.RegularExpressions;
using GommeHDnetForumAPI.DataModels.Entities;
using HtmlAgilityPack;

namespace GommeHDnetForumAPI.Parser.LiNodeParser
{
    internal class ThreadsLiNodeParser : LiNodeParser<ForumThread, SubForum>
    {
        public ThreadsLiNodeParser(Forum forum, IEnumerable<HtmlNode> liNodes, SubForum parent) : base(forum, liNodes, parent) { }

        protected override ForumThread ParseElement(HtmlNode node) {
            var author = node.GetAttributeValue("data-author", "");
            if (string.IsNullOrWhiteSpace(author)) return null;
            var liId = node.GetAttributeValue("id", "");
            if (!Regex.IsMatch(liId, "thread-(\\d+)")) return null;
            if(!long.TryParse(Regex.Match(liId, "thread-(\\d+)").Groups[1].Value, out var id)) return null;
            var title = WebUtility.HtmlDecode(node.SelectSingleNode(".//div[@class='titleText']/h3[@class='title']/a[@class='PreviewTooltip']")?.InnerText ?? "");

            var prefixnode = node.SelectSingleNode(".//div[@class='titleText']/h3[@class='title']/a[@class='prefixLink']");
            var prefix = prefixnode != null
                                  && Regex.IsMatch(prefixnode.GetAttributeValue("href", ""), "\\?prefix_id=([\\d]+)")
                                  && long.TryParse(Regex.Match(prefixnode.GetAttributeValue("href", ""), "\\?prefix_id=([\\d]+)").Groups[1].Value, out var prefixid) 
                ? new ThreadPrefix(Forum, prefixid, prefixnode.InnerText) : null;

            if (string.IsNullOrWhiteSpace(title)) return null;
            var avatarnode = node.SelectSingleNode(".//span[@class='avatarContainer']/a[@class]");
            if (avatarnode == null) return null;
            var avnclass = avatarnode.GetAttributeValue("class", "");
            if (!Regex.IsMatch(avnclass, "Av(\\d+)s")) return null;
            long.TryParse(Regex.Match(avnclass, "Av(\\d+)s").Groups[1].Value, out var userid);

            return new ForumThread(Forum, id, title, new UserInfo(Forum, userid, author), Parent, prefix);
        }
    }
}
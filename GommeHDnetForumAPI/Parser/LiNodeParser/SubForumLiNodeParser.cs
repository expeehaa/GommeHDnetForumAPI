using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using GommeHDnetForumAPI.DataModels.Entities;
using GommeHDnetForumAPI.DataModels.Entities.Interfaces;
using HtmlAgilityPack;

namespace GommeHDnetForumAPI.Parser.LiNodeParser
{
    internal class SubForumLiNodeParser : LiNodeParser<ISubForum, IForum>
    {
        public SubForumLiNodeParser(Forum forum, IEnumerable<HtmlNode> liNodes, IForum parent) : base(forum, liNodes, parent) { }
        
        protected override ISubForum ParseElement(HtmlNode node) {
            //SubLink
            if (node.GetAttributeValue("class", "").Contains("link"))
            {
                var a = node.SelectSingleNode(".//h3[@class='nodeTitle']/a");
                if (a == null) return null;
                var title = WebUtility.HtmlDecode(a.InnerText);
                var nullableid = GetIdFromNodeClass(node);
                if (nullableid == null) return null;
                var id = nullableid.Value;
                var description = WebUtility.HtmlDecode(a.OwnerDocument.GetElementbyId($"nodeDescription-{id}")?.InnerText ?? string.Empty);
                return new SubLink(Forum, id, Parent, title, description);
            }

            //SubForum
            if (node.GetAttributeValue("class", "").Contains("forum"))
            {
                var nullableid = GetIdFromNodeClass(node);
                if (nullableid == null) return null;
                var id = nullableid.Value;
                var title = WebUtility.HtmlDecode(node.SelectSingleNode(".//div[@class='nodeText']/h3[@class='nodeTitle']/a")?.InnerText ?? string.Empty);
                var description = WebUtility.HtmlDecode(node.OwnerDocument.GetElementbyId($"nodeDescription-{id}")?.InnerText ?? string.Empty);
                var stats = node.SelectNodes(".//div[@class='nodeText']//dd")?.ToList() ?? new List<HtmlNode>();
                var postcount = GetSubForumStatsNumbers(stats.Count >= 2 ? stats[1].InnerText : "-");
                return new SubForum(Forum, id, Parent, title, description, postcount);
            }

            return null;
        }

        private static long? GetIdFromNodeClass(HtmlNode node)
        {
            var classes = node.GetAttributeValue("class", "");
            var regex = Regex.Match(classes, "node_(\\d+)");
            return Regex.IsMatch(classes, "node_(\\d+)")
                ? (long?)long.Parse(regex.Groups[1].Value)
                : null;
        }

        private static long? GetSubForumStatsNumbers(string s)
            => long.TryParse(s.Replace(".", ""), out var v) ? (long?)v : null;
    }
}
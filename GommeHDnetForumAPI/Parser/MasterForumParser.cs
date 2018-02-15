using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using GommeHDnetForumAPI.DataModels;
using GommeHDnetForumAPI.DataModels.Collections;
using GommeHDnetForumAPI.DataModels.Entities;
using HtmlAgilityPack;

namespace GommeHDnetForumAPI.Parser
{
    internal class MasterForumParser : Parser<MasterForumInfo>
    {
        private const string RegexNodeClass = "node_(\\d+)";

        public MasterForumParser(Forum forum) : base(forum, new BasicUrl(ForumPaths.ForumPath)) { }

        public override async Task<MasterForumInfo> ParseAsync() {
            var doc = await GetDoc().ConfigureAwait(false);
            var categories = new DataModelCollection<MasterForumCategoryInfo>();

            categories.AddRange((from li in doc.DocumentNode.SelectNodes("//ol[@id='forums']/li")
                let nullableid = GetIdFromNodeClass(li)
                where nullableid != null
                let id = nullableid.Value
                let titlenodes = li.SelectNodes(".//div[@class='categoryText']/h3/a")
                where titlenodes.Any()
                let title = WebUtility.HtmlDecode(titlenodes[0].InnerText)
                let description = doc.GetElementbyId($"nodeDescription-{id}")?.InnerText ?? string.Empty
                let mfci = new MasterForumCategoryInfo(Forum, id, title, description)
                let subforums = GetSubForums(li.SelectNodes(".//ol[@class='nodeList']/li").ToList(), mfci)
                select (mfci, subforums)).Select(a => {
                a.mfci.SubForums = a.subforums;
                return a.mfci;
            }));

            return new MasterForumInfo(Forum, categories);
        }

        private DataModelCollection<ISubForum> GetSubForums(IReadOnlyCollection<HtmlNode> hnodes, MasterForumCategoryInfo mfci)
        {
            var dmc = new DataModelCollection<ISubForum>();
            //SubLinks
            dmc.AddRange(from li in hnodes.Where(n => n.GetAttributeValue("class", "").Contains("link"))
                let a = li.SelectSingleNode(".//h3[@class='nodeTitle']/a")
                where a != null
                let title = WebUtility.HtmlDecode(a.InnerText)
                let nullableid = GetIdFromNodeClass(li)
                where nullableid != null
                let id = nullableid.Value
                let description = a.OwnerDocument.GetElementbyId($"nodeDescription-{id}")?.InnerText
                select new SubLink(Forum, id, mfci, title, description));

            //SubForums
            //dmc.AddRange(from li in hnodes.Where(n => n.GetAttributeValue("class", "").Contains("forum"))
            //             let nullableid = GetIdFromNodeClass(li)
            //             where nullableid != null
            //             let id = nullableid.Value
            //             let title = li.SelectSingleNode(".//div[@class='nodeText']/h3[@class='nodeTitle']/a")?.InnerText
            //             let description = li.OwnerDocument.GetElementbyId($"#nodeDescription-{id}")?.InnerText
            //             let stats = li.SelectNodes(".//div[@class='nodeStats']/dl/dd")
            //             where stats?.Count >= 2
            //             let threadcount = GetSubForumStatsNumbers(stats[0].InnerText)
            //             let postcount = GetSubForumStatsNumbers(stats[1].InnerText)
            //             let subforumnode = li.SelectSingleNode(".//div[@class='nodeStats']/div/a")
            //             let sfcount = subforumnode == null ? 0 : long.Parse(subforumnode.ChildNodes.First(n => n is HtmlTextNode).InnerText)
            //             select new SubForum(Forum, id, mfci, title, description, threadcount, postcount, sfcount));
            foreach (var li in hnodes.Where(n => n.GetAttributeValue("class", "").Contains("forum")))
            {
                var nullableid = GetIdFromNodeClass(li);
                if (nullableid == null) continue;
                var id = nullableid.Value;
                var title = li.SelectSingleNode(".//div[@class='nodeText']/h3[@class='nodeTitle']/a")?.InnerText;
                var description = li.OwnerDocument.GetElementbyId($"nodeDescription-{id}")?.InnerText ?? string.Empty;
                var stats = li.SelectNodes(".//div[@class='nodeText']//dd")?.ToList() ?? new List<HtmlNode>();
                var threadcount = GetSubForumStatsNumbers(stats.Any() ? stats[0].InnerText : "-");
                var postcount = GetSubForumStatsNumbers(stats.Count >= 2 ? stats[1].InnerText : "-");
                var subforumnode = li.SelectSingleNode(".//div[@class='nodeStats']/div/a");
                var sfcount = subforumnode == null ? 0 : long.Parse(subforumnode.ChildNodes.First(n => n is HtmlTextNode).InnerText);
                dmc.Add(new SubForum(Forum, id, mfci, title, description, threadcount, postcount, sfcount));
            }

            return dmc;
        }

        private static long? GetIdFromNodeClass(HtmlNode node) {
            var classes = node.GetAttributeValue("class", "");
            var regex = Regex.Match(classes, RegexNodeClass);
            return Regex.IsMatch(classes, RegexNodeClass)
                ? (long?) long.Parse(regex.Groups[1].Value)
                : null;
        }

        private static long? GetSubForumStatsNumbers(string s) 
            => long.TryParse(s.Replace(".", ""), out var v) ? (long?) v : null;
    }
}
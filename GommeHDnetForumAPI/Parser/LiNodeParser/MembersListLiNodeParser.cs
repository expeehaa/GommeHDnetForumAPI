using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using GommeHDnetForumAPI.DataModels.Entities;
using HtmlAgilityPack;

namespace GommeHDnetForumAPI.Parser.LiNodeParser
{
    internal class MembersListLiNodeParser : LiNodeParser<UserInfo, object>
    {
        public MembersListLiNodeParser(Forum forum, IEnumerable<HtmlNode> liNodes) : base(forum, liNodes, null)
        {
        }

        protected override UserInfo ParseElement(HtmlNode node)
        {
            var aNode = node.SelectSingleNode("./a");
            var aNodeClasses = aNode.GetAttributeValue("class", "").Split(" ");
            var avatarClass = aNodeClasses.FirstOrDefault(c => Regex.IsMatch(c, "Av([0-9]+)s"));
            if (string.IsNullOrWhiteSpace(avatarClass) || !long.TryParse(Regex.Match(avatarClass, "Av([0-9]+)s").Groups[1].Value, out var userId)) return null;
            var username = node.SelectSingleNode(".//div[@class='member']/h3[@class='username']")?.InnerText;
            if (string.IsNullOrWhiteSpace(username)) return null;
            var userInfoNode = node.SelectSingleNode(".//div[@class='member']/div[@class='userInfo']");
            var userTitle = userInfoNode.SelectSingleNode(".//span[@class='userTitle']")?.InnerText;
            var numbersNodes = userInfoNode.SelectNodes(".//dl[@class='userStats pairsInline']/dd");
            if (numbersNodes.Count < 3) return null;
            if (!int.TryParse(numbersNodes[0].InnerText.Replace(".", ""), out var posts)) return null;
            if (!int.TryParse(numbersNodes[1].InnerText.Replace(".", ""), out var likes)) return null;
            if (!int.TryParse(numbersNodes[2].InnerText.Replace(".", ""), out var trophies)) return null;

            return new UserInfo(Forum, userId, username)
            {
                PostCount = posts,
                LikeCount = likes,
                Trophies = trophies,
                UserTitle = userTitle
            };
        }
    }
}
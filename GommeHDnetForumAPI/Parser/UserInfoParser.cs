using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GommeHDnetForumAPI.DataModels;
using GommeHDnetForumAPI.DataModels.Entities;
using GommeHDnetForumAPI.DataModels.Exceptions;
using HtmlAgilityPack;

namespace GommeHDnetForumAPI.Parser
{
    internal class UserInfoParser : Parser<UserInfo>
    {
        private readonly bool _ignoreLoginCheck;

        public UserInfoParser(Forum forum, ForumUrlPathString urlpath, bool ignoreLoginCheck = false) : base(forum, urlpath) {
            _ignoreLoginCheck = ignoreLoginCheck;
        }

        public UserInfoParser(Forum forum, long userId) : base(forum, new ForumUrlPathString("forum/members/" + userId)) {
        }

        public UserInfoParser(Forum forum, string html) : base(forum, html) {
        }

        public override async Task<UserInfo> ParseAsync()
        {
            var htmldata = Html;
            if (string.IsNullOrWhiteSpace(Html))
            {
                if (!Forum.LoggedIn && !_ignoreLoginCheck) throw new LoginRequiredException("Getting conversation messages needs login!");
                var hrm = await Forum.GetData(Url, false);
                if (!hrm.IsSuccessStatusCode) return null;
                htmldata = await hrm.Content.ReadAsStringAsync().ConfigureAwait(false);
            }
            var doc = new HtmlDocument();
            doc.LoadHtml(htmldata);
            var profilePage = doc.DocumentNode.SelectSingleNode("//div[@class='profilePage']");
            if(profilePage == null) throw new NodeNotFoundException("ProfilePage node not found!");
            var userId = doc.GetElementbyId("get-premium").GetAttributeValue("data-user-id", 0);
            var username = profilePage.SelectSingleNode(".//h1[@class='username']").InnerText.Trim();
            var avatarUrl = profilePage.SelectSingleNode(".//div[@class='avatarScaler']/img").GetAttributeValue("src", "");
            var status = doc.GetElementbyId("UserStatus")?.FirstChild.InnerText.Trim();
            var infoNodes = profilePage.SelectNodes(".//div[@class='section infoBlock']//dl");
            var infos = (from node in infoNodes
                where node.FirstChild.Name.Equals("dt", StringComparison.OrdinalIgnoreCase) && node.LastChild.Name.Equals("dd", StringComparison.OrdinalIgnoreCase)
                select new KeyValuePair<string, HtmlNode>(node.FirstChild.InnerText, node.LastChild)).ToList();
            var gotPosts = int.TryParse(infos.FirstOrDefault(p => p.Key.ToLower().Contains("beiträge")).Value?.InnerText.Replace(".", ""), out var posts);
            var gotLikes = int.TryParse(infos.FirstOrDefault(p => p.Key.ToLower().Contains("zustimmungen")).Value?.InnerText.Replace(".", ""), out var likes);
            var gotTrophies = int.TryParse(infos.FirstOrDefault(p => p.Key.ToLower().Contains("erfolge")).Value?.InnerText.Replace(".", ""), out var trophies);
            var location = infos.FirstOrDefault(p => p.Key.ToLower().Contains("ort")).Value?.FirstChild?.InnerText.Trim();
            var gender = GenderParser.Parse(infos.FirstOrDefault(p => p.Value.GetAttributeValue("itemprop", "").Equals("gender", StringComparison.OrdinalIgnoreCase)).Value?.InnerText);
            var verified = doc.GetElementbyId("statistic").SelectSingleNode("./div[@class='']/div[@class='stat-table']") == null;

            return new UserInfo(Forum, userId, username) {
                AvatarUrl = avatarUrl,
                Status = status,
                PostCount = gotPosts ? (int?) posts : null,
                LikeCount = gotLikes ? (int?) likes : null,
                Trophies = gotTrophies ? (int?) trophies : null,
                Verified = verified,
                Location = location,
                Gender = gender
            };
        }
    }
}

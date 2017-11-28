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
            var username = profilePage.SelectSingleNode(".//h1[@class='username']").InnerText;
            var avatarUrl = profilePage.SelectSingleNode(".//div[@class='avatarScaler']/img").GetAttributeValue("src", "");
            var status = doc.GetElementbyId("UserStatus")?.InnerText;
            var infoNodes = profilePage.SelectNodes(".//div[@class='section infoBlock']/div/dl/dd");
            if (infoNodes.Count < 5) throw new NodeNotFoundException("Some info section nodes are missing!");
            var posts = int.Parse(infoNodes.ElementAt(2).InnerText);
            var likes = int.Parse(infoNodes.ElementAt(3).InnerText);
            var trophies = int.Parse(infoNodes.ElementAt(4).InnerText);
            var verified = doc.GetElementbyId("statistic").SelectSingleNode("./div[@class='']/div[@class='stat-table']") == null;

            return new UserInfo(Forum, userId, username) {
                AvatarUrl = avatarUrl,
                Status = status,
                PostCount = posts,
                LikeCount = likes,
                Trophies = trophies,
                Verified = verified
            };
        }
    }
}

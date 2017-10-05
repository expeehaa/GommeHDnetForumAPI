using System.Threading.Tasks;
using GommeHDnetForumAPI.DataModels;
using GommeHDnetForumAPI.DataModels.Entities;
using HtmlAgilityPack;

namespace GommeHDnetForumAPI.Parser
{
    internal class UserInfoParser : Parser<UserInfo>
    {
        public UserInfoParser(Forum forum, long userId) : base(forum, new ForumUrlPathString(Forum.ForumUrl + "members/" + userId)) {
        }

        public override async Task<UserInfo> ParseAsync()
        {
            var hrm = await Forum.GetData(Url);
            var doc = new HtmlDocument();
            doc.LoadHtml(await hrm.Content.ReadAsStringAsync());
            return null;
            //return new UserInfo(Forum, _userid, );
        }
    }
}

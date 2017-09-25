using System.Threading.Tasks;
using GommeHDnetForumAPI.DataModels.Entities;
using HtmlAgilityPack;

namespace GommeHDnetForumAPI.Parser
{
    internal class UserInfoParser : Parser<UserInfo>
    {
        private readonly UserInfo _userInfo;

        public UserInfoParser(Forum forum, UserInfo userInfo) : base(forum)
        {
            _userInfo = userInfo;
        }

        public override async Task<UserInfo> ParseAsync()
        {
            var hrm = await Forum.GetData(_userInfo.Url);
            var doc = new HtmlDocument();
            doc.LoadHtml(await hrm.Content.ReadAsStringAsync());

            return _userInfo;
        }
    }
}

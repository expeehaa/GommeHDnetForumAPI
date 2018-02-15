using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using GommeHDnetForumAPI.DataModels;
using GommeHDnetForumAPI.DataModels.Collections;
using GommeHDnetForumAPI.DataModels.Entities;
using HtmlAgilityPack;

namespace GommeHDnetForumAPI.Parser
{
    /// <summary>
    /// Parse conversations from https://gommehd.net/forum/conversations/
    /// </summary>
    internal class ConversationsParser : Parser<Conversations>
    {
        private readonly int _startPage, _pageCount;

        public ConversationsParser(Forum forum, int startPage, int pageCount) : base(forum, new BasicUrl(ForumPaths.ConversationsUrl)) {
            _startPage = startPage < 1 ? 1 : startPage;
            _pageCount = pageCount;
        }

        public override async Task<Conversations> ParseAsync() {
            var doc = await GetDoc().ConfigureAwait(false);
            var pages = doc.DocumentNode.SelectSingleNode("//div[@class='PageNav']")?.GetAttributeValue("data-last", 0) ?? 1;

            if (pages < _startPage) return new Conversations();
            var pageMax = _pageCount <= 0 ? pages : (_startPage + _pageCount - 1 >= pages ? pages : _startPage + _pageCount - 1);

            var conversations = new Conversations();

            for (var i = _startPage; i <= pageMax; i++) {
                var hrm = await Forum.GetData(Url + "?page=" + i).ConfigureAwait(false);
                doc = new HtmlDocument();
                doc.LoadHtml(await hrm.Content.ReadAsStringAsync().ConfigureAwait(false));
                var nodes = doc.DocumentNode.SelectNodes("//ol[@class='discussionListItems']/li");

                conversations.AddRange(from node in nodes
                    where Regex.IsMatch(node.Id, "conversation-([0-9]+)")
                    let conid = long.Parse(Regex.Match(node.Id, "conversation-([0-9]+)").Groups[1].Value)
                    let contitle = node.SelectSingleNode(".//h3[@class='title']/a").InnerText
                    let members = new UserCollection(from mnode in node.SelectNodes(".//div[@class='secondRow']/div/a[@class='username']")
                        let murl = ForumPaths.ForumPath + mnode.GetAttributeValue("href", "")
                        let mid = long.Parse(Regex.Match(murl, ".+\\.([0-9]+)").Groups[1].Value)
                        select new UserInfo(Forum, mid, mnode.InnerText))
                    let conauthorname = node.GetAttributeValue("data-author", "")
                    let conauthor = members.First(m => m.Username == conauthorname)
                    let conanswers = uint.Parse(node.SelectSingleNode("div/dl[@class='major']/dd").InnerText)
                    select new ConversationInfo(Forum, conid, contitle, conauthor, members, conanswers));
            }

            return conversations;
        }
    }
}

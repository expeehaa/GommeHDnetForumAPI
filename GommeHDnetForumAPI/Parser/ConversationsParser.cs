using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using GommeHDnetForumAPI.DataModels.Collections;
using GommeHDnetForumAPI.DataModels.Entities;
using GommeHDnetForumAPI.DataModels.Exceptions;
using HtmlAgilityPack;

namespace GommeHDnetForumAPI.Parser
{
    internal class ConversationsParser : Parser<Conversations>
    {
        private const string Url = "forum/conversations/";
        private readonly int _startPage, _pageCount;

        public ConversationsParser(Forum forum, int startPage, int pageCount) : base(forum) {
            _startPage = startPage < 1 ? 1 : startPage;
            _pageCount = pageCount;
        }

        public override async Task<Conversations> ParseAsync()
        {
            if(!Forum.LoggedIn) throw new LoginRequiredException("Getting conversations needs login!");

            var hrm = await Forum.GetData(Url);
            var doc = new HtmlDocument();
            doc.LoadHtml(await hrm.Content.ReadAsStringAsync());

            var pages = doc.DocumentNode.SelectSingleNode("//div[@class='PageNav']")?.GetAttributeValue("data-last", 0) ?? 1;

            if (pages < _startPage) return new Conversations();
            var pageMax = _pageCount <= 0 ? pages : (_startPage + _pageCount - 1 >= pages ? pages : _startPage + _pageCount - 1);

            var conversations = new Conversations();

            for (var i = _startPage; i <= pageMax; i++) {
                hrm = await Forum.GetData(Url + "?page=" + i);
                doc = new HtmlDocument();
                doc.LoadHtml(await hrm.Content.ReadAsStringAsync());
                var nodes = doc.DocumentNode.SelectNodes("//ol[@class='discussionListItems']/li");
                
                foreach (var node in nodes)
                {
                    if (!Regex.IsMatch(node.Id, "conversation-([0-9]+)"))
                        continue;
                    var conId = long.Parse(Regex.Match(node.Id, "conversation-([0-9]+)").Groups[1].Value);
                    var title = node.SelectSingleNode("div/div/h3/a").InnerText;
                    var url = "forum/" + node.SelectSingleNode("div/div/h3/a").GetAttributeValue("href", "");
                    var authorname = node.GetAttributeValue("data-author", "");
                    var membersBuffer = from mnode in node.SelectNodes("div/div/div/div/a[@class='username']")
                        let url2 = "forum/" + mnode.GetAttributeValue("href", "")
                        let id = long.Parse(Regex.Match(url2, ".+\\.([0-9]+)").Groups[1].Value)
                        select new UserInfo(Forum, id, mnode.InnerText, url2);
                    var members = new UserCollection();
                    foreach (var userInfo in membersBuffer)
                        members.Add(userInfo);
                    var author = members.First(u => u.Username == authorname);
                    var answerCount = uint.Parse(node.SelectSingleNode("div/dl[@class='major']/dd").InnerText);
                    conversations.Add(new ConversationInfo(Forum, conId, title, url, author, members, answerCount));
                }
            }

            return conversations;
        }
    }
}

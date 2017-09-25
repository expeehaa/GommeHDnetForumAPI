using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using GommeHDnetForumAPI.DataModels;
using GommeHDnetForumAPI.DataModels.Collections;
using GommeHDnetForumAPI.DataModels.Entities;
using GommeHDnetForumAPI.DataModels.Exceptions;
using HtmlAgilityPack;

namespace GommeHDnetForumAPI.Parser
{
    internal class ConversationMessageParser : Parser<ConversationMessages>
    {
        private readonly ForumRedirectUrlString _url;
        private readonly int _startPage, _pageCount;

        public ConversationMessageParser(Forum forum, ForumRedirectUrlString url, int startPage, int pageCount) : base(forum)
        {
            _url = url;
            _pageCount = pageCount;
            _startPage = startPage;
        }

        public override async Task<ConversationMessages> ParseAsync()
        {
            if (!Forum.LoggedIn) throw new LoginRequiredException("Getting conversation messages needs login!");

            var hrm = await Forum.GetData(_url);
            var doc = new HtmlDocument();
            doc.LoadHtml(await hrm.Content.ReadAsStringAsync());

            var pages = doc.DocumentNode.SelectSingleNode("//div[@class='PageNav']")?.GetAttributeValue("data-last", 0) ?? 1;

            if (pages - _startPage < 0) return new ConversationMessages();
            var pageMax = _pageCount <= 0 ? pages : (_startPage + _pageCount - 1 >= pages ? pages : _startPage + _pageCount - 1);

            var messages = new ConversationMessages();

            for (var i = _startPage; i <= pageMax; i++) {
                hrm = await Forum.GetData(_url + "page-" + i);
                doc = new HtmlDocument();
                doc.LoadHtml(await hrm.Content.ReadAsStringAsync());
                var nodes = doc.DocumentNode.SelectNodes("//ol[@id='messageList']/li");
                
                foreach (var node in nodes)
                {
                    if (!Regex.IsMatch(node.Id, "message-([0-9]+)"))
                        continue;
                    var id = long.Parse(Regex.Match(node.Id, "message-([0-9]+)").Groups[1].Value);
                    var author = node.GetAttributeValue("data-author", "");
                    var authorurl = "forum/" + node.SelectSingleNode("div/div/h3/a[@class='username']").GetAttributeValue("href", "");
                    var authorid = long.Parse(Regex.Match(authorurl, ".+\\.([0-9]+)").Groups[1].Value);
                    var content = (from n in node.SelectSingleNode("div/div/article/blockquote").ChildNodes where n.GetAttributeValue("class", "") != "bbCodeBlock bbCodeQuote" && n.GetAttributeValue("class", "") != "messageTextEndMarker" select n.InnerText).Aggregate("", (s, t) => s + t).Trim('\n', ' ');
                    messages.Add(new ConversationMessage(id, new UserInfo(Forum, authorid, author, authorurl), content));
                }
            }

            return messages;
        }
    }
}

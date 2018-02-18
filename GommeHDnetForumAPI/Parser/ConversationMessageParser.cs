using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using GommeHDnetForumAPI.DataModels;
using GommeHDnetForumAPI.DataModels.Entities;
using GommeHDnetForumAPI.DataModels.Exceptions;
using HtmlAgilityPack;

namespace GommeHDnetForumAPI.Parser
{
    internal class ConversationMessageParser : Parser<IEnumerable<ConversationMessage>>
    {
        /// <summary>
        /// Start page for parser
        /// </summary>
        private readonly int _startPage;

        /// <summary>
        /// Page count
        /// </summary>
        private readonly int _pageCount;

        private readonly ConversationInfo _parent;

        public ConversationMessageParser(Forum forum, BasicUrl url, int startPage, int pageCount, ConversationInfo parent) : base(forum, url) {
            _startPage = startPage;
            _pageCount = pageCount;
            _parent = parent;
        }

        public ConversationMessageParser(Forum forum, string html, ConversationInfo parent) : base(forum, html) {
            _parent = parent;
        }

        public override async Task<IEnumerable<ConversationMessage>> ParseAsync() {
            HtmlDocument doc;
            switch (Content)
            {
                case ParserContent.Html:
                    doc = new HtmlDocument();
                    doc.LoadHtml(Html);
                    break;
                case ParserContent.Url:
                    doc = await GetDoc().ConfigureAwait(false);
                    break;
                case ParserContent.HttpResponseMessage:
                    doc = new HtmlDocument();
                    doc.LoadHtml(await HttpResponse.Content.ReadAsStringAsync().ConfigureAwait(false));
                    break;
                default:
                    throw new ParserContentNotSupportedException(null, Content);
            }

            var messages = new List<ConversationMessage>();

            if (Content == ParserContent.Url) {
                var pages = doc.DocumentNode.SelectSingleNode("//div[@class='PageNav']")?.GetAttributeValue("data-last", 0) ?? 1;
                if (pages - _startPage < 0) return new List<ConversationMessage>();
                var pageMax = _pageCount <= 0 ? pages : (_startPage + _pageCount - 1 >= pages ? pages : _startPage + _pageCount - 1);

                for (var i = _startPage; i <= pageMax; i++) {
                    var hrm = await Forum.GetData(Url + "page-" + i).ConfigureAwait(false);
                    doc = new HtmlDocument();
                    doc.LoadHtml(await hrm.Content.ReadAsStringAsync().ConfigureAwait(false));
                    messages.AddRange(ParseMessages(doc.DocumentNode.SelectNodes("//ol[@id='messageList']/li")));
                }
            }
            else messages.AddRange(ParseMessages(doc.DocumentNode.SelectNodes("//ol[@id='messageList']/li")));
            return messages;
        }

        private IEnumerable<ConversationMessage> ParseMessages(HtmlNodeCollection liCollection) 
            => new List<ConversationMessage>(from node in liCollection
                where Regex.IsMatch(node.Id, "message-([0-9]+)")
                let id = long.Parse(Regex.Match(node.Id, "message-([0-9]+)").Groups[1].Value)
                let authornode = node.SelectSingleNode(".//h3[@class='userText']/a[@class='username']")
                let authorname = authornode.InnerText
                let authorurl = authornode.GetAttributeValue("href", "")
                let authorid = string.IsNullOrWhiteSpace(authorurl) ? 0 : long.Parse(authorurl.Split('.')[authorurl.Split('.').Length - 1].TrimEnd('/'))
                let content = node.SelectSingleNode(".//div[@class='messageContent']/article/blockquote").InnerText.Replace("&nbsp;", " ").Trim()
                select new ConversationMessage(Forum, id, new UserInfo(Forum, authorid, authorname), content, _parent));
    }
}

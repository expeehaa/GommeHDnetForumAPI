using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using GommeHDnetForumAPI.DataModels;
using GommeHDnetForumAPI.DataModels.Collections;
using GommeHDnetForumAPI.DataModels.Entities;
using GommeHDnetForumAPI.Exceptions;
using HtmlAgilityPack;

namespace GommeHDnetForumAPI.Parser
{
    internal class ConversationInfoParser : Parser<ConversationInfo>
    {
        /// <summary>
        /// Prepare Parser to parse a given HTML string to ConversationInfo
        /// </summary>
        /// <param name="forum">Forum instance</param>
        /// <param name="html">Html to use in ParseAsync</param>
        public ConversationInfoParser(Forum forum, string html) : base(forum, html) { }
        
        ///// <summary>
        ///// Prepare Parser to download HTML from url and parse that to ConversationInfo
        ///// </summary>
        ///// <param name="forum">Forum instance</param>
        ///// <param name="url">Url to download HTML from.</param>
        //public ConversationInfoParser(Forum forum, BasicUrl url) : base(forum, url) { }

        ///// <summary>
        ///// Prepare Parser to read HTML from an HttpResponseMessage object and parse that to ConversationInfo
        ///// </summary>
        ///// <param name="forum">Forum instance</param>
        ///// <param name="hrm">HttpResponseMessage object</param>
        //public ConversationInfoParser(Forum forum, HttpResponseMessage hrm) : base(forum, hrm) { }


        public override async Task<ConversationInfo> ParseAsync() {
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
                case ParserContent.None:
                    throw new ParserContentNotSupportedException(null, Content);
                default:
                    throw new ParserContentNotSupportedException(null, Content);
            }

            var container = doc.DocumentNode.SelectSingleNode("//div[@class='mainContainer']");

            var idstr = container?.SelectSingleNode("//div[@class='pageNavLinkGroup']/div[@class='linkGroup']/a")?.GetAttributeValue("href", "");
            if (string.IsNullOrWhiteSpace(idstr)) return null;
            var idstrar = idstr.Split('/');
            var id = long.Parse(idstrar[idstrar.Length - 2].Substring(idstrar[idstrar.Length - 2].LastIndexOf(".", StringComparison.OrdinalIgnoreCase) + 1));
            var title = container.SelectSingleNode("//div[@class='titleBar']/h1").InnerText;

            var messages = (await new ConversationMessageParser(Forum, null, doc.DocumentNode.OuterHtml).ParseAsync().ConfigureAwait(false)).ToList();
            if (!messages.Any()) return null;
            var author = messages[0].Author;
            var recipients = new UserCollection(from li in container.SelectNodes("//ul[@id='ConversationRecipients']/li")
                let a = li.SelectSingleNode("//a[@class='username']")
                let username = a.InnerText
                let userurl = a.GetAttributeValue("href", "")
                let userid = long.Parse(userurl.Split('.')[userurl.Split('.').Length - 1].TrimEnd('/'))
                select new UserInfo(Forum, userid));
            var answers = uint.Parse(container.SelectNodes("//div[@class='secondaryContent']/div[@class='pairsJustified']/dl")[1].InnerText);

            return new ConversationInfo(Forum, id, title, author, recipients, answers);
        }
    }
}
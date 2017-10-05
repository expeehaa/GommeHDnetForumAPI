using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GommeHDnetForumAPI.DataModels;
using GommeHDnetForumAPI.DataModels.Collections;
using GommeHDnetForumAPI.DataModels.Entities;
using GommeHDnetForumAPI.DataModels.Exceptions;
using HtmlAgilityPack;

namespace GommeHDnetForumAPI.Parser
{
    internal class ConversationInfoParser : Parser<ConversationInfo>
    {
        /// <summary>
        /// Download HTML from URL and parse to ConversationInfo
        /// </summary>
        /// <param name="forum">Forum instance</param>
        /// <param name="url">url to first page of conversation</param>
        public ConversationInfoParser(Forum forum, ForumUrlPathString url) : base(forum, url) {}

        /// <summary>
        /// Parse a ConversationInfo from HTML
        /// </summary>
        /// <param name="forum">Forum instance</param>
        /// <param name="html">html of first page of conversation</param>
        public ConversationInfoParser(Forum forum, string html) : base(forum, html) {}

        public override async Task<ConversationInfo> ParseAsync() {
            var con = Html;
            if (string.IsNullOrWhiteSpace(Html)) {
                if (!Forum.LoggedIn) throw new LoginRequiredException();

                var hrm = await Forum.GetData(Url);
                con = await hrm.Content.ReadAsStringAsync();
            }

            var doc = new HtmlDocument();
            doc.LoadHtml(con);

            var container = doc.DocumentNode.SelectSingleNode("//div[@class='mainContainer']");

            var idstr = container?.SelectSingleNode("//div[@class='pageNavLinkGroup']/div[@class='linkGroup']/a")?.GetAttributeValue("href", "");
            if (string.IsNullOrWhiteSpace(idstr)) return null;
            var idstrar = idstr.Split('/');
            var id = long.Parse(idstrar[idstrar.Length - 2].Substring(idstrar[idstrar.Length - 2].LastIndexOf(".", StringComparison.OrdinalIgnoreCase) + 1));

            var title = container.SelectSingleNode("//div[@class='titleBar']/h1").InnerText;
            var url = string.Join("/", idstrar.Except(new List<string> { idstrar[idstrar.Length - 1] }));

            var messages = await new ConversationMessageParser(Forum, con).ParseAsync();
            if (!messages.Any()) return null;
            var author = messages[0].Author;
            var recipients = new UserCollection(from li in container.SelectNodes("//ul[@id='ConversationRecipients']/li")
                let a = li.SelectSingleNode("//a[@class='username']")
                let username = a.InnerText
                let userurl = a.GetAttributeValue("href", "")
                let userid = long.Parse(userurl.Split('.')[userurl.Split('.').Length - 1].TrimEnd('/'))
                select new UserInfo(Forum, userid));
            var answers = uint.Parse(container.SelectSingleNode("//div[@class='secondaryContent']/div[@class='pairsJustified']").SelectSingleNode("//dl/dt[@]").InnerText);

            return new ConversationInfo(Forum, id, title, url, author, recipients, answers);
        }
    }
}
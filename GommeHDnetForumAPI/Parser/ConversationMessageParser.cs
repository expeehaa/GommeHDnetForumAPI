using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using GommeHDnetForumAPI.Conversation;
using HtmlAgilityPack;

namespace GommeHDnetForumAPI.Parser
{
    internal class ConversationMessageParser : Parser<ConversationMessages>
    {
        private readonly ConversationInfo _conversationInfo;

        public ConversationMessageParser(Forum forum, ConversationInfo conversationInfo) : base(forum)
        {
            _conversationInfo = conversationInfo;
        }

        public override async Task<ConversationMessages> ParseAsync()
        {
            var hrm = await Forum.GetHttpResponseMessage(_conversationInfo.Url);
            var doc = new HtmlDocument();
            doc.LoadHtml(await hrm.Content.ReadAsStringAsync());
            var nodes = doc.DocumentNode.SelectNodes("//ol[@id='messageList']/li");
            var messages = new ConversationMessages();
            foreach (var node in nodes) {
                if (!Regex.IsMatch(node.Id, "message-([0-9]+)"))
                    continue;
                var id = long.Parse(Regex.Match(node.Id, "message-([0-9]+)").Groups[1].Value);
                var author = node.GetAttributeValue("data-author", "");
                var content = (from n in node.SelectSingleNode("div/div/article/blockquote").ChildNodes where n.GetAttributeValue("class", "") != "bbCodeBlock bbCodeQuote" && n.GetAttributeValue("class", "") != "messageTextEndMarker" select n.InnerText).Aggregate("", (s, t) => s + t).Trim('\n', ' ');
                messages.Add(new ConversationMessage(id, author, content));
            }
            return messages;
        }
    }
}

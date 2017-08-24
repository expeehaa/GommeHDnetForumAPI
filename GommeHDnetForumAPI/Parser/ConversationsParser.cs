using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using GommeHDnetForumAPI.Conversation;
using HtmlAgilityPack;

namespace GommeHDnetForumAPI.Parser
{
    internal class ConversationsParser : Parser<Conversations>
    {
        public ConversationsParser(Forum forum) : base(forum)
        {
        }

        public override async Task<Conversations> ParseAsync()
        {
            var hrm = await Forum.GetHttpResponseMessage("/forum/conversations");
            var doc = new HtmlDocument();
            doc.LoadHtml(await hrm.Content.ReadAsStringAsync());
            var nodes = doc.DocumentNode.SelectNodes("//ol[@class='discussionListItems']/li");
            var conversations = new Conversations();
            foreach (var node in nodes) {
                if (!Regex.IsMatch(node.Id, "conversation-([0-9]+)"))
                    continue;
                var conId = long.Parse(Regex.Match(node.Id, "conversation-([0-9]+)").Groups[1].Value);
                var title = node.SelectSingleNode("div/div/h3/a").InnerText;
                var url = node.SelectSingleNode("div/div/h3/a").GetAttributeValue("href", "");
                var author = node.GetAttributeValue("data-author", "");
                var members = from mNode in node.SelectNodes("div/div/div/div/a[@class='username']") select mNode?.InnerText;
                var answerCount = uint.Parse(node.SelectSingleNode("div/dl[@class='major']/dd").InnerText);
                conversations.Add(new ConversationInfo(Forum, conId, title, url, author, members, answerCount));
            }
            return conversations;
        }
    }
}

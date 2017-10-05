using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GommeHDnetForumAPI.DataModels.Collections;
using GommeHDnetForumAPI.DataModels.Exceptions;
using GommeHDnetForumAPI.Parser;
using HtmlAgilityPack;

namespace GommeHDnetForumAPI.DataModels.Entities
{
    public class ConversationInfo : ForumEntity
    {
        /// <summary>
        /// Conversation title
        /// </summary>
        public string Title { get; }
        /// <summary>
        /// Conversation author
        /// </summary>
        public UserInfo Author { get; }
        /// <summary>
        /// Conversation members (including author)
        /// </summary>
        public UserCollection Members { get; }
        /// <summary>
        /// Number of answers
        /// </summary>
        public uint AnswerCount { get; }
        /// <summary>
        /// Message collection
        /// </summary>
        public ConversationMessages Messages { get; private set; }

        /// <summary>
        /// Internal constructor
        /// </summary>
        /// <param name="forum">Forum instance this object is assigned to.</param>
        /// <param name="id">Conversation ID</param>
        /// <param name="title">Conversation title</param>
        /// <param name="url">Conversation url relative to Forum.BaseUrl</param>
        /// <param name="author">Conversation author</param>
        /// <param name="members">Conversation members (including author)</param>
        /// <param name="answerCount">Number of answers</param>
        internal ConversationInfo(Forum forum, long id, string title, string url, UserInfo author, UserCollection members, uint answerCount) : base(forum, id, url)
        {
            Title = title;
            Author = author;
            Members = members;
            AnswerCount = answerCount;
            Messages = null;
        }

        /// <summary>
        /// Downloads all messages in a conversation.
        /// </summary>
        public async Task DownloadMessagesAsync() 
            => await DownloadMessagesAsync(1);

        /// <summary>
        /// Downloads conversation messages in a range of pages.
        /// </summary>
        /// <param name="startPage">First page, starting with 1.</param>
        /// <param name="pageCount">Number of pages. If 0 or less all pages from <paramref name="startPage"/> to last will be downloaded.</param>
        public async Task DownloadMessagesAsync(int startPage, int pageCount = 0) 
            => Messages = await new ConversationMessageParser(Forum, UrlPath, startPage, pageCount).ParseAsync();

        /// <summary>
        /// Sends a reply to the conversation if possible.
        /// </summary>
        /// <param name="message">The message to send</param>
        /// <returns>bool indicating wether replying was successful or not.</returns>
        public async Task<bool> Reply(string message) {
            if (!Forum.LoggedIn) throw new LoginRequiredException("Login required to reply to a conversation.");
            var h = await Forum.GetData(UrlPath);
            var doc = new HtmlDocument();
            doc.LoadHtml(await h.Content.ReadAsStringAsync());
            var qrnode = doc.GetElementbyId("QuickReply");
            if (qrnode == null) return false;
            var xftoken = qrnode.SelectSingleNode(".//input[@name='_xfToken']").GetAttributeValue("value", "");
            var xfrelativeresolver = qrnode.SelectSingleNode(".//input[@name='_xfRelativeResolver']").GetAttributeValue("value", "");

            var kvlist = new List<KeyValuePair<string, string>> {
                new KeyValuePair<string, string>("message_html", message),
                new KeyValuePair<string, string>("_xfToken", xftoken),
                new KeyValuePair<string, string>("_xfRelativeResolver", xfrelativeresolver)
            };
            var hrm = await Forum.PostData(UrlPath + "insert-reply", kvlist, false);
            var messages = await new ConversationMessageParser(Forum, await hrm.Content.ReadAsStringAsync()).ParseAsync();
            Messages.Add(messages.Last());
            return hrm.IsSuccessStatusCode;
        }

        public override string ToString() 
            => $"Id: {Id} | Title: \"{Title}\" | Author: \"{Author}\" | Answers: {AnswerCount} | Members: ({Members}) | Messages: ({Messages})";
    }
}

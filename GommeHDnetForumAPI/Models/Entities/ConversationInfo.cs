using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using GommeHDnetForumAPI.Exceptions;
using GommeHDnetForumAPI.Models.Collections;
using GommeHDnetForumAPI.Models.Entities.Interfaces;
using GommeHDnetForumAPI.Parser;
using HtmlAgilityPack;

namespace GommeHDnetForumAPI.Models.Entities {
	public class ConversationInfo : IndexedEntity, IThread<ConversationMessage> {
		public string                           Title       { get; }
		public UserInfo                         Author      { get; }
		public UserCollection                   Members     { get; }
		public uint                             AnswerCount { get; }
		public IEnumerable<ConversationMessage> Messages    { get; private set; }
		public string                           UrlPath     => $"{ForumPaths.ConversationsPath}{Id}/";

		internal ConversationInfo(Forum forum, long id, string title, UserInfo author, UserCollection members, uint answerCount) : base(forum, id) {
			Title       = title;
			Author      = author;
			Members     = members;
			AnswerCount = answerCount;
			Messages    = null;
		}

		public async Task DownloadMessagesAsync()
			=> await DownloadMessagesAsync(1).ConfigureAwait(false);

		public async Task DownloadMessagesAsync(int startPage, int pageCount = 0)
			=> Messages = (await new ConversationMessageParser(Forum, this, startPage, pageCount).ParseAsync().ConfigureAwait(false)).ToList();

		public async Task<bool> Reply(string message) {
			if (!Forum.LoggedIn) throw new LoginRequiredException("Login required to reply to a conversation.");
			var h   = await Forum.GetData(UrlPath).ConfigureAwait(false);
			var doc = new HtmlDocument();
			doc.LoadHtml(await h.Content.ReadAsStringAsync().ConfigureAwait(false));
			var qrnode = doc.GetElementbyId("QuickReply");
			if (qrnode == null) return false;

			var xftoken            = qrnode.SelectSingleNode(".//input[@name='_xfToken']").GetAttributeValue("value", "");
			var xfrelativeresolver = qrnode.SelectSingleNode(".//input[@name='_xfRelativeResolver']").GetAttributeValue("value", "");
			var attachment_hash    = qrnode.SelectSingleNode(".//input[@name='attachment_hash']").GetAttributeValue("value", "");
			var last_date          = qrnode.SelectSingleNode(".//input[@name='last_date']").GetAttributeValue("value", "");
			var last_known_date    = qrnode.SelectSingleNode(".//input[@name='last_known_date']").GetAttributeValue("value", "");

			var kvlist = new List<KeyValuePair<string, string>> {
				new KeyValuePair<string, string>("message_html",        message),
				new KeyValuePair<string, string>("_xfToken",            xftoken),
				new KeyValuePair<string, string>("_xfRelativeResolver", xfrelativeresolver),
				new KeyValuePair<string, string>("attachment_hash",     attachment_hash),
				new KeyValuePair<string, string>("last_date",           last_date),
				new KeyValuePair<string, string>("last_known_date",     last_known_date)
			};
			var hrm = await Forum.PostData(UrlPath + "insert-reply", kvlist, false).ConfigureAwait(false);
			await DownloadMessagesAsync().ConfigureAwait(false);

			if (!hrm.IsSuccessStatusCode && Regex.IsMatch(hrm.RequestMessage.RequestUri.Fragment, "#message-(\\d+)")) {
				var match = Regex.Match(hrm.RequestMessage.RequestUri.Fragment, "#message-(\\d+)").Groups[1].Value;
				var msgId = long.Parse(match);
				return Messages.Any(cm => cm.Id == msgId);
			} else {
				return hrm.IsSuccessStatusCode;
			}
		}

		public override string ToString()
			=> $"Id: {Id} | Title: \"{Title}\" | Author: \"{Author}\" | Answers: {AnswerCount} | Members: ({Members}) | Messages: ({string.Join(", ", Messages.Select(c => $"({c})"))})";
	}
}
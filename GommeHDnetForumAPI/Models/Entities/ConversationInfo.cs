using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using GommeHDnetForumAPI.Exceptions;
using GommeHDnetForumAPI.Models.Collections;
using GommeHDnetForumAPI.Models.Entities.Interfaces;
using GommeHDnetForumAPI.Parser.LiNodeParser;
using HtmlAgilityPack;

namespace GommeHDnetForumAPI.Models.Entities {
	public class ConversationInfo : IndexedEntity, IThread<ConversationMessage> {
		public string                           Title       { get; }
		public IUserInfo                        Author      { get; }
		public UserCollection                   Members     { get; }
		public uint                             AnswerCount { get; }
		public IEnumerable<ConversationMessage> Messages    { get; private set; }
		public string                           UrlPath     => $"{ForumPaths.ConversationsPath}{Id}/";

		internal ConversationInfo(Forum forum, long id, string title, IUserInfo author, UserCollection members, uint answerCount) : base(forum, id) {
			Title       = title;
			Author      = author;
			Members     = members;
			AnswerCount = answerCount;
			Messages    = new List<ConversationMessage>();
		}

		public async Task DownloadMessagesAsync()
			=> await DownloadMessagesAsync(1).ConfigureAwait(false);

		public async Task DownloadMessagesAsync(int startPage, int pageCount = 0) {
			startPage = Math.Max(1, startPage);

			var doc            = await Forum.GetHtmlDocument($"{UrlPath}?page={startPage}");
			var lastPageNumber = doc.DocumentNode.SelectSingleNode("//div[@class='PageNav']")?.GetAttributeValue("data-last", 0) ?? 1;
			var pageMax        = pageCount <= 0 ? lastPageNumber : Math.Min(startPage+pageCount-1, lastPageNumber);
			var docs           = new List<HtmlDocument>{ doc };

			for(var i = startPage; i <= pageMax; i++) {
				docs.Add(await Forum.GetHtmlDocument($"{ForumPaths.ConversationsPath}?page={i}"));
			}

			var parser = new ConversationMessagesLiNodeParser(Forum, this);
			Messages = docs.SelectMany(d => parser.Parse(d.DocumentNode)).ToList();
		}

		public async Task<bool> Reply(string message) {
			if (!Forum.LoggedIn) throw new LoginRequiredException("Login required to reply to a conversation.");
			var h   = await Forum.GetData(UrlPath).ConfigureAwait(false);
			var doc = new HtmlDocument();
			doc.LoadHtml(await h.Content.ReadAsStringAsync().ConfigureAwait(false));
			var qrnode = doc.GetElementbyId("QuickReply");
			if (qrnode == null) return false;

			var xftoken            = qrnode.GetInputValueByName("_xfToken");
			var xfrelativeresolver = qrnode.GetInputValueByName("_xfRelativeResolver");
			var attachment_hash    = qrnode.GetInputValueByName("attachment_hash");
			var last_date          = qrnode.GetInputValueByName("last_date");
			var last_known_date    = qrnode.GetInputValueByName("last_known_date");

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
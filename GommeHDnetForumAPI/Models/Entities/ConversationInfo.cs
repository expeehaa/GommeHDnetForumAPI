using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using GommeHDnetForumAPI.Exceptions;
using GommeHDnetForumAPI.Models.Collections;
using GommeHDnetForumAPI.Models.Entities.Interfaces;
using GommeHDnetForumAPI.Parser.NodeListParser;
using HtmlAgilityPack;

namespace GommeHDnetForumAPI.Models.Entities {
	public class ConversationInfo : IndexedEntity, IThread<ConversationMessage> {
		public string Title { get; }
		public UserInfo Author { get; }
		public UserCollection Members { get; }
		public uint AnswerCount { get; }
		public IEnumerable<ConversationMessage> Messages { get; private set; }
		public string UrlPath => $"{ForumPaths.ConversationsPath}{Id}/";

		internal ConversationInfo(Forum forum, long id, string title, UserInfo author, UserCollection members, uint answerCount) : base(forum, id) {
			Title = title;
			Author = author;
			Members = members;
			AnswerCount = answerCount;
			Messages = new List<ConversationMessage>();
		}

		public async Task DownloadMessagesAsync()
			=> await DownloadMessagesAsync(1).ConfigureAwait(false);

		public async Task DownloadMessagesAsync(int startPage, int pageCount = 0) {
			startPage = Math.Max(1, startPage);

			var doc                  = await Forum.GetHtmlDocument($"{UrlPath}page-{startPage}");
			var lastPageNumberString = doc.DocumentNode.SelectSingleNode("(//div[contains(concat(' ', normalize-space(@class), ' '), ' pageNav ')]/ul/li)[last()]")?.InnerText;
			var lastPageNumber       = !string.IsNullOrWhiteSpace(lastPageNumberString) ? int.Parse(lastPageNumberString) : 1;
			var pageMax              = pageCount <= 0 ? lastPageNumber : Math.Min(startPage+pageCount-1, lastPageNumber);
			var docs                 = new List<HtmlDocument>{ doc };

			for(var i = startPage + 1; i <= pageMax; i++) {
				docs.Add(await Forum.GetHtmlDocument($"{UrlPath}page-{i}"));
			}

			var parser = new ConversationMessagesNodeListParser(Forum, this);
			Messages = docs.SelectMany(d => parser.Parse(d.DocumentNode)).ToList();
		}

		public async Task<bool> Reply(string message) {
			if(!Forum.LoggedIn)
				throw new LoginRequiredException("Login required to reply to a conversation.");

			var doc      = await Forum.GetHtmlDocument(UrlPath);
			var formNode = doc.DocumentNode.SelectSingleNode("//form[contains(concat(' ', normalize-space(@class), ' '), ' js-quickReply ')]");

			if(formNode != null) {
				var xfToken                = formNode.GetInputValueByName("_xfToken");
				var attachmentHash         = formNode.GetInputValueByName("attachment_hash");
				var attachmentHashCombined = formNode.GetInputValueByName("attachment_hash_combined");
				var lastDate               = formNode.GetInputValueByName("last_date");
				var lastKnownDate          = formNode.GetInputValueByName("last_known_date");
				var loadExtra              = formNode.GetInputValueByName("load_extra");

				var kvlist = new List<KeyValuePair<string, string>> {
					new KeyValuePair<string, string>("message_html",             message               ),
					new KeyValuePair<string, string>("_xfToken",                 xfToken               ),
					new KeyValuePair<string, string>("attachment_hash",          attachmentHash        ),
					new KeyValuePair<string, string>("attachment_hash_combined", attachmentHashCombined),
					new KeyValuePair<string, string>("last_date",                lastDate              ),
					new KeyValuePair<string, string>("last_known_date",          lastKnownDate         ),
					new KeyValuePair<string, string>("load_extra",               loadExtra             ),
				};
				var hrm = await Forum.PostData(UrlPath + "add-reply", kvlist, false);
				await DownloadMessagesAsync();

				var match = Regex.Match(hrm.RequestMessage.RequestUri.Fragment, "-(\\d+)");

				if(!hrm.IsSuccessStatusCode && match.Success) {
					var msgId = long.Parse(match.Groups[1].Value);
					return Messages.Any(cm => cm.Id == msgId);
				} else {
					return hrm.IsSuccessStatusCode;
				}
			} else {
				return false;
			}
		}

		public override string ToString()
			=> $"Id: {Id} | Title: \"{Title}\" | Author: \"{Author}\" | Answers: {AnswerCount} | Members: ({Members}) | Messages: ({string.Join(", ", Messages.Select(c => $"({c})"))})";
	}
}
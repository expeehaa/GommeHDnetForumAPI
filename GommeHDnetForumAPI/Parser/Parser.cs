using System;
using System.Net.Http;
using System.Threading.Tasks;
using GommeHDnetForumAPI.Exceptions;
using GommeHDnetForumAPI.Models;
using HtmlAgilityPack;

namespace GommeHDnetForumAPI.Parser {
	internal abstract class Parser<T> {
		protected Forum               Forum        { get; }
		protected string              Html         { get; }
		protected BasicUrl            Url          { get; }
		protected HttpResponseMessage HttpResponse { get; }
		protected ParserContent       Content      { get; } = ParserContent.None;

		protected Parser(Forum forum) {
			Forum = forum;
		}

		protected Parser(Forum forum, string html) : this(forum) {
			Html    = html;
			Content = ParserContent.Html;
		}

		protected Parser(Forum forum, BasicUrl url) : this(forum) {
			Url     = url;
			Content = ParserContent.Url;
		}

		protected Parser(Forum forum, HttpResponseMessage hrm) : this(forum) {
			HttpResponse = hrm;
			Content      = ParserContent.HttpResponseMessage;
		}

		public abstract Task<T> ParseAsync();

		protected async Task<HtmlDocument> GetDoc(bool loginRequired = true, Action<HttpResponseMessage> onResponse = null, string url = null) {
			if (!Forum.LoggedIn && loginRequired) throw new LoginRequiredException("Login required!");
			var hrm = await Forum.GetData(url ?? Url.Url).ConfigureAwait(false);
			onResponse?.Invoke(hrm);
			var doc = new HtmlDocument();
			doc.LoadHtml(await hrm.Content.ReadAsStringAsync().ConfigureAwait(false));
			return doc;
		}
	}
}
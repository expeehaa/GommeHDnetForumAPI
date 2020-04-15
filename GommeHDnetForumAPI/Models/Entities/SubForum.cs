using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GommeHDnetForumAPI.Models.Entities.Interfaces;
using GommeHDnetForumAPI.Parser;
using GommeHDnetForumAPI.Parser.LiNodeParser;
using HtmlAgilityPack;

namespace GommeHDnetForumAPI.Models.Entities {
	public class SubForum : IndexedEntity, ISubForum {
		public string                    Title         { get; private set; }
		public string                    Description   { get; private set; }
		public IForum                    Parent        { get; }
		public long?                     PostCount     { get; private set; }
		public IEnumerable<ISubForum>    SubForums     { get; internal set; }
		public IEnumerable<ForumThread>  Threads       { get; internal set; }
		public IEnumerable<ThreadPrefix> Prefixes      { get; internal set; }
		public long?                     ThreadCount   => Threads?.Count();
		public long?                     SubForumCount => SubForums?.Count();

		public string UrlPath => $"{ForumPaths.ForumsPath}{Id}/";
		public string RssFeed => $"{UrlPath}index.rss";

		public IEnumerable<SubForum> RealSubForums => SubForums?.Select(sf => sf as SubForum).Where(sf => sf != null).ToList();

		public IEnumerable<SubForum> AllRealSubForums {
			get {
				var list = new List<SubForum>(RealSubForums ?? new List<SubForum>());
				list.AddRange(SubForums?.Where(sf => sf is SubForum).SelectMany(sf => ((SubForum) sf).AllRealSubForums) ?? new List<SubForum>());
				return list;
			}
		}

		public IEnumerable<SubLink> SubLinks => SubForums?.Select(sl => sl as SubLink).Where(sl => sl != null).ToList();

		internal SubForum(Forum forum, long id, IForum parent, string title, string description, long? postCount) : base(forum, id) {
			Title       = title;
			Description = description;
			PostCount   = postCount;
			Parent      = parent;
		}

		public async Task DownloadDataAsync()
			=> await DownloadDataAsync(1).ConfigureAwait(false);

		public async Task DownloadDataAsync(int startPage, int pageCount = 0) {
			startPage = Math.Max(1, startPage);
			
			var doc = await Forum.GetHtmlDocument($"{UrlPath}?page={startPage}");
			var sf  = new SubForumParser(this).Parse(doc.DocumentNode);
			Title       = sf.Title;
			Description = sf.Description;
			PostCount   = sf.PostCount;
			SubForums   = sf.SubForums;
			Prefixes    = sf.Prefixes;

			var lastPageNumber = doc.DocumentNode.SelectSingleNode("//div[@class='PageNav']")?.GetAttributeValue("data-last", 0) ?? 1;
			var pageMax        = pageCount <= 0 ? lastPageNumber : Math.Min(startPage+pageCount-1, lastPageNumber);
			var docs           = new List<HtmlDocument>{ doc };

			for(var i = startPage; i <= pageMax; i++) {
				docs.Add(await Forum.GetHtmlDocument($"{UrlPath}?page={i}"));
			}

			var parser = new ThreadsLiNodeParser(Forum, this);
			Threads = sf.Threads.Concat(docs.SelectMany(doc => parser.Parse(doc.DocumentNode)));
		}

		public override string ToString()
			=> $"({Id}: {Title} | {ThreadCount?.ToString() ?? "null"} | {PostCount?.ToString() ?? "null"} | {SubForumCount?.ToString() ?? "null"})";
	}
}
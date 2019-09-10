using System.Collections.Generic;
using System.Linq;
using GommeHDnetForumAPI.DataModels.Entities.Interfaces;

namespace GommeHDnetForumAPI.DataModels.Entities {
	public class MasterForumCategoryInfo : IndexedEntity, IForum {
		public string                 Title       { get; }
		public string                 Description { get; }
		public string                 Href        { get; }
		public string                 UrlPath     => $"{ForumPaths.ForumPath}{Href}";
		public IEnumerable<ISubForum> SubForums   { get; internal set; }

		public IEnumerable<SubForum> RealSubForums => SubForums?.Select(sf => sf as SubForum).Where(sf => sf != null).ToList();

		public IEnumerable<SubForum> AllRealSubForums {
			get {
				var list = new List<SubForum>(RealSubForums ?? new List<SubForum>());
				list.AddRange(SubForums?.Where(sf => sf is SubForum).SelectMany(sf => ((SubForum) sf).AllRealSubForums).ToList() ?? new List<SubForum>());
				return list;
			}
		}

		public IEnumerable<SubLink> SubLinks => SubForums?.Select(sl => sl as SubLink).Where(sl => sl != null).ToList();

		internal MasterForumCategoryInfo(Forum forum, long id, string title, string description, string href, IEnumerable<ISubForum> subForums = null) : base(forum, id) {
			Title       = title;
			Description = description;
			Href        = href;
			SubForums   = subForums ?? new List<ISubForum>();
		}

		public override string ToString()
			=> $"{Title}: {string.Join(", ", SubForums)}";
	}
}
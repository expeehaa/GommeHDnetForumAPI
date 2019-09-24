namespace GommeHDnetForumAPI.Models {
	public class BasicUrl {
		public string Url { get; set; }

		public BasicUrl(string url) {
			Url = url;
		}

		public override string ToString()
			=> Url;
	}
}
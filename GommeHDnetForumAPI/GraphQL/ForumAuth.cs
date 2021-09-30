namespace GommeHDnetForumAPI.GraphQL {
	public class ForumAuth {
		public bool Success { get; set; }
		public string Error { get; set; }
		// The error field seems to contain the token for the forum. This is probably a bug and may lead to errors here, if ever fixed in the API.
		public string ForumToken => Error;
	}
}

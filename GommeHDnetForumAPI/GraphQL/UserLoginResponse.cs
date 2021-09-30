namespace GommeHDnetForumAPI.GraphQL {
	public class UserLoginResponse {
		public bool Success { get; set; }
		public bool RequireTwoFa { get; set; }
		public string Token { get; set; }
		public string Error { get; set; }
	}
}

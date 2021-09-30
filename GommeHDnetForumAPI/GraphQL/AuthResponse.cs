namespace GommeHDnetForumAPI.GraphQL {
	public class AuthResponse {
		public bool Success { get; set; }
		public bool ForceTwoFa { get; set; }
		public User User { get; set; }
	}
}

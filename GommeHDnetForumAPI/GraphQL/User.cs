namespace GommeHDnetForumAPI.GraphQL {
	public class User {
		public string Id { get; set; }
		public string Email { get; set; }
		public bool HasTwoFa { get; set; }
		public string Locale { get; set; }
		public bool EmailVerified { get; set; }
		public MinecraftAccount Minecraft { get; set; }
		public DiscordAccount Discord { get; set; }
	}
}

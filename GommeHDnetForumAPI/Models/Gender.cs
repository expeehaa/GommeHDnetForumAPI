namespace GommeHDnetForumAPI.Models {
	public enum Gender {
		Unknown,
		Male,
		Female
	}

	public class GenderParser {
		public static Gender Parse(string gender) {
			switch (gender?.ToLower().Trim()) {
				case "männlich":
					return Gender.Male;
				case "weiblich":
					return Gender.Female;
				default:
					return Gender.Unknown;
			}
		}
	}
}
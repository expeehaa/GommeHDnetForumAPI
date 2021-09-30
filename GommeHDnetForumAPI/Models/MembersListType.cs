using System;

namespace GommeHDnetForumAPI.Models {
	public enum MembersListType {
		[StringValue("most_messages")]
		Posts,
		[StringValue("highest_reaction_score")]
		Likes,
		[StringValue("most_solutions")]
		Solutions,
		[StringValue("most_points")]
		Points,
		[StringValue("todays_birthdays")]
		Birthdays,
		[StringValue("staff_members")]
		Staff,
	}

	public static class MembersListTypeExtension {
		public static string GetStringValue(this MembersListType type)
			=> (type.GetType().GetField(type.ToString()).GetCustomAttributes(typeof(StringValueAttribute), false) as StringValueAttribute[])[0].Value;
	}

	[AttributeUsage(AttributeTargets.Field, Inherited = false, AllowMultiple = false)]
	internal sealed class StringValueAttribute : Attribute {
		public string Value { get; set; }

		public StringValueAttribute(string value) {
			Value = value;
		}
	}
}
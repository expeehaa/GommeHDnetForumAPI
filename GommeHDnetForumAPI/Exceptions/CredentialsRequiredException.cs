using System;

namespace GommeHDnetForumAPI.Exceptions {
	public class CredentialsRequiredException : Exception {
		public CredentialsRequiredException() { }

		public CredentialsRequiredException(string message) : base(message) { }
	}
}
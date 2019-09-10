using System;
using System.Runtime.Serialization;

namespace GommeHDnetForumAPI.Exceptions {
	public class LoginRequiredException : Exception {
		public LoginRequiredException() { }

		public LoginRequiredException(string message) : base(message) { }

		public LoginRequiredException(string message, Exception innerException) : base(message, innerException) { }

		protected LoginRequiredException(SerializationInfo info, StreamingContext context) : base(info, context) { }
	}
}
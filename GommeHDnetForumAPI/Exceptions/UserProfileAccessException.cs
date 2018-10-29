using System;
using System.Runtime.Serialization;

namespace GommeHDnetForumAPI.Exceptions
{
    public class UserProfileAccessException : Exception
    {
        public UserProfileAccessException()
        {
        }

        public UserProfileAccessException(string message) : base(message)
        {
        }

        public UserProfileAccessException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected UserProfileAccessException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}
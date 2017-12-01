using System;
using System.Runtime.Serialization;

namespace GommeHDnetForumAPI.DataModels.Exceptions
{
    public class CredentialsRequiredException : Exception
    {
        public CredentialsRequiredException()
        {
        }

        public CredentialsRequiredException(string message) : base(message)
        {
        }

        public CredentialsRequiredException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected CredentialsRequiredException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}
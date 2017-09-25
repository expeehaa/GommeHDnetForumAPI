using System;

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
    }
}
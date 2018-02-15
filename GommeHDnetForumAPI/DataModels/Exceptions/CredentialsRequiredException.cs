using System;

namespace GommeHDnetForumAPI.DataModels.Exceptions
{
    /// <inheritdoc />
    /// <summary>
    /// Thrown when GommeHDnetForumAPI.Forum#Login is executed and no credentials are available
    /// </summary>
    public class CredentialsRequiredException : Exception
    {
        public CredentialsRequiredException()
        {
        }

        public CredentialsRequiredException(string message) : base(message)
        {
        }
    }
}
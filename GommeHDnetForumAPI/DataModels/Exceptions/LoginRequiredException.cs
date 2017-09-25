using System;

namespace GommeHDnetForumAPI.DataModels.Exceptions
{
    public class LoginRequiredException : Exception
    {
        public LoginRequiredException()
        {
        }

        public LoginRequiredException(string message) : base(message)
        {
        }

        public LoginRequiredException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}
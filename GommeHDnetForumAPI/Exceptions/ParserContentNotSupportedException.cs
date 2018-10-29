using System;
using GommeHDnetForumAPI.Parser;

namespace GommeHDnetForumAPI.Exceptions
{
    public class ParserContentNotSupportedException : Exception
    {
        public ParserContent ParserContentNotSupported { get; }

        public ParserContentNotSupportedException()
        {
        }

        public ParserContentNotSupportedException(string message) : base(message)
        {
        }

        public ParserContentNotSupportedException(string message, ParserContent parserContent) : base(message) {
            ParserContentNotSupported = parserContent;
        }
    }
}
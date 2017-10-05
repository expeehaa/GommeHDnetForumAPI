using System;
using System.Threading.Tasks;
using GommeHDnetForumAPI.DataModels;

namespace GommeHDnetForumAPI.Parser
{
    internal abstract class Parser<T>
    {
        protected Forum Forum;
        protected string Html;
        protected ForumUrlPathString Url;

        protected Parser(Forum forum) {
            Forum = forum;
            Url = null;
            Html = null;
        }

        protected Parser(Forum forum, ForumUrlPathString url)
        {
            Forum = forum;
            Url = url;
            Html = null;
        }

        protected Parser(Forum forum, string html) {
            Forum = forum;
            Url = null;
            if(string.IsNullOrWhiteSpace(html)) throw new ArgumentNullException(nameof(html));
            Html = html;
        }

        public abstract Task<T> ParseAsync();
    }
}
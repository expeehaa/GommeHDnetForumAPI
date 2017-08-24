using System.Threading.Tasks;

namespace GommeHDnetForumAPI.Parser
{
    internal abstract class Parser<T>
    {
        protected Forum Forum;

        protected Parser(Forum forum)
        {
            Forum = forum;
        }

        public abstract Task<T> ParseAsync();
    }
}
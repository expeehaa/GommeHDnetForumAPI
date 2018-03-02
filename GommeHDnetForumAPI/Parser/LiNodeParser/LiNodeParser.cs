using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HtmlAgilityPack;

namespace GommeHDnetForumAPI.Parser.LiNodeParser
{
    internal abstract class LiNodeParser<T, O> : Parser<IEnumerable<T>>
    {
        protected readonly IEnumerable<HtmlNode> LiNodes;
        protected readonly O Parent;

        protected LiNodeParser(Forum forum, IEnumerable<HtmlNode> liNodes, O parent) : base(forum) {
            LiNodes = liNodes;
            Parent = parent;
        }

        public override Task<IEnumerable<T>> ParseAsync() 
            => Task.FromResult(LiNodes.Select(ParseElement).Where(t => t != null));

        protected abstract T ParseElement(HtmlNode node);
    }
}
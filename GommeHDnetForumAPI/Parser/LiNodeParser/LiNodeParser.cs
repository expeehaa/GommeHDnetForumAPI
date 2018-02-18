using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GommeHDnetForumAPI.DataModels.Entities;
using HtmlAgilityPack;

namespace GommeHDnetForumAPI.Parser.LiNodeParser
{
    internal abstract class LiNodeParser<T> : Parser<IEnumerable<T>>
    {
        protected readonly IEnumerable<HtmlNode> LiNodes;
        protected readonly IForum Parent;

        protected LiNodeParser(Forum forum, IEnumerable<HtmlNode> liNodes, IForum parent) : base(forum) {
            LiNodes = liNodes;
            Parent = parent;
        }

        public override Task<IEnumerable<T>> ParseAsync() 
            => Task.FromResult(LiNodes.Select(ParseElement).Where(t => t != null));

        protected abstract T ParseElement(HtmlNode node);
    }
}
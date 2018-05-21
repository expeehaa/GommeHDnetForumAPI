using System.Linq;
using System.Threading.Tasks;
using GommeHDnetForumAPI.DataModels;
using GommeHDnetForumAPI.DataModels.Entities;
using HtmlAgilityPack;

namespace GommeHDnetForumAPI.Parser
{
    internal class ForumThreadParser : Parser<ForumThread>
    {
        private readonly long Id;

        public ForumThreadParser(Forum forum, long id) : base(forum, new BasicUrl(ForumPaths.ForumThreadsPath + id)) {
            Id = id;
        }

        public override async Task<ForumThread> ParseAsync() {
            var doc = await GetDoc().ConfigureAwait(false);
            var titleBarNode = doc.DocumentNode.SelectSingleNode("//div[@class='titleBar']");
            if(titleBarNode == null) throw new NodeNotFoundException();
            var h1 = titleBarNode.SelectSingleNode("./h1");
            var prefixnode = h1.SelectSingleNode("./span[@class='prefix']");
            var prefixName = prefixnode.InnerText;
            var title = h1.LastChild.InnerText;


            long parentforumId = 0;
            var parent = Forum.MasterForum.Categories.SelectMany(c => c.AllRealSubForums).FirstOrDefault(sf => sf.Id == parentforumId);
            //var thread = new ForumThread(Forum, Id, title, new UserInfo(Forum, ), parent);
            //return thread;
            return null;
        }
    }
}
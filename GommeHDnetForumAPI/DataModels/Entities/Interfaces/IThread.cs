using System.Collections.Generic;
using System.Threading.Tasks;

namespace GommeHDnetForumAPI.DataModels.Entities.Interfaces
{
    public interface IThread<out T> : IThread where T : IPost
    {
        IEnumerable<T> Messages { get; }
    }

    public interface IThread : IUrlPath
    {
        string Title { get; }
        UserInfo Author { get; }

        Task DownloadMessagesAsync();
    }
}
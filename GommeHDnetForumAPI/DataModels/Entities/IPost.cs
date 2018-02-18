namespace GommeHDnetForumAPI.DataModels.Entities
{
    public interface IPost<out T> : IPost where T : IThread
    {
        T Parent { get; }
    }

    public interface IPost : IUrlPath
    {
        UserInfo Author { get; }
        string Content { get; }
    }
}
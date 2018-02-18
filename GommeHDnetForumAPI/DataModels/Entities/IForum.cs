namespace GommeHDnetForumAPI.DataModels.Entities
{
    public interface IForum : IUrlPath
    {
        string Title { get; }
        string Description { get; }
    }
}
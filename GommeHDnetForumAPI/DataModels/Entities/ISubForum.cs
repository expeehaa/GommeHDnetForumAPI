namespace GommeHDnetForumAPI.DataModels.Entities
{
    public interface ISubForum : IForum
    {
        IForum Parent { get; }
    }
}
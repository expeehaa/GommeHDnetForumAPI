namespace GommeHDnetForumAPI.DataModels.Entities
{
    public interface ISubForum
    {
        string Title { get; }
        string Description { get; }
        MasterForumCategoryInfo ParentCategory { get; }
        string UrlPath { get; }
    }
}
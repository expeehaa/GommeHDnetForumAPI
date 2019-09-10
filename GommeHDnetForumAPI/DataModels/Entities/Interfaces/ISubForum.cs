namespace GommeHDnetForumAPI.DataModels.Entities.Interfaces {
	public interface ISubForum : IForum {
		IForum Parent { get; }
	}
}
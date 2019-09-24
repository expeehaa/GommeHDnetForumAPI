namespace GommeHDnetForumAPI.Models.Entities.Interfaces {
	public interface ISubForum : IForum {
		IForum Parent { get; }
	}
}
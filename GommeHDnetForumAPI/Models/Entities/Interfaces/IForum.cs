namespace GommeHDnetForumAPI.Models.Entities.Interfaces {
	public interface IForum : IUrlPath {
		string Title       { get; }
		string Description { get; }
	}
}
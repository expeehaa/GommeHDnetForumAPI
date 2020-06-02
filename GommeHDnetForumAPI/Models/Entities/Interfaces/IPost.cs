namespace GommeHDnetForumAPI.Models.Entities.Interfaces {
	public interface IPost<out T> : IPost where T : IThread {
		T Parent { get; }
	}

	public interface IPost : IUrlPath {
		IUserInfo Author  { get; }
		string    Content { get; }
	}
}
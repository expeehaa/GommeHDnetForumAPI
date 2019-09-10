namespace GommeHDnetForumAPI.DataModels.Entities.Interfaces {
	public interface IPost<out T> : IPost where T : IThread {
		T Parent { get; }
	}

	public interface IPost : IUrlPath {
		UserInfo Author  { get; }
		string   Content { get; }
	}
}
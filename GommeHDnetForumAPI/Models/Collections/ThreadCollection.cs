using System.Collections.Generic;
using System.Threading.Tasks;
using GommeHDnetForumAPI.Models.Entities.Interfaces;

namespace GommeHDnetForumAPI.Models.Collections {
	public class ThreadCollection<T> : List<T> where T : IThread {
		public ThreadCollection() { }

		public ThreadCollection(IEnumerable<T> collection) : base(collection) { }

		public ThreadCollection(int capacity) : base(capacity) { }

		public async Task DownloadDataAsync() {
			foreach (var thread in this)
				await thread.DownloadMessagesAsync().ConfigureAwait(false);
		}
	}
}
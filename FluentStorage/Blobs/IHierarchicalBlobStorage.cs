using System.Threading;
using System.Threading.Tasks;

namespace FluentStorage.Blobs {
	/// <summary>
	/// Blob Storage that supports hierarchy
	/// </summary>
	public interface IHierarchicalBlobStorage {
		/// <summary>
		/// Creates a new folder
		/// </summary>
		/// <param name="folderPath">Path to the new folder.</param>
		/// <param name="cancellationToken"></param>
		/// <returns></returns>
		Task CreateFolderAsync(string folderPath, CancellationToken cancellationToken = default);
	}
}

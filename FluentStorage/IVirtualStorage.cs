using FluentStorage.Blobs;

namespace FluentStorage {
	/// <summary>
	/// Virtual storage
	/// </summary>
	public interface IVirtualStorage : IBlobStorage {
		/// <summary>
		/// Mounts a storage to virtual path
		/// </summary>
		void Mount(string path, IBlobStorage storage);
	}
}

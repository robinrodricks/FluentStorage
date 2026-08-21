using System.Threading.Tasks;
using FluentStorage.Storage;

namespace FluentStorage.AWS.Storage;

/// <summary>
/// Provides access to native operations
/// </summary>
public interface IS3Storage : IStore {

	/// <summary>
	/// Return bucket name.
	/// </summary>
	string BucketName { get; }

	/// <summary>
	/// Set acl for object.
	/// </summary>
	/// <param name="fullPath"></param>
	/// <param name="acl"></param>
	Task SetAcl(string fullPath, string acl);
}
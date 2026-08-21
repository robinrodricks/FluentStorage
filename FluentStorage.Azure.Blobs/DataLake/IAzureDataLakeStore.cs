using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FluentStorage.Azure.Blobs.DataLake.Model;

namespace FluentStorage.Azure.Blobs;

/// <summary>
/// Data Lake Gen 2 storage operations
/// </summary>
public interface IAzureDataLakeStore : IAzureBlobStore {

	/// <summary>
	/// Lists filesystems in the data lake.
	/// </summary>
	/// <returns></returns>
	Task<List<Filesystem>> ListFilesystems(CancellationToken cancellationToken = default);

	/// <summary>
	/// Creates a filesystem in the data lake.
	/// </summary>
	/// <param name="filesystemName"></param>
	/// <param name="cancellationToken"></param>
	/// <returns></returns>
	Task CreateFilesystem(string filesystemName, CancellationToken cancellationToken = default);

	/// <summary>
	/// Deletes a filesystem from the data lake.
	/// </summary>
	/// <param name="filesystem"></param>
	/// <param name="cancellationToken"></param>
	/// <returns></returns>
	Task DeleteFilesystem(string filesystem, CancellationToken cancellationToken = default);


	/// <summary>
	/// Sets permissions on an object
	/// </summary>
	/// <param name="fullPath"></param>
	/// <param name="accessControl">Access control rules. A good idea whould be to retreive them using <see cref="GetAccessControl(string, bool, CancellationToken)"/>, modify, and send back via this method.</param>
	/// <param name="cancellationToken"></param>
	/// <returns></returns>
	Task SetAccessControl(string fullPath, AccessControl accessControl, CancellationToken cancellationToken = default);

	/// <summary>
	/// Gets permissions from an object
	/// </summary>
	/// <param name="fullPath"></param>
	/// <param name="getUpn">When true, the call will return UPNs instead of object IDs when querying for permissions</param>
	/// <param name="cancellationToken"></param>
	/// <returns></returns>
	Task<AccessControl> GetAccessControl(string fullPath, bool getUpn = false, CancellationToken cancellationToken = default);

}
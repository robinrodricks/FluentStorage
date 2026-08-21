using FluentStorage.AWS.Storage;
using FluentStorage.Storage;

namespace FluentStorage;

/// <summary>
/// Vultr Object Storage factory to create instances of `IStore` using this provider.
/// </summary>
public static class VultrStorage {

	/// <summary>
	/// Creates a Vultr Object Storage provider (S3-compatible).
	/// </summary>
	/// <param name="accessKeyId">Access key ID</param>
	/// <param name="secretAccessKey">Secret access key</param>
	/// <param name="bucketName">Bucket name</param>
	/// <param name="hostName">Storage endpoint hostname (e.g. `sgp1.vultrobjects.com`)</param>
	/// <param name="sessionToken">Optional. Only required when using session credentials.</param>
	/// <returns>A reference to the created storage</returns>
	public static IStore FromCredentials(
		string accessKeyId,
		string secretAccessKey,
		string bucketName,
		string hostName,
		string sessionToken = null) {
		return S3Store.FromVultr(accessKeyId, secretAccessKey, bucketName, hostName, sessionToken);
	}
}
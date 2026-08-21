using FluentStorage.AWS.Storage;
using FluentStorage.Storage;

namespace FluentStorage.AWS.Factory;

/// <summary>
/// Hetzner Object Storage factory to create instances of `IStore` using this provider.
/// </summary>
public static class HetznerStorage {

	/// <summary>
	/// Creates a Hetzner Object Storage provider (S3-compatible).
	/// </summary>
	/// <param name="accessKeyId">Access key ID</param>
	/// <param name="secretAccessKey">Secret access key</param>
	/// <param name="bucketName">Bucket name</param>
	/// <param name="region">Storage region (e.g. `fsn1`, `nbg1`, `hel1`)</param>
	/// <param name="sessionToken">Optional. Only required when using session credentials.</param>
	/// <returns>A reference to the created storage</returns>
	public static IStore FromCredentials(
		string accessKeyId,
		string secretAccessKey,
		string bucketName,
		string region,
		string sessionToken = null) {
		return S3Store.FromHetzner(accessKeyId, secretAccessKey, bucketName, region, sessionToken);
	}
}
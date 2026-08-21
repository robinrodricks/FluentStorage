using FluentStorage.AWS.Storage;
using FluentStorage.Storage;

namespace FluentStorage;

/// <summary>
/// DigitalOcean Spaces factory to create instances of `IStore` using this provider.
/// </summary>
public static class DigitalOceanSpacesStorage {

	/// <summary>
	/// Creates an DigitalOcean Spaces storage provider (S3-compatible).
	/// </summary>
	/// <param name="accessKeyId">Access key ID</param>
	/// <param name="secretAccessKey">Secret access key</param>
	/// <param name="bucketName">Bucket name</param>
	/// <param name="digitalOceanRegion">DigitalOcean Region endpoint (like "nyc3")</param>
	/// <param name="sessionToken">Optional. Only required when using session credentials.</param>
	/// <returns>A reference to the created storage</returns>
	public static IStore FromCredentials(
		string accessKeyId,
		string secretAccessKey,
		string bucketName,
		string digitalOceanRegion,
		string sessionToken = null) {
		return S3Store.FromDigitalOcean(accessKeyId, secretAccessKey, bucketName, digitalOceanRegion, sessionToken);
	}
}
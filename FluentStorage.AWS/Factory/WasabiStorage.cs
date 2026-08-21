using FluentStorage.AWS.Storage;
using FluentStorage.Storage;

namespace FluentStorage;

/// <summary>
/// Wasabi storage factory to create instances of `IStore` using this provider.
/// </summary>
public static class WasabiStorage {

	/// <summary>
	/// Creates an Wasabi storage provider (S3-compatible).
	/// </summary>
	/// <param name="accessKeyId">Access key ID</param>
	/// <param name="secretAccessKey">Secret access key</param>
	/// <param name="bucketName">Bucket name</param>
	/// <param name="wasabiServiceUrl">Wasabi Service URL endpoint (like "https://s3.wasabisys.com")</param>
	/// <param name="sessionToken">Optional. Only required when using session credentials.</param>
	/// <returns>A reference to the created storage</returns>
	public static IStore FromCredentials(
		string accessKeyId,
		string secretAccessKey,
		string bucketName,
		string wasabiServiceUrl,
		string sessionToken = null) {
		return S3Store.FromWasabi(accessKeyId, secretAccessKey, bucketName, wasabiServiceUrl, sessionToken);
	}
}
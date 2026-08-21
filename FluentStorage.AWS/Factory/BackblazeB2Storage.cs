using FluentStorage.AWS.Storage;
using FluentStorage.Storage;

namespace FluentStorage;

/// <summary>
/// Backblaze B2 factory to create instances of `IStore` using this provider.
/// </summary>
public static class BackblazeB2Storage {

	/// <summary>
	/// Creates a Backblaze B2 storage provider (S3-compatible).
	/// </summary>
	/// <param name="accessKeyId">Application Key ID</param>
	/// <param name="secretAccessKey">Application Key</param>
	/// <param name="bucketName">Bucket name</param>
	/// <param name="region">Bucket region (e.g. `us-west-004`)</param>
	/// <param name="sessionToken">Optional. Only required when using session credentials.</param>
	/// <returns>A reference to the created storage</returns>
	public static IStore FromCredentials(
		string accessKeyId,
		string secretAccessKey,
		string bucketName,
		string region,
		string sessionToken = null) {
		return S3Store.FromBackblazeB2(accessKeyId, secretAccessKey, bucketName, region, sessionToken);
	}
}
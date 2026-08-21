using FluentStorage.Minio.Storage;
using FluentStorage.Storage;
using Minio;

namespace FluentStorage;

/// <summary>
/// Factory methods for creating MinIO stores.
/// </summary>
public static class MinioStorage {

	/// <summary>
	/// Creates a MinIO storage provider using access key / secret key credentials.
	/// </summary>
	public static IStore FromCredentials(string endpoint, string accessKey, string secretKey,
		string bucketName, bool useSsl = true, string region = null) {
		return new MinioStore(endpoint, accessKey, secretKey, bucketName, useSsl, region);
	}

	/// <summary>
	/// Creates a MinIO storage provider using temporary STS credentials.
	/// </summary>
	public static IStore FromSts(string endpoint, string accessKey, string secretKey,
		string sessionToken, string bucketName, bool useSsl = true, string region = null) {
		return new MinioStore(endpoint, accessKey, secretKey, sessionToken, bucketName, useSsl, region);
	}

	/// <summary>
	/// Creates a MinIO storage provider using auto-resolved EC2/ECS/EKS IAM credentials.
	/// </summary>
	public static IStore FromIamRole(string endpoint, string bucketName,
		bool useSsl = true, string region = null, string iamEndpoint = null) {
		return new MinioStore(endpoint, bucketName, useSsl, region, iamEndpoint);
	}

	/// <summary>
	/// Creates a MinIO storage provider using STS AssumeRole credentials.
	/// </summary>
	public static IStore FromAssumeRole(string endpoint, string accessKey, string secretKey,
		string roleArn, string bucketName, string roleSessionName = null,
		string externalId = null, string policy = null,
		uint durationInSeconds = 3600, bool useSsl = true,
		string region = null, string stsEndpoint = null) {
		return new MinioStore(endpoint,accessKey,secretKey,roleArn,bucketName,roleSessionName,
			externalId,policy,durationInSeconds,useSsl,region,stsEndpoint);
	}

	/// <summary>
	/// Wraps a preconfigured MinIO Client into a storage provider.
	/// </summary>
	public static IStore FromClient(IMinioClient existingClient, string bucketName) {
		return new MinioStore(existingClient, bucketName);
	}

}
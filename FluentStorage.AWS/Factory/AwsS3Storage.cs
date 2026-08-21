using Amazon.Runtime;
using Amazon.S3;
using Amazon.S3.Transfer;
using FluentStorage.AWS.Storage;
using FluentStorage.ConnectionStrings;
using FluentStorage.Storage;

namespace FluentStorage;

/// <summary>
/// Amazon Web Services S3 factory to create instances of `IStore` using this provider.
/// </summary>
public static class AwsS3Storage {

	/// <summary>
	/// Enable AWS S3 connection string support.
	/// </summary>
	public static void Use() {
		StorageFactory.Use(new AwsStorageModule());
	}


	/// <summary>
	/// Creates an Amazon S3 storage using assumed role permissions (useful when running the code wform within ECS tasks or lambda where you don't need to provide and manage accessKeys and secrets as the permissions are assumed via the IAM role the lambda or ecs tasks has assigned to it)
	/// </summary>
	/// <param name="bucketName">Bucket name</param>
	/// <param name="region">Required regional endpoint.</param>
	/// <returns>A reference to the created storage</returns>
	public static IStore FromRole(
		string bucketName,
		string region) {
		return new S3Store(bucketName, region);
	}

	/// <summary>
	/// Creates an Amazon S3 storage
	/// </summary>
	/// <param name="accessKeyId">Access key ID</param>
	/// <param name="secretAccessKey">Secret access key</param>
	/// /// <param name="sessionToken">Optional. Only required when using session credentials.</param>
	/// <param name="bucketName">Bucket name</param>
	/// <param name="region">Region endpoint</param>
	/// <param name="serviceUrl">S3-compatible service location</param>
	/// <returns>A reference to the created storage</returns>
	public static IStore FromCredentials(
		string accessKeyId,
		string secretAccessKey,
		string sessionToken,
		string bucketName,
		string region,
		string serviceUrl = null) {
		return new S3Store(accessKeyId, secretAccessKey, sessionToken, bucketName, region, serviceUrl);
	}

	/// <summary>
	/// Creates an Amazon S3 storage provider for a custom S3-compatible storage server
	/// </summary>
	/// <param name="accessKeyId">Access key ID</param>
	/// <param name="secretAccessKey">Secret access key</param>
	/// <param name="sessionToken">Optional. Only required when using session credentials.</param>
	/// <param name="bucketName">Bucket name</param>
	/// <param name="clientConfig">S3 client configuration</param>
	/// <param name="transferUtilityConfig">S3 transfer utility configuration</param>
	/// <returns>A reference to the created storage</returns>
	public static IStore FromThirdPartyCredentials(
		string accessKeyId,
		string secretAccessKey,
		string sessionToken,
		string bucketName,
		AmazonS3Config clientConfig,
		TransferUtilityConfig transferUtilityConfig = null) {
		return new S3Store(accessKeyId, secretAccessKey, sessionToken, bucketName, clientConfig, transferUtilityConfig);
	}

#if !NET16

	/// <summary>
	/// Creates an Amazon S3 storage provider using credentials from AWS CLI configuration file (~/.aws/credentials)
	/// </summary>
	/// <param name="awsCliProfileName"></param>
	/// <param name="bucketName">Bucket name</param>
	/// <param name="region"></param>
	/// <returns>A reference to the created storage</returns>
	public static IStore FromConfigFile(
		string awsCliProfileName,
		string bucketName,
		string region) {
		return S3Store.FromAwsCliProfile(awsCliProfileName, bucketName, region);
	}

	/// <summary>
	/// Creates an Amazon S3 storage provider using credentials retrieved from SSO.
	/// </summary>
	/// <param name="credentials"></param>
	/// <param name="bucketName">Bucket name</param>
	/// <param name="region"></param>
	/// <returns>A reference to the created storage</returns>
	public static IStore FromSSO(
		AWSCredentials credentials,
		string bucketName,
		string region) {
		return S3Store.FromAwsCredentials(credentials, bucketName, region);
	}
#endif



	/// <summary>
	/// Creates a connection string from AWS CLI profile name
	/// </summary>
	/// <param name="factory"></param>
	/// <param name="profileName"></param>
	/// <param name="bucketName"></param>
	/// <param name="region"></param>
	/// <returns></returns>
	public static ConnectionString CreateConnectionStringFromCliProfile(
		string profileName,
		string bucketName,
		string region) {
		if (profileName is null)
			throw new System.ArgumentNullException(nameof(profileName));
		if (bucketName is null)
			throw new System.ArgumentNullException(nameof(bucketName));
		if (region is null)
			throw new System.ArgumentNullException(nameof(region));
		var cs = new ConnectionString(ConnectionStringPrefix.AwsS3 + "://");
		cs[ConnectionStringParam.LocalProfileName] = profileName;
		cs[ConnectionStringParam.BucketName] = bucketName;
		cs[ConnectionStringParam.Region] = region;
		return cs;
	}

}
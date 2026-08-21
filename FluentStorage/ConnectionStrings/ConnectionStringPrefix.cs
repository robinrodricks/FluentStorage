namespace FluentStorage.ConnectionStrings;

/// <summary>
/// Provider-specific prefixes that are supported in FluentStorage connection strings
/// </summary>
public static class ConnectionStringPrefix {

	/// <summary>
	/// Returns true if the prefix is S3 or S3-compatible storage.
	/// </summary>
	public static bool IsS3Compatible(string prefix) {

		// [ADD STORAGE PROVIDER]

		return prefix == AwsS3 || prefix == MinIoS3 || prefix == CloudflareR2 ||
		       prefix == Wasabi || prefix == DigitalOceanSpaces ||
		       prefix == BackblazeB2 || prefix == Hetzner || prefix == Vultr;
	}

	/// <summary>
	/// AWS S3 storage
	/// </summary>
	public static string AwsS3 = "aws.s3";

	/// <summary>
	/// MinIO storage
	/// </summary>
	public static string MinIoS3 = "minio.s3";

	/// <summary>
	/// Wasabi storage
	/// </summary>
	public static string Wasabi = "wasabi";

	/// <summary>
	/// DigitalOcean Spaces storage
	/// </summary>
	public static string DigitalOceanSpaces = "do.spaces";

	/// <summary>
	/// Cloudflare R2 storage
	/// </summary>
	public static string CloudflareR2 = "cloudflare.r2";

	/// <summary>
	/// Backblaze B2 S3-compatible object storage.
	/// </summary>
	public static string BackblazeB2 = "backblaze.b2";

	/// <summary>
	/// Hetzner S3-compatible object storage.
	/// </summary>
	public static string Hetzner = "hetzner";

	/// <summary>
	/// Vultr S3-compatible object storage.
	/// </summary>
	public static string Vultr = "vultr";

	// [ADD STORAGE PROVIDER]

	/// <summary>
	/// Azure Data Lake Gen 1
	/// </summary>
	public static string AzureDataLakeGen1 = "azure.datalake.gen1";

	/// <summary>
	/// Azure Data Lake Gen 2
	/// </summary>
	public static string AzureDataLakeGen2 = "azure.datalake.gen2";

	/// <summary>
	/// Azure Data Lake latest generation, currently Gen 2
	/// </summary>
	public static string AzureDataLake = "azure.datalake";

	/// <summary>
	/// Azure Key Vault
	/// </summary>
	public static string AzureKeyVault = "azure.keyvault";

	/// <summary>
	/// Azure Blob Storage
	/// </summary>
	public const string AzureBlobStorage = "azure.blob";

	/// <summary>
	/// Azure File Storage
	/// </summary>
	public const string AzureFilesStorage = "azure.file";

	/// <summary>
	/// Microsoft Azure Table Storage
	/// </summary>
	public const string AzureTableStorage = "azure.tables";

	/// <summary>
	/// Azure Storage Queues
	/// </summary>
	public const string AzureQueueStorage = "azure.queue";

}
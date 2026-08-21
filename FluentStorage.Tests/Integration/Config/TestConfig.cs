using YamlDotNet.Serialization;

namespace FluentStorage.Tests.Integration.Config;

[YamlSerializable]
public class TestConfig {

	// ----------------------------------------------------------------------
	// Local Disk
	// ----------------------------------------------------------------------

	public string LocalDiskPath { get; set; }


	// ---------------------------------------------------------------------
	// Azure Blob/Files/DataLake
	// ---------------------------------------------------------------------


	public string AzureClientId { get; set; }

	public string AzureClientSecret { get; set; }

	public string AzureTenantId { get; set; }
	public string AzureStorageName { get; set; }

	public string AzureStorageKey { get; set; }

	public string AzureDataLakeStorageName { get; set; }

	public string AzureDataLakeStorageKey { get; set; }

	public string AzureDataLakeOperatorObjectId { get; set; }

	public string AzureServiceBusConnectionString { get; set; }

	public Uri AzureKeyVaultUri { get; set; }



	// ---------------------------------------------------------------------
	// AWS S3
	// ---------------------------------------------------------------------

	public string AwsAccessKey { get; set; }

	public string AwsSecretKey { get; set; }

	public string AwsBucket { get; set; }

	public string AwsBucketRegion { get; set; }



	// ---------------------------------------------------------------------
	// GCP Storage
	// ---------------------------------------------------------------------

	public string GcpBucket { get; set; }

	public string GcpJsonKey { get; set; }



	// ---------------------------------------------------------------------
	// FTP
	// ---------------------------------------------------------------------

	public string FtpHost { get; set; }

	public string FtpUsername { get; set; }

	public string FtpPassword { get; set; }



	// ---------------------------------------------------------------------
	// SFTP
	// ---------------------------------------------------------------------

	public string SftpHost { get; set; }
	public int SftpPort { get; set; }
	public string SftpUser { get; set; }
	public string SftpPassphrase { get; set; }
	public string SftpPrivateKeyPath { get; set; }



	// ---------------------------------------------------------------------
	// Backblaze B2
	// ---------------------------------------------------------------------

	public string B2AccessKey { get; set; }

	public string B2SecretKey { get; set; }

	public string B2Bucket { get; set; }

	public string B2Region { get; set; }


	// ---------------------------------------------------------------------
	// Cloudflare R2
	// ---------------------------------------------------------------------

	public string CloudflareAccessKey { get; set; }

	public string CloudflareSecretKey { get; set; }

	public string CloudflareBucket { get; set; }

	public string CloudflareAccountId { get; set; }


	// ---------------------------------------------------------------------
	// DigitalOcean Spaces
	// ---------------------------------------------------------------------

	public string DoAccessKey { get; set; }

	public string DoSecretKey { get; set; }

	public string DoBucket { get; set; }

	public string DoRegion { get; set; }


	// ---------------------------------------------------------------------
	// Hetzner Object Storage
	// ---------------------------------------------------------------------

	public string HetznerAccessKey { get; set; }

	public string HetznerSecretKey { get; set; }

	public string HetznerBucket { get; set; }

	public string HetznerRegion { get; set; }


	// ---------------------------------------------------------------------
	// MinIO (S3 SDK)
	// ---------------------------------------------------------------------

	public string MinioS3AccessKey { get; set; }

	public string MinioS3SecretKey { get; set; }

	public string MinioS3Bucket { get; set; }

	public string MinioS3AwsRegion { get; set; }

	public string MinioS3ServerUrl { get; set; }


	// ---------------------------------------------------------------------
	// Vultr Object Storage
	// ---------------------------------------------------------------------

	public string VultrAccessKey { get; set; }

	public string VultrSecretKey { get; set; }

	public string VultrBucket { get; set; }

	public string VultrHostName { get; set; }


	// ---------------------------------------------------------------------
	// Wasabi
	// ---------------------------------------------------------------------

	public string WasabiAccessKey { get; set; }

	public string WasabiSecretKey { get; set; }

	public string WasabiBucket { get; set; }

	public string WasabiServiceUrl { get; set; }


	// ---------------------------------------------------------------------
	// MongoDB GridFS
	// ---------------------------------------------------------------------

	public string MongoHost { get; set; }

	public int MongoPort { get; set; }

	public string MongoUsername { get; set; }

	public string MongoPassword { get; set; }

	public string MongoDatabase { get; set; }

	public string MongoBucket { get; set; }

	public string MongoAuthDatabase { get; set; }

	public bool MongoSsl { get; set; }


	// ---------------------------------------------------------------------
	// MinIO
	// ---------------------------------------------------------------------

	public string MinioEndpoint { get; set; }

	public string MinioAccessKey { get; set; }

	public string MinioSecretKey { get; set; }

	public string MinioBucket { get; set; }

	public bool MinioSsl { get; set; }

	public string MinioRegion { get; set; }


	// ---------------------------------------------------------------------
	// Alibaba OSS
	// ---------------------------------------------------------------------

	public string AlibabaEndpoint { get; set; }

	public string AlibabaBucket { get; set; }

	public string AlibabaAccessKey { get; set; }

	public string AlibabaSecretKey { get; set; }


	public TestConfig() {
	}
}
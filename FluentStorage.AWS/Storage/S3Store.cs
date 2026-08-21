using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Amazon.Runtime;
using Amazon.S3;
using Amazon.S3.Model;
using Amazon.S3.Transfer;
using Amazon.S3.Util;
using FluentStorage.AWS.Utils;
using FluentStorage.Enums;
using FluentStorage.Exceptions;
using FluentStorage.Model;
using FluentStorage.Storage;
using FluentStorage.Streaming;
using FluentStorage.Utils.Extensions;
using FluentStorage.Utils.Validation;
using MimeMapping;

namespace FluentStorage.AWS.Storage;

/// <summary>
/// Manages a single S3 or S3-compatible bucket using the Amazon S3 SDK.
/// </summary>
public class S3Store : StoreBase, IS3Storage {
	private const int ListChunkSize = 10;
	private readonly string _bucketName;
	private readonly AmazonS3Client _client;
	private readonly TransferUtility _fileTransferUtility;
	private bool _initialised = false;
	private bool _usePutObject = false;
	private bool _disablePayloadSigning = false;


	/// <summary>
	/// Return bucket name.
	/// </summary>
	public string BucketName => _bucketName;

#if !NET16
	public static S3Store FromAwsCliProfile(string profileName, string bucketName, string region) {
		return new S3Store(bucketName, region, AwsCliCredentials.GetCredentials(profileName));
	}
	public static S3Store FromAwsCredentials(AWSCredentials credentials, string bucketName, string region) {
		return new S3Store(bucketName, region, credentials);
	}
	/// <summary>
	/// Creates an S3-compatible blob storage backed by DigitalOcean Spaces.
	/// </summary>
	public static S3Store FromDigitalOcean(string accessKeyId, string secretAccessKey, string bucketName, string digitalOceanRegion, string sessionToken = null) {
		var serviceUrl = $"https://{digitalOceanRegion}.digitaloceanspaces.com";
		return new S3Store(accessKeyId, secretAccessKey, sessionToken, bucketName, null, serviceUrl);
	}
	/// <summary>
	/// Creates an S3-compatible blob storage backed by MinIO.
	/// </summary>
	public static S3Store FromMinIO(string accessKeyId, string secretAccessKey, string bucketName, string awsRegion, string minioServerUrl, string sessionToken = null) {
		var config = new AmazonS3Config {
			AuthenticationRegion = awsRegion,
			ServiceURL = minioServerUrl,
			ForcePathStyle = true,
		};
		return new S3Store(accessKeyId, secretAccessKey, sessionToken, bucketName, config);
	}
	/// <summary>
	/// Creates an S3-compatible blob storage backed by Wasabi.
	/// </summary>
	public static S3Store FromWasabi(string accessKeyId, string secretAccessKey, string bucketName, string wasabiServiceUrl, string sessionToken = null) {
		return new S3Store(accessKeyId, secretAccessKey, sessionToken, bucketName, null, wasabiServiceUrl);
	}
	/// <summary>
	/// Creates an S3-compatible blob storage backed by Cloudflare R2 Storage.
	/// </summary>
	public static S3Store FromCloudflareR2(string accessKeyId, string secretAccessKey, string bucketName, string cloudflareAccountId, string sessionToken = null) {

		var config = new AmazonS3Config {
			// ServiceURL is always https://<account-id>.r2.cloudflarestorage.com.
			ServiceURL = $"https://{cloudflareAccountId}.r2.cloudflarestorage.com",
			// AuthenticationRegion = "auto" is the recommended value for R2
			AuthenticationRegion = "auto",
			// Uses virtual-hosted style requests, which R2 supports and recommends
			ForcePathStyle = false,
			// Ensures HTTPS (the endpoint itself is HTTPS, but this makes it explicit).
			UseHttp = false,
		};

		var store = new S3Store(accessKeyId, secretAccessKey, sessionToken, bucketName, config);
		store._usePutObject = true;
		store._disablePayloadSigning = true;
		return store;
	}
	/// <summary>
	/// Creates an S3-compatible blob storage backed by Backblaze B2.
	/// </summary>
	public static S3Store FromBackblazeB2(string accessKeyId,string secretAccessKey,string bucketName,string region, string sessionToken = null) {

		var config = new AmazonS3Config {
			// Endpoint format is https://s3.<region>.backblazeb2.com
			ServiceURL = $"https://s3.{region}.backblazeb2.com",
			// Requests are signed with the bucket region.
			AuthenticationRegion = region,
			// Uses virtual-hosted style requests, which B2 supports
			ForcePathStyle = false,
			// Ensures HTTPS (the endpoint itself is HTTPS, but this makes it explicit).
			UseHttp = false,
		};

		var store = new S3Store(accessKeyId, secretAccessKey, sessionToken, bucketName, config);
		return store;
	}
	/// <summary>
	/// Creates an S3-compatible blob storage backed by Hetzner Object Storage.
	/// </summary>
	public static S3Store FromHetzner(string accessKeyId,string secretAccessKey,string bucketName,string region, string sessionToken = null) {

		var config = new AmazonS3Config {
			// Endpoint format is https://<region>.your-objectstorage.com
			ServiceURL = $"https://{region}.your-objectstorage.com",
			// Requests are signed with the bucket region.
			AuthenticationRegion = region,
			// Uses virtual-hosted style requests
			ForcePathStyle = false,
			// Ensures HTTPS (the endpoint itself is HTTPS, but this makes it explicit).
			UseHttp = false,
		};

		var store = new S3Store(accessKeyId, secretAccessKey, sessionToken, bucketName, config);
		return store;
	}
	/// <summary>
	/// Creates an S3-compatible blob storage backed by Vultr Object Storage.
	/// </summary>
	public static S3Store FromVultr(string accessKeyId,string secretAccessKey,string bucketName,string hostname, string sessionToken = null) {

		var config = new AmazonS3Config {
			// Endpoint is unique per cluster.
			ServiceURL = $"https://{hostname}",
			// Vultr accepts us-east-1 for request signing.
			AuthenticationRegion = "us-east-1",
			// Uses virtual-hosted style requests
			ForcePathStyle = false,
			// Ensures HTTPS (the endpoint itself is HTTPS, but this makes it explicit).
			UseHttp = false,
		};

		var store = new S3Store(accessKeyId, secretAccessKey, sessionToken, bucketName, config);
		return store;
	}
#endif

	// [ADD STORAGE PROVIDER]


	public S3Store(string bucketName, string region, AWSCredentials credentials) {
		_bucketName = bucketName;
		_client = new AmazonS3Client(credentials, CreateConfig(region, null));
		_fileTransferUtility = new TransferUtility(_client);
	}

	/// <summary>
	/// Creates a new instance of <see cref="S3Store"/> for a given region endpoint, and will assume the running AWS ECS Task role credentials or Lambda role credentials.
	/// </summary>
	public S3Store(string bucketName, string region) {
		_bucketName = bucketName ?? throw new ArgumentNullException(nameof(bucketName));
		_client = new AmazonS3Client(region.ToRegionEndpoint());
		_fileTransferUtility = new TransferUtility(_client);
	}

	/// <summary>
	/// Creates a new instance of <see cref="S3Store"/> for a given region endpoint.
	/// </summary>
	public S3Store(string accessKeyId, string secretAccessKey, string sessionToken, string bucketName, string region, string serviceUrl)
		: this(accessKeyId, secretAccessKey, sessionToken, bucketName, CreateConfig(region, serviceUrl)) {
	}

	private static AmazonS3Config CreateConfig(string region, string serviceUrl) {
		var config = new AmazonS3Config();
		if (region != null)
			config.RegionEndpoint = region.ToRegionEndpoint();
		if (serviceUrl != null)
			config.ServiceURL = serviceUrl;
		return config;
	}

	/// <summary>
	/// Creates a new instance of <see cref="S3Store"/> for a given S3 client configuration
	/// </summary>
	public S3Store(string accessKeyId, string secretAccessKey, string sessionToken,
		string bucketName, AmazonS3Config clientConfig)
		: this(accessKeyId, secretAccessKey, sessionToken, bucketName, clientConfig, new TransferUtilityConfig()) {
	}

	/// <summary>
	/// Creates a new instance of <see cref="S3Store"/> for a given S3 client configuration
	/// </summary>
	public S3Store(string accessKeyId, string secretAccessKey, string sessionToken,
		string bucketName, AmazonS3Config clientConfig, TransferUtilityConfig transferUtilityConfig) {
		if (accessKeyId == null)
			throw new ArgumentNullException(nameof(accessKeyId));
		if (secretAccessKey == null)
			throw new ArgumentNullException(nameof(secretAccessKey));
		_bucketName = bucketName ?? throw new ArgumentNullException(nameof(bucketName));

		AWSCredentials awsCreds = (sessionToken == null)
			? (AWSCredentials)new BasicAWSCredentials(accessKeyId, secretAccessKey)
			: new SessionAWSCredentials(accessKeyId, secretAccessKey, sessionToken);

		_client = new AmazonS3Client(awsCreds, clientConfig);

		_fileTransferUtility = new TransferUtility(_client, transferUtilityConfig ?? new TransferUtilityConfig());
	}

	/// <summary>
	/// Returns the AmazonS3Client instance for this store.
	/// </summary>
	public override async Task<object> GetClient() {
		return await Client();
	}

	/// <summary>
	/// Create a client and checks if the S3 bucket exists.
	/// </summary>
	private async Task<AmazonS3Client> Client() {

		if (!_initialised) {
			var bucketExists = await AmazonS3Util.DoesS3BucketExistV2Async(_client, _bucketName);
			if (!bucketExists) {
				throw new StorageException($"Bucket '{_bucketName}' does not exist!");
			}
			else {
				_initialised = true;
			}
		}

		return _client;
	}

	/// <summary>
	/// Lists all objects in this bucket.
	/// </summary>
	public override async Task<List<StoreObject>> ListObjects(StorageListOptions options = null, CancellationToken cancellationToken = default) {
		if (options == null)
			options = new StorageListOptions();

		ArgValidator.AssertPrefix(options.FilePrefix);

		AmazonS3Client client = await Client().ConfigureAwait(false);

		List<StoreObject> blobs;
		using (var browser = new S3DirectoryBrowser(client, _bucketName)) {
			blobs = await browser.ListAsync(options, cancellationToken).ConfigureAwait(false);
		}

		if (options.IncludeAttributes) {

			foreach (IEnumerable<StoreObject> page in blobs.Where(b => b != null && !b.IsFolder).Chunk(ListChunkSize)) {
				await AwsConverter.AppendMetadataAsync(client, _bucketName, page, cancellationToken).ConfigureAwait(false);
			}
		}

		return blobs;
	}

	/// <summary>
	/// Uploads an object to S3 or S3-compatible storage, by automatically computing the Content-Type.
	///
	/// If the supplied stream is not seekable or its length cannot be determined,
	/// the AWS SDK may buffer the entire stream into a `MemoryStream`
	/// before uploading, potentially consuming a large amount of memory.
	/// 
	/// </summary>
	public override async Task SetObject(string fullPath, Stream dataStream, bool append = false,
		CancellationToken cancellationToken = default) {
		await SetObject(fullPath, dataStream, null, append, cancellationToken).ConfigureAwait(false);
	}

	/// <summary>
	/// Uploads an object to S3 or S3-compatible storage, with the given Content-Type.
	///
	/// Uses `TransferUtility` API for AWS S3, MinIO, Wasabi, DigitalOcean Spaces.
	/// Uses `PutObjectAsync` API for Cloudflare R2.
	/// `TransferUtility` performs either a single PUT or a multipart upload depending on the stream size.
	///
	/// If the supplied stream is not seekable or its length cannot be determined,
	/// the AWS SDK may buffer the entire stream into a `MemoryStream`
	/// before uploading, potentially consuming a large amount of memory.
	/// 
	/// </summary>
	public override async Task SetObject(string fullPath, Stream dataStream, string contentType,
		bool append = false, CancellationToken cancellationToken = default) {

		if (append)
			throw new NotSupportedException();

		// Compute the full object path.
		if (fullPath == null) throw new ArgumentNullException(nameof(fullPath));
		fullPath = StoragePath.Normalize(fullPath);

		// Auto compute a MIME type (content type) if not given
		if (contentType == null) {
			contentType = MimeUtility.GetMimeMapping(fullPath);
		}

		// if PutObject API is required
		if (_usePutObject) {

			// Use PutObjectAsync for Cloudflare R2.
			var request = new PutObjectRequest {
				BucketName = _bucketName,
				Key = fullPath,
				InputStream = dataStream,
				ContentType = contentType,
				DisablePayloadSigning = _disablePayloadSigning // R2 does not support "Streaming Signature V4".
			};

			await _client.PutObjectAsync(request, cancellationToken).ConfigureAwait(false);

		}
		else {

			// Use TransferUtility for AWS S3, MinIO, Wasabi, DigitalOcean Spaces.
			var request = new TransferUtilityUploadRequest {
				BucketName = _bucketName,
				Key = fullPath,
				InputStream = dataStream,
				ContentType = contentType
			};

			await _fileTransferUtility.UploadAsync(request, cancellationToken).ConfigureAwait(false);

		}
	}

	/// <summary>
	/// Opens an object for reading and returns its content stream.
	///
	/// The returned stream wraps the AWS response stream and must be disposed by the caller,
	/// which also disposes the underlying HTTP response.
	/// Returns null if the blob does not exist.
	/// </summary>
	public override async Task<Stream> OpenRead(string fullPath, CancellationToken cancellationToken = default) {
		if (fullPath == null) throw new ArgumentNullException(nameof(fullPath));

		fullPath = StoragePath.Normalize(fullPath);
		GetObjectResponse response = await GetObjectAsync(fullPath).ConfigureAwait(false);
		if (response == null)
			return null;

		return new FixedStream(response.ResponseStream, length: response.ContentLength, (Action<FixedStream>)null);
	}

	/// <summary>
	/// Opens an object for writing and returns its content stream.
	/// Object will be written when the stream is disposed.
	/// </summary>
	public override async Task<Stream> OpenWrite(string fullPath, bool overwrite, CancellationToken cancellationToken = default) {
		if (fullPath == null) throw new ArgumentNullException(nameof(fullPath));

		// exit if file exists and overwriting is disabled
		if (!overwrite && await ObjectExists(fullPath, cancellationToken)) return null;

		fullPath = StoragePath.Normalize(fullPath);

		MemoryStream stream = new();

		return new FixedStream(stream, null, async s => {

			// write object on stream dispose
			s.Position = 0;

			PutObjectRequest request = new() {
				BucketName = _bucketName,
				Key = fullPath,
				InputStream = s
			};

			await _client.PutObjectAsync(request, cancellationToken).ConfigureAwait(false);
		});
	}

	/// <summary>
	/// Opens a readable stream beginning at the specified byte offset.
	/// 
	/// S3 returns:
	/// * 206 Partial Content for valid ranges.
	/// * 416 Requested Range Not Satisfiable if offset is beyond the end of the object.
	/// </summary>
	public override async Task<Stream> OpenRange(string fullPath,long offset,long length,CancellationToken cancellationToken = default) {
		AmazonS3Client client = await Client().ConfigureAwait(false);

		fullPath = StoragePath.Normalize(fullPath);

		var request = new GetObjectRequest {
			BucketName = BucketName,
			Key = fullPath
		};

		// Request the desired byte range.
		request.ByteRange = new ByteRange(offset, offset + length - 1);

		GetObjectResponse response = await client
			.GetObjectAsync(request, cancellationToken)
			.ConfigureAwait(false);

		return response.ResponseStream;
	}

	public override async Task<bool> IsSeekable() {
		return true;
	}

	public override async Task<long> GetObjectLength(string fullPath, long defaultValue = -1, CancellationToken cancellationToken = default) {
		try {
			AmazonS3Client client = await Client().ConfigureAwait(false);

			fullPath = StoragePath.Normalize(fullPath);

			var response = await client.GetObjectMetadataAsync(new GetObjectMetadataRequest {
				BucketName = BucketName,
				Key = fullPath
			}, cancellationToken).ConfigureAwait(false);

			return response != null && response.HttpStatusCode != HttpStatusCode.NotFound
				? response.ContentLength : defaultValue;
		}
		catch {
			return defaultValue;
		}
	}

	/// <summary>
	/// Deletes multiple objects in parallel.
	///
	/// Each path is processed independently, including deletion of any virtual directory
	/// placeholders beneath the object's path.
	/// </summary>
	public override async Task DeleteObjects(IEnumerable<string> fullPaths, CancellationToken cancellationToken = default) {
			
		await Task.WhenAll(fullPaths.Select(fullPath => DeleteObject(fullPath, cancellationToken))).ConfigureAwait(false);
	}

	/// <summary>
	/// Deletes an object.
	/// </summary>
	public override async Task DeleteObject(string fullPath, CancellationToken cancellationToken = default) {
		AmazonS3Client client = await Client().ConfigureAwait(false);

		if (fullPath == null) throw new ArgumentNullException(nameof(fullPath));

		fullPath = StoragePath.Normalize(fullPath);

		await client.DeleteObjectAsync(_bucketName, fullPath, cancellationToken).ConfigureAwait(false);
		/*using (var browser = new S3DirectoryBrowser(client, _bucketName)) {
			await browser.DeleteRecursiveAsync(fullPath, cancellationToken).ConfigureAwait(false);
		}*/
	}

	/// <summary>
	/// Determines whether each specified blob exists.
	///
	/// The existence checks are performed in parallel.
	/// </summary>
	public override async Task<List<bool>> ObjectsExists(IEnumerable<string> fullPaths, CancellationToken cancellationToken = default) {
			
		return (await Task.WhenAll(fullPaths.Select(fullPath => ObjectExists(fullPath, cancellationToken))).ConfigureAwait(false)).ToList();
	}

	/// <summary>
	/// Determines whether an object exists by requesting its metadata.
	/// </summary>
	public override async Task<bool> ObjectExists(string fullPath, CancellationToken cancellationToken = default) {
		if (fullPath == null) throw new ArgumentNullException(nameof(fullPath));

		try {
			AmazonS3Client client = await Client().ConfigureAwait(false);
			fullPath = StoragePath.Normalize(fullPath);
			await client.GetObjectMetadataAsync(_bucketName, fullPath, cancellationToken).ConfigureAwait(false);
			return true;
		}
		catch (AmazonS3Exception ex) when (ex.StatusCode == HttpStatusCode.NotFound) {

		}

		return false;
	}

	/// <summary>
	/// Retrieves metadata for multiple objects in parallel.
	///
	/// Blobs that do not exist are returned as null.
	/// </summary>
	public override async Task<List<StoreObject>> GetObjectsInfo(IEnumerable<string> fullPaths, CancellationToken cancellationToken = default) {
		return (await Task.WhenAll(fullPaths.Select(fullPath => GetObjectInfo(fullPath, cancellationToken))).ConfigureAwait(false)).ToList();
	}

	/// <summary>
	/// Retrieves a object's metadata without downloading its contents.
	///
	/// Returns null if the blob does not exist.
	/// </summary>
	public override async Task<StoreObject> GetObjectInfo(string fullPath, CancellationToken cancellationToken = default) {
		if (fullPath == null) throw new ArgumentNullException(nameof(fullPath));
		fullPath = StoragePath.Normalize(fullPath);

		AmazonS3Client client = await Client().ConfigureAwait(false);

		try {
			GetObjectMetadataResponse meta = await client.GetObjectMetadataAsync(_bucketName, fullPath).ConfigureAwait(false);
			return meta.ToBlob(fullPath);
		}
		catch (AmazonS3Exception ex) when (ex.StatusCode == HttpStatusCode.NotFound) {
			//if blob is not found, don't return any information
		}

		return null;
	}

	public override async Task SetObjectInfo(StoreObject obj, CancellationToken cancellationToken = default) {
		await SetObjectsInfo(new List<StoreObject> { obj }, cancellationToken).ConfigureAwait(false);
	}

	/// <summary>
	/// Updates the metadata for the specified blobs.
	///
	/// S3 metadata is immutable, so each update is implemented by copying the object
	/// onto itself with replacement metadata. Blob contents are not re-uploaded.
	/// Blobs with no metadata are skipped.
	/// </summary>
	public override async Task SetObjectsInfo(IEnumerable<StoreObject> objs, CancellationToken cancellationToken = default) {
		if (objs == null)
			return;

		AmazonS3Client client = await Client().ConfigureAwait(false);

		foreach (StoreObject blob in objs.Where(b => b != null)) {
			if (blob.Metadata != null) {
				await AwsConverter.UpdateMetadataAsync(
					client,
					blob,
					_bucketName,
					StoragePath.Normalize(blob.FullPath)).ConfigureAwait(false);
			}
		}
	}

	/// <summary>
	/// Retrieves an object from S3.
	///
	/// Returns null if the object does not exist; otherwise returns the full
	/// S3 response containing the content stream and object metadata.
	/// </summary>
	private async Task<GetObjectResponse> GetObjectAsync(string key) {
		var request = new GetObjectRequest { BucketName = _bucketName, Key = key };
		AmazonS3Client client = await Client().ConfigureAwait(false);

		try {
			GetObjectResponse response = await client.GetObjectAsync(request).ConfigureAwait(false);
			return response;
		}
		catch (AmazonS3Exception ex) {
			if (IsDoesntExist(ex))
				return null;

			TryHandleException(ex);
			throw;
		}
	}


	private static bool TryHandleException(AmazonS3Exception ex) {
		if (IsDoesntExist(ex)) {
			throw new StorageException(StorageErrorCode.NotFound, ex);
		}

		return false;
	}

	private static bool IsDoesntExist(AmazonS3Exception ex) {
		return ex.ErrorCode == "NoSuchKey";
	}

	/// <summary>
	/// Generates a pre-signed URL for the specified object.
	/// The URL grants temporary access to the object and expries after the specified duration. MIME type is auto computed.
	/// </summary>
	public override async Task<string> GetPresignedUrl(string fullPath, bool forDownload, bool https, int expiresInSeconds = 86000) {
		IAmazonS3 client = await Client().ConfigureAwait(false);

		var request = new GetPreSignedUrlRequest() {
			BucketName = _bucketName,
			Expires = DateTime.UtcNow.AddSeconds(expiresInSeconds),
			Key = StoragePath.Normalize(fullPath),
			Protocol = https ? Protocol.HTTPS : Protocol.HTTP,
			Verb = forDownload ? HttpVerb.GET : HttpVerb.PUT,
		};

		// Auto compute a MIME type (content type) based on the file path
		request.ContentType = MimeUtility.GetMimeMapping(fullPath);

		return await client.GetPreSignedURLAsync(request);
	}

	/// <summary>
	/// Generates a pre-signed URL for the specified object.
	/// The URL grants temporary access to the object and expries after the specified duration. MIME type is auto computed.
	/// </summary>
	public override async Task<string> GetObjectSas(string objectPath, StorageUrlOptions options) {

		if (options == null)
			throw new ArgumentNullException(nameof(options));

		// S3 implementation currently supports only the common options.
		return await GetPresignedUrl(
				objectPath,
				options.Permissions.HasFlag(StorageUrlPermissions.Read),
				options.RequireHttps,
				(int)options.ExpiresIn.TotalSeconds)
			.ConfigureAwait(false);
	}

	/// <summary>
	/// Sets the object's canned ACL.
	///
	/// The supplied ACL string must match one of the AWS predefined canned ACL values;
	/// otherwise an <see cref="ArgumentException"/> is thrown.
	/// </summary>
	public async Task SetAcl(string fullPath, string acl) {
		IAmazonS3 client = await Client().ConfigureAwait(false);
		var s3CannedAcl = S3CannedACL.FindValue(acl);
		if (s3CannedAcl is null) {
			throw new ArgumentException($"Unknown ACL value '{acl}'", acl);
		}

		await client.PutObjectAclAsync(new PutObjectAclRequest {
			BucketName = _bucketName,
			Key = StoragePath.Normalize(fullPath),
			ACL = s3CannedAcl
		});
	}

	/// <summary>
	/// Moves an object on the bucket. Returns true if it completed and false if it was skipped or the object did not exist.
	/// </summary>
	/// <param name="oldPath">Current object path.</param>
	/// <param name="newPath">New object path.</param>
	/// <param name="overwrite">Whether to overwrite the destination object if it already exists.</param>
	/// <param name="cancellationToken">Cancellation token.</param>
	public override async Task<bool> MoveObject(string oldPath, string newPath, bool overwrite, CancellationToken cancellationToken = default) {
		if (string.IsNullOrWhiteSpace(oldPath)) throw new ArgumentNullException(nameof(oldPath));
		if (string.IsNullOrWhiteSpace(newPath)) throw new ArgumentNullException(nameof(newPath));

		oldPath = StoragePath.Normalize(oldPath);
		newPath = StoragePath.Normalize(newPath);

		if (!await ObjectExists(oldPath, cancellationToken).ConfigureAwait(false))
			return false;

		if (!overwrite && await ObjectExists(newPath, cancellationToken).ConfigureAwait(false))
			return false;

		AmazonS3Client client = await Client().ConfigureAwait(false);

		await client.CopyObjectAsync(new CopyObjectRequest {
			SourceBucket = BucketName,
			SourceKey = oldPath,
			DestinationBucket = BucketName,
			DestinationKey = newPath
		}, cancellationToken).ConfigureAwait(false);

		await client.DeleteObjectAsync(BucketName, oldPath, cancellationToken).ConfigureAwait(false);

		return true;
	}

	/// <summary>
	/// Returns all available versions of the specified object.
	/// </summary>
	public override async Task<List<StorageObjectVersion>> ListObjectVersions(string objectPath, CancellationToken cancellationToken = default) {

		if (objectPath == null) throw new ArgumentNullException(nameof(objectPath));

		objectPath = StoragePath.Normalize(objectPath);

		AmazonS3Client client = await Client().ConfigureAwait(false);

		var result = new List<StorageObjectVersion>();

		string keyMarker = null;
		string versionMarker = null;

		try {

			do {

				var response = await client.ListVersionsAsync(new ListVersionsRequest {
					BucketName = _bucketName,
					Prefix = objectPath,
					KeyMarker = keyMarker,
					VersionIdMarker = versionMarker
				}, cancellationToken).ConfigureAwait(false);

				foreach (S3ObjectVersion version in response.Versions) {

					if (!string.Equals(version.Key, objectPath, StringComparison.Ordinal))
						continue;

					result.Add(VersionToObject(version));
				}

				keyMarker = response.NextKeyMarker;
				versionMarker = response.NextVersionIdMarker;

				if (response.IsTruncated != true)
					break;

			} while (true);

			return result;
		}
		catch (AmazonS3Exception ex) when (
			ex.StatusCode == HttpStatusCode.NotFound ||
			ex.ErrorCode == "NoSuchBucket") {

			// Returns an empty collection if versioning is not enabled, or no versions exist, or the object does not exist.
			return new List<StorageObjectVersion>();
		}
	}

	private static StorageObjectVersion VersionToObject(S3ObjectVersion version) {
		return new StorageObjectVersion {
			VersionId = version.VersionId,
			IsCurrent = version.IsLatest ?? false,
			DateCreated = version.LastModified ?? DateTime.MinValue,
			Length = version.Size ?? 0,
			ETag = version.ETag
		};
	}

	/// <summary>
	/// Returns information about a specific version of an object.
	/// </summary>
	public override async Task<StorageObjectVersion> GetObjectVersion(string objectPath, string versionId, CancellationToken cancellationToken = default) {

		if (objectPath == null) throw new ArgumentNullException(nameof(objectPath));
		if (string.IsNullOrWhiteSpace(versionId))throw new ArgumentNullException(nameof(versionId));

		objectPath = StoragePath.Normalize(objectPath);

		AmazonS3Client client = await Client().ConfigureAwait(false);

		string keyMarker = null;
		string versionMarker = null;

		try {

			do {

				ListVersionsResponse response = await client.ListVersionsAsync(new ListVersionsRequest {
					BucketName = _bucketName,
					Prefix = objectPath,
					KeyMarker = keyMarker,
					VersionIdMarker = versionMarker
				}, cancellationToken).ConfigureAwait(false);

				foreach (S3ObjectVersion version in response.Versions) {

					if (!string.Equals(version.Key, objectPath, StringComparison.Ordinal))
						continue;

					if (!string.Equals(version.VersionId, versionId, StringComparison.Ordinal))
						continue;

					return VersionToObject(version);
				}

				if (response.IsTruncated != true)
					break;

				keyMarker = response.NextKeyMarker;
				versionMarker = response.NextVersionIdMarker;

			} while (true);

			// Returns null if the version or object does not exist.
			return null;
		}
		catch (AmazonS3Exception ex) when (
			ex.StatusCode == HttpStatusCode.NotFound ||
			ex.ErrorCode == "NoSuchBucket") {

			return null;
		}
	}

	/// <summary>
	/// Restores the specified version as the current version of the object.
	/// Provider-specific restore semantics may apply.
	/// </summary>
	public override async Task<bool> RestoreObjectVersion(string objectPath, string versionId, CancellationToken cancellationToken = default) {

		if (objectPath == null) throw new ArgumentNullException(nameof(objectPath));
		if (string.IsNullOrWhiteSpace(versionId))throw new ArgumentNullException(nameof(versionId));

		objectPath = StoragePath.Normalize(objectPath);

		AmazonS3Client client = await Client().ConfigureAwait(false);

		if (!await ObjectExists(objectPath, cancellationToken).ConfigureAwait(false))
			return false;

		try {

			await client.CopyObjectAsync(new CopyObjectRequest {
				SourceBucket = _bucketName,
				SourceKey = objectPath,
				SourceVersionId = versionId,
				DestinationBucket = _bucketName,
				DestinationKey = objectPath,
				MetadataDirective = S3MetadataDirective.COPY
			}, cancellationToken).ConfigureAwait(false);

			return true;
		}
		catch (AmazonS3Exception ex) when (
			ex.StatusCode == HttpStatusCode.NotFound ||
			ex.ErrorCode == "NoSuchKey" || ex.ErrorCode == "NoSuchVersion") {

			// Returns true if restored, or false if the object was not found.
			return false;
		}
	}

	/// <summary>
	/// Permanently deletes the specified object version.
	/// Does not delete other versions of the object.
	/// </summary>
	public override async Task<bool> DeleteObjectVersion(string objectPath, string versionId, CancellationToken cancellationToken = default) {

		if (objectPath == null) throw new ArgumentNullException(nameof(objectPath));
		if (string.IsNullOrWhiteSpace(versionId))throw new ArgumentNullException(nameof(versionId));

		objectPath = StoragePath.Normalize(objectPath);

		AmazonS3Client client = await Client().ConfigureAwait(false);

		try {

			DeleteObjectResponse response = await client.DeleteObjectAsync(new DeleteObjectRequest {
				BucketName = _bucketName,
				Key = objectPath,
				VersionId = versionId
			}, cancellationToken).ConfigureAwait(false);

			return response.HttpStatusCode == HttpStatusCode.NoContent;
		}
		catch (AmazonS3Exception ex) when (
			ex.StatusCode == HttpStatusCode.NotFound ||
			ex.ErrorCode == "NoSuchKey" ||
			ex.ErrorCode == "NoSuchVersion") {

			// Returns true if deleted, or false if the object was not found.
			return false;
		}
	}

	/// <summary>
	/// Returns true if bucket versioning is enabled.
	/// </summary>
	public override async Task<bool> IsVersioned() {
		AmazonS3Client client = await Client().ConfigureAwait(false);

		GetBucketVersioningResponse response = await client.GetBucketVersioningAsync(new GetBucketVersioningRequest {
			BucketName = _bucketName
		}).ConfigureAwait(false);

		return response.VersioningConfig.Status == VersionStatus.Enabled;
	}

	/// <summary>
	/// Returns all tags associated with the specified object.
	/// Returns an empty collection if no tags exist.
	/// </summary>
	public override async Task<Dictionary<string, string>> GetObjectTags(string objectPath, CancellationToken cancellationToken = default) {
		if (string.IsNullOrWhiteSpace(objectPath))
			throw new ArgumentNullException(nameof(objectPath));

		objectPath = StoragePath.Normalize(objectPath);

		AmazonS3Client client = await Client().ConfigureAwait(false);

		try {
			var response = await client.GetObjectTaggingAsync(new GetObjectTaggingRequest {
				BucketName = _bucketName,
				Key = objectPath
			}, cancellationToken).ConfigureAwait(false);

			Dictionary<string, string> tags = response.Tagging?
				                                  .ToDictionary(x => x.Key, x => x.Value)
			                                  ?? new Dictionary<string, string>();

			return tags;
		}
		catch (AmazonS3Exception ex) when (ex.StatusCode == HttpStatusCode.NotFound) {
			// Returns null if the object cannot be found.
			return null;
		}
	}


	/// <summary>
	/// Replaces all tags associated with the specified object.
	/// Existing tags are removed before the new tags are applied.
	/// </summary>
	public override async Task<bool> SetObjectTags(string objectPath, Dictionary<string, string> tags, CancellationToken cancellationToken = default) {
		if (string.IsNullOrWhiteSpace(objectPath))
			throw new ArgumentNullException(nameof(objectPath));

		objectPath = StoragePath.Normalize(objectPath);

		AmazonS3Client client = await Client().ConfigureAwait(false);

		try {
			await client.PutObjectTaggingAsync(new PutObjectTaggingRequest {
				BucketName = _bucketName,
				Key = objectPath,
				Tagging = new Tagging {
					TagSet = tags?.Select(x => new Tag {
						Key = x.Key,
						Value = x.Value
					}).ToList() ?? new List<Tag>()
				}
			}, cancellationToken).ConfigureAwait(false);

			return true;
		}
		catch (AmazonS3Exception ex) when (ex.StatusCode == HttpStatusCode.NotFound) {

			// Returns true if succeeded, or false if the object cannot be found.
			return false;
		}
	}


	/// <summary>
	/// Removes all tags from the specified object.
	/// Does nothing if the object has no tags.
	/// </summary>
	public override async Task<bool> DeleteObjectTags(string objectPath, CancellationToken cancellationToken = default) {
		if (string.IsNullOrWhiteSpace(objectPath))
			throw new ArgumentNullException(nameof(objectPath));

		objectPath = StoragePath.Normalize(objectPath);

		AmazonS3Client client = await Client().ConfigureAwait(false);

		try {
			await client.DeleteObjectTaggingAsync(new DeleteObjectTaggingRequest {
				BucketName = _bucketName,
				Key = objectPath
			}, cancellationToken).ConfigureAwait(false);

			return true;
		}
		catch (AmazonS3Exception ex) when (ex.StatusCode == HttpStatusCode.NotFound) {
			// Returns true if succeeded, or false if the object cannot be found.
			return false;
		}
	}

	public override async Task<bool> IsTagged() {
		return true;
	}


	/// <summary>
	/// Returns the storage tier or storage class of the specified object.
	/// </summary>
	public override async Task<StorageTier> GetObjectTier(string objectPath, CancellationToken cancellationToken = default) {
		if (string.IsNullOrWhiteSpace(objectPath))
			throw new ArgumentNullException(nameof(objectPath));

		objectPath = StoragePath.Normalize(objectPath);

		AmazonS3Client client = await Client().ConfigureAwait(false);

		try {
			GetObjectMetadataResponse response = await client.GetObjectMetadataAsync(new GetObjectMetadataRequest {
				BucketName = _bucketName,
				Key = objectPath
			}, cancellationToken).ConfigureAwait(false);

			return AwsTier.ToFluentTier.TryGetValue(response.StorageClass, out StorageTier tier)
				? tier
				: StorageTier.Unknown;
		}
		catch (AmazonS3Exception ex) when (ex.StatusCode == HttpStatusCode.NotFound) {
			// Returns NotFound if the object cannot be found.
			return StorageTier.NotFound;
		}
	}


	/// <summary>
	/// Changes the storage tier or storage class of the specified object.
	/// </summary>
	public override async Task<bool> SetObjectTier(string objectPath, StorageTier tier, CancellationToken cancellationToken = default) {
		if (string.IsNullOrWhiteSpace(objectPath))
			throw new ArgumentNullException(nameof(objectPath));

		if (!AwsTier.FromFluentTier.TryGetValue(tier, out S3StorageClass storageClass))
			throw new StorageException($"AWS S3 does not support the tier \"{tier}\". Use a supported tier and try again.");

		objectPath = StoragePath.Normalize(objectPath);

		AmazonS3Client client = await Client().ConfigureAwait(false);

		try {
			await client.CopyObjectAsync(new CopyObjectRequest {
				SourceBucket = _bucketName,
				SourceKey = objectPath,
				DestinationBucket = _bucketName,
				DestinationKey = objectPath,
				StorageClass = storageClass,
				MetadataDirective = S3MetadataDirective.COPY
			}, cancellationToken).ConfigureAwait(false);

			return true;
		}
		catch (AmazonS3Exception ex) when (ex.StatusCode == HttpStatusCode.NotFound) {
			// Returns true if succeeded, or false if the object cannot be found.
			return false;
		}
	}

	public override async Task<bool> IsTiered() {
		return true;
	}

	/// <summary>
	/// Fastest possible implemention to check if a virtual directory exists in an S3 bucket.
	/// ListObjectsV2 request with `MaxKeys = 1`, so S3 will stop after finding the first matching object.
	/// </summary>
	public override async Task<bool> DirectoryExists(string fullPath, CancellationToken cancellationToken = default) {
		if (fullPath == null) throw new ArgumentNullException(nameof(fullPath));
		fullPath = StoragePath.Normalize(fullPath);

		// do not check root directory
		if (fullPath.Length == 0) return true;

		AmazonS3Client client = await Client().ConfigureAwait(false);

		// Virtual directories must end with '/'
		if (fullPath.Length > 0 && !fullPath.EndsWith("/"))
			fullPath += "/";

		ListObjectsV2Response response = await client.ListObjectsV2Async(new ListObjectsV2Request {
			BucketName = BucketName,
			Prefix = fullPath,
			MaxKeys = 1
		}, cancellationToken).ConfigureAwait(false);

		return response.S3Objects.Count > 0;
	}

}
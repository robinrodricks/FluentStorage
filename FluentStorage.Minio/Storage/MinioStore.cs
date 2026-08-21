using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentStorage.Enums;
using FluentStorage.Exceptions;
using FluentStorage.Minio.Utils;
using FluentStorage.Model;
using FluentStorage.Storage;
using FluentStorage.Streaming;
using MimeMapping;
using Minio;
using Minio.Credentials;
using Minio.DataModel;
using Minio.DataModel.Args;
using Minio.DataModel.ILM;
using Minio.DataModel.Tags;
using Minio.Exceptions;

namespace FluentStorage.Minio.Storage;

/// <summary>
/// Manages a single MinIO bucket using the native Minio SDK.
/// </summary>
public class MinioStore : StoreBase {
	private readonly IMinioClient _client;
	private readonly string _bucketName;
	private bool _bucketChecked;
	private readonly SemaphoreSlim _bucketCheckLock = new SemaphoreSlim(1, 1);

	public MinioStore(string endpoint, string accessKey, string secretKey, string bucketName,
		bool useSsl = true, string region = null) {
		if (string.IsNullOrWhiteSpace(endpoint)) throw new ArgumentNullException(nameof(endpoint));
		if (string.IsNullOrWhiteSpace(accessKey)) throw new ArgumentNullException(nameof(accessKey));
		if (string.IsNullOrWhiteSpace(secretKey)) throw new ArgumentNullException(nameof(secretKey));
		if (string.IsNullOrWhiteSpace(bucketName)) throw new ArgumentNullException(nameof(bucketName));

		_bucketName = bucketName;

		var builder = new MinioClient()
			.WithEndpoint(endpoint)
			.WithCredentials(accessKey, secretKey)
			.WithSSL(useSsl);

		if (!string.IsNullOrWhiteSpace(region))
			builder = builder.WithRegion(region);

		_client = builder.Build();
	}

	public MinioStore(string endpoint, string accessKey, string secretKey, string sessionToken,
		string bucketName, bool useSsl = true, string region = null) {
		if (string.IsNullOrWhiteSpace(endpoint)) throw new ArgumentNullException(nameof(endpoint));
		if (string.IsNullOrWhiteSpace(accessKey)) throw new ArgumentNullException(nameof(accessKey));
		if (string.IsNullOrWhiteSpace(secretKey)) throw new ArgumentNullException(nameof(secretKey));
		if (string.IsNullOrWhiteSpace(sessionToken)) throw new ArgumentNullException(nameof(sessionToken));
		if (string.IsNullOrWhiteSpace(bucketName)) throw new ArgumentNullException(nameof(bucketName));

		_bucketName = bucketName;

		var builder = new MinioClient()
			.WithEndpoint(endpoint)
			.WithCredentials(accessKey, secretKey)
			.WithSessionToken(sessionToken)
			.WithSSL(useSsl);

		if (!string.IsNullOrWhiteSpace(region))
			builder = builder.WithRegion(region);

		_client = builder.Build();
	}

	public MinioStore(IMinioClient existingClient, string bucketName) {
		_client = existingClient ?? throw new ArgumentNullException(nameof(existingClient));
		if (string.IsNullOrWhiteSpace(bucketName)) throw new ArgumentNullException(nameof(bucketName));
		_bucketName = bucketName;
	}

	public MinioStore(string endpoint, string bucketName, bool useSsl = true, string region = null,
		string iamEndpoint = null) {
		if (string.IsNullOrWhiteSpace(endpoint)) throw new ArgumentNullException(nameof(endpoint));
		if (string.IsNullOrWhiteSpace(bucketName)) throw new ArgumentNullException(nameof(bucketName));

		_bucketName = bucketName;

		var baseClient = BuildBaseClient(endpoint, useSsl, region);

		var provider = new IAMAWSProvider(iamEndpoint, baseClient);

		var builder = new MinioClient()
			.WithEndpoint(endpoint)
			.WithCredentialsProvider(provider)
			.WithSSL(useSsl);

		if (!string.IsNullOrWhiteSpace(region))
			builder = builder.WithRegion(region);

		_client = builder.Build();
	}

	/// <summary>
	/// Creates a store using STS AssumeRole credentials: an initial set of long-lived
	/// access/secret keys is exchanged for short-lived, auto-refreshed session credentials
	/// scoped to <paramref name="roleArn"/>.
	/// </summary>
	public MinioStore(string endpoint, string accessKey, string secretKey, string roleArn,
		string bucketName, string roleSessionName = null, string externalId = null, string policy = null,
		uint durationInSeconds = 3600, bool useSsl = true, string region = null, string stsEndpoint = null) {
		if (string.IsNullOrWhiteSpace(endpoint)) throw new ArgumentNullException(nameof(endpoint));
		if (string.IsNullOrWhiteSpace(accessKey)) throw new ArgumentNullException(nameof(accessKey));
		if (string.IsNullOrWhiteSpace(secretKey)) throw new ArgumentNullException(nameof(secretKey));
		if (string.IsNullOrWhiteSpace(roleArn)) throw new ArgumentNullException(nameof(roleArn));
		if (string.IsNullOrWhiteSpace(bucketName)) throw new ArgumentNullException(nameof(bucketName));

		_bucketName = bucketName;

		// AssumeRoleProvider signs its initial STS call using this base client's static credentials.
		var baseClient = new MinioClient()
			.WithEndpoint(stsEndpoint ?? endpoint)
			.WithCredentials(accessKey, secretKey)
			.WithSSL(useSsl)
			.Build();

		var provider = new AssumeRoleProvider(baseClient)
			.WithRoleARN(roleArn)
			.WithDurationInSeconds(durationInSeconds)
			.WithSTSEndpoint(stsEndpoint ?? endpoint);

		if (!string.IsNullOrWhiteSpace(roleSessionName))
			provider = provider.WithRoleSessionName(roleSessionName);
		if (!string.IsNullOrWhiteSpace(policy))
			provider = provider.WithPolicy(policy);
		if (!string.IsNullOrWhiteSpace(externalId))
			provider = provider.WithExternalID(externalId);

		var builder = new MinioClient()
			.WithEndpoint(endpoint)
			.WithCredentialsProvider(provider)
			.WithSSL(useSsl);

		if (!string.IsNullOrWhiteSpace(region))
			builder = builder.WithRegion(region);

		_client = builder.Build();
	}

	private static IMinioClient BuildBaseClient(string endpoint, bool useSsl, string region) {
		var builder = new MinioClient().WithEndpoint(endpoint).WithSSL(useSsl);
		if (!string.IsNullOrWhiteSpace(region))
			builder = builder.WithRegion(region);
		return builder.Build();
	}


	/// <summary>
	/// Returns the underlying SDK client, checking bucket existence the first time it's called.
	/// </summary>
	private async Task<IMinioClient> Client(CancellationToken cancellationToken = default) {
		if (_bucketChecked) return _client;

		await _bucketCheckLock.WaitAsync(cancellationToken).ConfigureAwait(false);
		try {
			if (!_bucketChecked) {
				bool exists = await _client
					.BucketExistsAsync(new BucketExistsArgs().WithBucket(_bucketName), cancellationToken)
					.ConfigureAwait(false);

				if (!exists)
					throw new StorageException($"Bucket '{_bucketName}' does not exist!");

				_bucketChecked = true;
			}
		}
		finally {
			_bucketCheckLock.Release();
		}

		return _client;
	}

	public override async Task<object> GetClient() {
		return (await Client().ConfigureAwait(false)) as MinioClient;
	}



	private static (string folder, string name) SplitPath(string key) {
		key = key.TrimEnd('/');
		int idx = key.LastIndexOf('/');
		return idx < 0 ? (string.Empty, key) : (key.Substring(0, idx), key.Substring(idx + 1));
	}

	private static string CombineFullPath(StoreObject obj) {
		return string.IsNullOrEmpty(obj.FolderPath) ? obj.Name : obj.FolderPath.TrimEnd('/') + "/" + obj.Name;
	}

	private static string NormalizeFolderPrefix(string folderPath) {
		if (string.IsNullOrEmpty(folderPath)) return string.Empty;
		return StoragePath.Normalize(folderPath).TrimEnd('/') + "/";
	}



	public override async Task<List<StoreObject>> ListObjects(StorageListOptions options = null,
		CancellationToken cancellationToken = default) {
		options ??= new StorageListOptions();

		var client = await Client(cancellationToken).ConfigureAwait(false);
		string rootPrefix = NormalizeFolderPrefix(options.FolderPath);

		var result = new List<StoreObject>();
		await ListObjectsInternal(client, rootPrefix, options, result, cancellationToken).ConfigureAwait(false);

		if (options.MaxResults.HasValue && result.Count > options.MaxResults.Value)
			result = result.Take(options.MaxResults.Value).ToList();

		return result;
	}

	private async Task ListObjectsInternal(IMinioClient client, string prefix, StorageListOptions options,
		List<StoreObject> result, CancellationToken cancellationToken) {
		bool remoteRecurse = options.Recurse && options.RecursionMode == StorageRecursion.Remote;

		var args = new ListObjectsArgs()
			.WithBucket(_bucketName)
			.WithPrefix(prefix)
			.WithRecursive(remoteRecurse);

		var localSubFolders = new List<string>();

		await foreach (Item item in client.ListObjectsEnumAsync(args, cancellationToken).ConfigureAwait(false)) {
			if (options.MaxResults.HasValue && result.Count >= options.MaxResults.Value)
				break;

			string key = item.Key?.TrimEnd('/');
			if (string.IsNullOrEmpty(key)) continue;

			if (item.IsDir) {
				var (folder, name) = SplitPath(key);
				result.Add(new StoreObject(folder, name, StorageObjectType.Folder));

				if (options.Recurse && options.RecursionMode == StorageRecursion.Local)
					localSubFolders.Add(item.Key);

				continue;
			}

			var (fp, fn) = SplitPath(key);

			if (!string.IsNullOrEmpty(options.FilePrefix) &&
			    !fn.StartsWith(options.FilePrefix, StringComparison.OrdinalIgnoreCase)) {
				continue;
			}

			var so = new StoreObject(fp, fn, StorageObjectType.File) {
				Size = checked((long)item.Size),
				MD5 = item.ETag,
				DateModified = item.LastModifiedDateTime == default
					? (DateTimeOffset?)null
					: new DateTimeOffset(item.LastModifiedDateTime.GetValueOrDefault(), TimeSpan.Zero)
			};

			if (options.IncludeAttributes) {
				so.TryAddProperties(
					"ETag", item.ETag,
					//"StorageClass", item.StorageClass,
					"IsLatest", item.IsLatest,
					"VersionId", item.VersionId);
			}

			result.Add(so);
		}

		if (options.Recurse && options.RecursionMode == StorageRecursion.Local) {
			foreach (string subFolderKey in localSubFolders) {
				if (options.MaxResults.HasValue && result.Count >= options.MaxResults.Value)
					break;

				await ListObjectsInternal(client, subFolderKey, options, result, cancellationToken)
					.ConfigureAwait(false);
			}
		}
	}



	public override async Task SetObject(string fullPath, Stream dataStream, bool append = false,
		CancellationToken cancellationToken = default) {
		await SetObject(fullPath, dataStream, null, append, cancellationToken).ConfigureAwait(false);
	}

	public override async Task SetObject(string fullPath, Stream dataStream, string contentType,
		bool append = false, CancellationToken cancellationToken = default) {
		if (string.IsNullOrWhiteSpace(fullPath)) throw new ArgumentNullException(nameof(fullPath));
		if (dataStream == null) throw new ArgumentNullException(nameof(dataStream));

		var client = await Client(cancellationToken).ConfigureAwait(false);
		string key = StoragePath.Normalize(fullPath);

		if (string.IsNullOrWhiteSpace(contentType))
			contentType = MimeUtility.GetMimeMapping(fullPath);

		Stream uploadStream = dataStream;
		bool disposeUploadStream = false;

		try {

			// MinIO has no native "append" operation - emulate it by downloading the
			// existing object and concatenating the new bytes after it, then re-uploading.
			if (append && await ObjectExists(fullPath, cancellationToken).ConfigureAwait(false)) {
				var combined = new MemoryStream();
				using (Stream existing = await OpenRead(fullPath, cancellationToken).ConfigureAwait(false)) {
					await existing.CopyToAsync(combined, 81920, cancellationToken).ConfigureAwait(false);
				}
				await dataStream.CopyToAsync(combined, 81920, cancellationToken).ConfigureAwait(false);
				combined.Position = 0;
				uploadStream = combined;
				disposeUploadStream = true;
			}

			// PutObjectArgs needs an up-front size, so buffer non-seekable streams first
			if (!uploadStream.CanSeek) {
				var buffered = new MemoryStream();
				await uploadStream.CopyToAsync(buffered, 81920, cancellationToken).ConfigureAwait(false);
				buffered.Position = 0;
				if (disposeUploadStream) uploadStream.Dispose();
				uploadStream = buffered;
				disposeUploadStream = true;
			}

			long size = uploadStream.Length - uploadStream.Position;

			var putArgs = new PutObjectArgs()
				.WithBucket(_bucketName)
				.WithObject(key)
				.WithStreamData(uploadStream)
				.WithObjectSize(size)
				.WithContentType(contentType);

			await client.PutObjectAsync(putArgs, cancellationToken).ConfigureAwait(false);
		}
		finally {
			if (disposeUploadStream) uploadStream.Dispose();
		}
	}

	public override async Task<Stream> OpenRead(string fullPath, CancellationToken cancellationToken = default) {
		if (string.IsNullOrWhiteSpace(fullPath)) throw new ArgumentNullException(nameof(fullPath));

		var client = await Client(cancellationToken).ConfigureAwait(false);
		string key = StoragePath.Normalize(fullPath);

		try {
			var ms = new MemoryStream();

			var getArgs = new GetObjectArgs()
				.WithBucket(_bucketName)
				.WithObject(key)
				.WithCallbackStream(async (stream, ct) => {
					await stream.CopyToAsync(ms, 81920, ct).ConfigureAwait(false);
				});

			await client.GetObjectAsync(getArgs, cancellationToken).ConfigureAwait(false);

			ms.Position = 0;
			return ms;
		}
		catch (ObjectNotFoundException) {
			// if not found, just return null and don't throw anything
			return null;
		}
	}

	public override async Task<Stream> OpenWrite(string fullPath, bool overwrite,
		CancellationToken cancellationToken = default) {
		if (string.IsNullOrWhiteSpace(fullPath)) throw new ArgumentNullException(nameof(fullPath));

		if (!overwrite && await ObjectExists(fullPath, cancellationToken).ConfigureAwait(false))
			return null;
		//throw new StorageException($"Object '{fullPath}' already exists and overwrite is disabled.");

		var buffer = new MemoryStream();

		return new FixedStream(buffer, null, async s => {

			// write to cloud when stream is disposed
			s.Position = 0;
			await SetObject(fullPath, s, null, false, cancellationToken).ConfigureAwait(false);
		});
	}

	public override async Task<Stream> OpenRange(string fullPath, long offset, long length, CancellationToken cancellationToken = default) {
		if (string.IsNullOrWhiteSpace(fullPath)) throw new ArgumentNullException(nameof(fullPath));

		var client = await Client(cancellationToken).ConfigureAwait(false);
		string key = StoragePath.Normalize(fullPath);

		try {

			var stream = new MemoryStream();

			var args = new GetObjectArgs().WithBucket(_bucketName).WithObject(key).WithOffsetAndLength(offset, length);

			await client.GetObjectAsync(
				args.WithCallbackStream(async s => {
					await s.CopyToAsync(stream, (int)length, cancellationToken).ConfigureAwait(false);
				}),
				cancellationToken).ConfigureAwait(false);

			stream.Position = 0;
			return stream;
		}
		catch (ObjectNotFoundException) {
			// if not found, just return null and don't throw anything
			return null;
		}
	}

	public override async Task<bool> IsSeekable() {
		return true;
	}

	public override async Task<long> GetObjectLength(string fullPath, long defaultValue = -1, CancellationToken cancellationToken = default) {
		try {
			if (string.IsNullOrWhiteSpace(fullPath)) return defaultValue;

			var client = await Client(cancellationToken).ConfigureAwait(false);
			string key = StoragePath.Normalize(fullPath);

			var args = new StatObjectArgs().WithBucket(_bucketName).WithObject(key);

			ObjectStat stat = await client.StatObjectAsync(args, cancellationToken).ConfigureAwait(false);

			return stat != null ? stat.Size : defaultValue;
		}
		catch {
			return defaultValue;
		}
	}

	public override async Task DeleteObjects(IEnumerable<string> fullPaths,
		CancellationToken cancellationToken = default) {
		if (fullPaths == null) throw new ArgumentNullException(nameof(fullPaths));

		var client = await Client(cancellationToken).ConfigureAwait(false);
		List<string> keys = fullPaths.Where(p => !string.IsNullOrWhiteSpace(p)).Select(StoragePath.Normalize).ToList();
		if (keys.Count == 0) return;

		var removeArgs = new RemoveObjectsArgs()
			.WithBucket(_bucketName)
			.WithObjects(keys);

		await client.RemoveObjectsAsync(removeArgs, cancellationToken).ConfigureAwait(false);
	}

	public override async Task DeleteObject(string fullPath, CancellationToken cancellationToken = default) {
		if (string.IsNullOrWhiteSpace(fullPath)) throw new ArgumentNullException(nameof(fullPath));

		var client = await Client(cancellationToken).ConfigureAwait(false);

		var removeArgs = new RemoveObjectArgs()
			.WithBucket(_bucketName)
			.WithObject(StoragePath.Normalize(fullPath));

		await client.RemoveObjectAsync(removeArgs, cancellationToken).ConfigureAwait(false);
	}

	public override async Task DeleteDirectory(string folderPath, bool recursive,
		CancellationToken cancellationToken = default) {
		if (string.IsNullOrWhiteSpace(folderPath)) throw new ArgumentNullException(nameof(folderPath));

		List<StoreObject> items = await ListDirectory(folderPath, recursive, cancellationToken)
			.ConfigureAwait(false);

		if (items == null || items.Count == 0) return;

		List<string> filePaths = items
			.Where(i => i.Type == StorageObjectType.File)
			.Select(CombineFullPath)
			.ToList();

		if (filePaths.Count > 0)
			await DeleteObjects(filePaths, cancellationToken).ConfigureAwait(false);

		// TODO: enable if required
		/*if (recursive) {
			foreach (StoreObject folder in items.Where(i => i.Type == StorageObjectType.Folder)) {
				await DeleteDirectory(CombineFullPath(folder), true, cancellationToken).ConfigureAwait(false);
			}
		}*/
	}



	public override async Task<bool> ObjectExists(string fullPath, CancellationToken cancellationToken = default) {
		if (string.IsNullOrWhiteSpace(fullPath)) throw new ArgumentNullException(nameof(fullPath));

		var client = await Client(cancellationToken).ConfigureAwait(false);

		try {
			var statArgs = new StatObjectArgs()
				.WithBucket(_bucketName)
				.WithObject(StoragePath.Normalize(fullPath));

			await client.StatObjectAsync(statArgs, cancellationToken).ConfigureAwait(false);
			return true;
		}
		catch (ObjectNotFoundException) {
			// This is the SDK's normal way of signalling "not found" from a stat call -
			// not a real error, so it's safe (and necessary) to suppress it here.
			return false;
		}
	}

	public override async Task<List<bool>> ObjectsExists(IEnumerable<string> fullPaths,
		CancellationToken cancellationToken = default) {
		if (fullPaths == null) throw new ArgumentNullException(nameof(fullPaths));

		var results = new List<bool>();
		foreach (string path in fullPaths)
			results.Add(await ObjectExists(path, cancellationToken).ConfigureAwait(false));

		return results;
	}

	public override async Task<StoreObject> GetObjectInfo(string fullPath,
		CancellationToken cancellationToken = default) {
		if (string.IsNullOrWhiteSpace(fullPath)) throw new ArgumentNullException(nameof(fullPath));

		var client = await Client(cancellationToken).ConfigureAwait(false);
		string key = StoragePath.Normalize(fullPath);

		var statArgs = new StatObjectArgs()
			.WithBucket(_bucketName)
			.WithObject(key);

		ObjectStat stat = await client.StatObjectAsync(statArgs, cancellationToken).ConfigureAwait(false);

		var (folder, name) = SplitPath(key);
		var so = new StoreObject(folder, name, StorageObjectType.File) {
			Size = stat.Size,
			MD5 = stat.ETag,
			DateModified = stat.LastModified == default
				? (DateTimeOffset?)null
				: new DateTimeOffset(stat.LastModified, TimeSpan.Zero)
		};

		so.TryAddProperties("ContentType", stat.ContentType, "ETag", stat.ETag);

		if (stat.MetaData != null) {
			foreach (var kv in stat.MetaData)
				so.Metadata[kv.Key] = kv.Value;
		}

		return so;
	}

	public override async Task<List<StoreObject>> GetObjectsInfo(IEnumerable<string> fullPaths,
		CancellationToken cancellationToken = default) {
		if (fullPaths == null) throw new ArgumentNullException(nameof(fullPaths));

		var results = new List<StoreObject>();
		foreach (string path in fullPaths)
			results.Add(await GetObjectInfo(path, cancellationToken).ConfigureAwait(false));

		return results;
	}

	public override async Task SetObjectInfo(StoreObject obj, CancellationToken cancellationToken = default) {
		if (obj == null) throw new ArgumentNullException(nameof(obj));

		var client = await Client(cancellationToken).ConfigureAwait(false);
		string key = StoragePath.Normalize(CombineFullPath(obj));

		// MinIO/S3 has no in-place metadata-update API. The standard technique is a
		// self-copy with the metadata-replace directive set.
		var copySource = new CopySourceObjectArgs()
			.WithBucket(_bucketName)
			.WithObject(key);

		var copyArgs = new CopyObjectArgs()
			.WithBucket(_bucketName)
			.WithObject(key)
			.WithCopyObjectSource(copySource)
			.WithHeaders(new Dictionary<string, string>(obj.Metadata))
			.WithReplaceMetadataDirective(true);

		await client.CopyObjectAsync(copyArgs, cancellationToken).ConfigureAwait(false);
	}

	public override async Task SetObjectsInfo(IEnumerable<StoreObject> objs,
		CancellationToken cancellationToken = default) {
		if (objs == null) throw new ArgumentNullException(nameof(objs));

		foreach (StoreObject obj in objs)
			await SetObjectInfo(obj, cancellationToken).ConfigureAwait(false);
	}



	public override async Task<string> GetPresignedUrl(string fullPath, bool forDownload, bool https,
		int expiresInSeconds = 86000) {
		if (string.IsNullOrWhiteSpace(fullPath)) throw new ArgumentNullException(nameof(fullPath));

		var client = await Client().ConfigureAwait(false);
		string key = StoragePath.Normalize(fullPath);

		string url;

		if (forDownload) {
			string contentType = MimeUtility.GetMimeMapping(fullPath);

			var reqParams = new Dictionary<string, string> {
				["response-content-type"] = contentType
			};

			var presignedGetArgs = new PresignedGetObjectArgs()
				.WithBucket(_bucketName)
				.WithObject(key)
				.WithExpiry(expiresInSeconds)
				.WithHeaders(reqParams);

			url = await client.PresignedGetObjectAsync(presignedGetArgs).ConfigureAwait(false);
		}
		else {
			var presignedPutArgs = new PresignedPutObjectArgs()
				.WithBucket(_bucketName)
				.WithObject(key)
				.WithExpiry(expiresInSeconds);

			url = await client.PresignedPutObjectAsync(presignedPutArgs).ConfigureAwait(false);
		}

		// The SDK signs using the scheme configured on the client. If a different scheme was
		// explicitly requested, patch it in - the signature itself doesn't depend on the scheme.
		if (https && url.StartsWith("http://", StringComparison.OrdinalIgnoreCase))
			url = "https://" + url.Substring("http://".Length);
		else if (!https && url.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
			url = "http://" + url.Substring("https://".Length);

		return url;
	}

	public override async Task<string> GetObjectSas(string objectPath, StorageUrlOptions options) {
		if (string.IsNullOrWhiteSpace(objectPath)) throw new ArgumentNullException(nameof(objectPath));
		if (options == null) throw new ArgumentNullException(nameof(options));

		return await GetPresignedUrl(
			objectPath,
			options.Permissions.HasFlag(StorageUrlPermissions.Read),
			options.RequireHttps,
			(int)options.ExpiresIn.TotalSeconds).ConfigureAwait(false);
	}



	public override async Task<bool> MoveObject(string oldPath, string newPath, bool overwrite,
		CancellationToken cancellationToken = default) {
		if (string.IsNullOrWhiteSpace(oldPath)) throw new ArgumentNullException(nameof(oldPath));
		if (string.IsNullOrWhiteSpace(newPath)) throw new ArgumentNullException(nameof(newPath));

		var client = await Client(cancellationToken).ConfigureAwait(false);

		if (!overwrite && await ObjectExists(newPath, cancellationToken).ConfigureAwait(false))
			return false;

		string sourceKey = StoragePath.Normalize(oldPath);
		string destKey = StoragePath.Normalize(newPath);

		var copySource = new CopySourceObjectArgs()
			.WithBucket(_bucketName)
			.WithObject(sourceKey);

		var copyArgs = new CopyObjectArgs()
			.WithBucket(_bucketName)
			.WithObject(destKey)
			.WithCopyObjectSource(copySource);

		await client.CopyObjectAsync(copyArgs, cancellationToken).ConfigureAwait(false);

		var removeArgs = new RemoveObjectArgs()
			.WithBucket(_bucketName)
			.WithObject(sourceKey);

		await client.RemoveObjectAsync(removeArgs, cancellationToken).ConfigureAwait(false);

		return true;
	}


	/// <summary>
	/// Returns all tags associated with the specified object.
	/// Returns an empty collection if no tags exist.
	/// </summary>
	public override async Task<Dictionary<string, string>> GetObjectTags(string objectPath, CancellationToken cancellationToken = default) {
		if (string.IsNullOrWhiteSpace(objectPath))throw new ArgumentNullException(nameof(objectPath));

		var client = await Client(cancellationToken).ConfigureAwait(false);

		string key = StoragePath.Normalize(objectPath);

		try {
			var response = await client.GetObjectTagsAsync(new GetObjectTagsArgs()
				.WithBucket(_bucketName)
				.WithObject(key), cancellationToken).ConfigureAwait(false);

			return response.Tags?
				       .ToDictionary(x => x.Key, x => x.Value)
			       ?? new Dictionary<string, string>();
		}
		catch (ObjectNotFoundException) {
			// Returns null if the object cannot be found.
			return null;
		}
	}


	/// <summary>
	/// Replaces all tags associated with the specified object.
	/// Existing tags are removed before the new tags are applied.
	/// </summary>
	public override async Task<bool> SetObjectTags(string objectPath, Dictionary<string, string> tags, CancellationToken cancellationToken = default) {
		if (string.IsNullOrWhiteSpace(objectPath))throw new ArgumentNullException(nameof(objectPath));

		var client = await Client(cancellationToken).ConfigureAwait(false);

		string key = StoragePath.Normalize(objectPath);

		try {
			var tagData = new Tagging(tags ?? new Dictionary<string, string>(), true);

			await client.SetObjectTagsAsync(new SetObjectTagsArgs()
				.WithBucket(_bucketName)
				.WithObject(key)
				.WithTagging(tagData), cancellationToken).ConfigureAwait(false);

			return true;
		}
		catch (ObjectNotFoundException) {
			// Returns true if succeeded, or false if the object cannot be found.
			return false;
		}
	}


	/// <summary>
	/// Removes all tags from the specified object.
	/// Does nothing if the object has no tags.
	/// </summary>
	public override async Task<bool> DeleteObjectTags(string objectPath, CancellationToken cancellationToken = default) {
		if (string.IsNullOrWhiteSpace(objectPath))throw new ArgumentNullException(nameof(objectPath));

		var client = await Client(cancellationToken).ConfigureAwait(false);

		string key = StoragePath.Normalize(objectPath);

		try {
			await client.RemoveObjectTagsAsync(new RemoveObjectTagsArgs()
				.WithBucket(_bucketName)
				.WithObject(key), cancellationToken).ConfigureAwait(false);

			return true;
		}
		catch (ObjectNotFoundException) {
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
		if (string.IsNullOrWhiteSpace(objectPath))throw new ArgumentNullException(nameof(objectPath));

		var client = await Client(cancellationToken).ConfigureAwait(false);
		string key = StoragePath.Normalize(objectPath);

		try {
			ObjectStat response = await client.StatObjectAsync(new StatObjectArgs()
				.WithBucket(_bucketName)
				.WithObject(key), cancellationToken).ConfigureAwait(false);

			if (!response.MetaData.TryGetValue("x-amz-storage-class", out string storageClass))
				return StorageTier.Standard;

			return MinioTier.ToFluentTier.TryGetValue(storageClass, out StorageTier tier)
				? tier
				: StorageTier.Unknown;
		}
		catch (ObjectNotFoundException) {
			// Returns NotFound if the object cannot be found.
			return StorageTier.NotFound;
		}
	}


	/// <summary>
	/// Changes the storage tier or storage class of the specified object.
	/// </summary>
	public override async Task<bool> SetObjectTier(string objectPath, StorageTier tier, CancellationToken cancellationToken = default) {
		if (string.IsNullOrWhiteSpace(objectPath))throw new ArgumentNullException(nameof(objectPath));

		if (!MinioTier.FromFluentTier.TryGetValue(tier, out string storageClass))
			throw new StorageException($"MinIO does not support the tier \"{tier}\". Use a supported tier and try again.");

		var client = await Client(cancellationToken).ConfigureAwait(false);
		string key = StoragePath.Normalize(objectPath);

		try {
			await client.CopyObjectAsync(new CopyObjectArgs()
				.WithBucket(_bucketName)
				.WithObject(key)
				.WithCopyObjectSource(new CopySourceObjectArgs()
					.WithBucket(_bucketName)
					.WithObject(key))
				.WithReplaceMetadataDirective(true)
				.WithHeaders(new Dictionary<string, string> {
					["x-amz-storage-class"] = storageClass
				}), cancellationToken).ConfigureAwait(false);

			return true;
		}
		catch (ObjectNotFoundException) {
			// Returns true if succeeded, or false if the object cannot be found.
			return false;
		}
	}

	/// <summary>
	/// Returns true if bucket lifecycle tiering is configured.
	/// </summary>
	public override async Task<bool> IsTiered() {
		var client = await Client().ConfigureAwait(false);
		try {
			LifecycleConfiguration config = await client.GetBucketLifecycleAsync(new GetBucketLifecycleArgs()
				.WithBucket(_bucketName)).ConfigureAwait(false);

			return config.Rules.Any(x => x.Status == LifecycleRule.LifecycleRuleStatusEnabled && x.TransitionObject != null);
		}
		catch (MinioException) {
		}
		return false;
	}

	/// <summary>
	/// Fastest possible implemention to check if a virtual directory exists in a MinIO store.
	/// </summary>
	public override async Task<bool> DirectoryExists(string fullPath, CancellationToken cancellationToken = default) {
		if (fullPath == null) throw new ArgumentNullException(nameof(fullPath));
		fullPath = StoragePath.Normalize(fullPath);

		// do not check root directory
		if (fullPath.Length == 0) return true;

		IMinioClient client = await Client().ConfigureAwait(false);

		if (!fullPath.EndsWith("/"))
			fullPath += "/";

		await foreach (Item _ in client.ListObjectsEnumAsync(
			               new ListObjectsArgs().WithBucket(_bucketName).WithPrefix(fullPath),cancellationToken).ConfigureAwait(false)) {
			return true;
		}

		return false;
	}

}
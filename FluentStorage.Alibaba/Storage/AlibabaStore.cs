using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Aliyun.OSS;
using Aliyun.OSS.Common;
using FluentStorage.Enums;
using FluentStorage.Exceptions;
using FluentStorage.Model;
using FluentStorage.Storage;
using FluentStorage.Streaming;
using MimeMapping;

namespace FluentStorage.Alibaba.Storage;

/// <summary>
/// Manages a single Alibaba Object Storage Service bucket using the native Aliyun OSS SDK.
/// </summary>
public class AlibabaStore : StoreBase {
	private readonly string _bucketName;
	private readonly OssClient _client;

	private volatile bool _bucketChecked;
	private readonly SemaphoreSlim _bucketCheckLock = new SemaphoreSlim(1, 1);

	public AlibabaStore(OssClient client, string bucketName) {
		if (client == null) throw new ArgumentNullException(nameof(client));
		if (string.IsNullOrWhiteSpace(bucketName)) throw new ArgumentNullException(nameof(bucketName));

		_bucketName = bucketName;
		_client = client;
	}

	public AlibabaStore(string endpoint, string bucketName, string accessKeyId, string accessKeySecret) {
		if (string.IsNullOrWhiteSpace(endpoint)) throw new ArgumentNullException(nameof(endpoint));
		if (string.IsNullOrWhiteSpace(bucketName)) throw new ArgumentNullException(nameof(bucketName));
		if (string.IsNullOrWhiteSpace(accessKeyId)) throw new ArgumentNullException(nameof(accessKeyId));
		if (string.IsNullOrWhiteSpace(accessKeySecret)) throw new ArgumentNullException(nameof(accessKeySecret));

		_bucketName = bucketName;
		_client = new OssClient(endpoint, accessKeyId, accessKeySecret);
	}

	public AlibabaStore(string endpoint, string bucketName, string accessKeyId, string accessKeySecret, string securityToken) {
		if (string.IsNullOrWhiteSpace(endpoint)) throw new ArgumentNullException(nameof(endpoint));
		if (string.IsNullOrWhiteSpace(bucketName)) throw new ArgumentNullException(nameof(bucketName));
		if (string.IsNullOrWhiteSpace(accessKeyId)) throw new ArgumentNullException(nameof(accessKeyId));
		if (string.IsNullOrWhiteSpace(accessKeySecret)) throw new ArgumentNullException(nameof(accessKeySecret));
		if (string.IsNullOrWhiteSpace(securityToken)) throw new ArgumentNullException(nameof(securityToken));

		_bucketName = bucketName;
		_client = new OssClient(endpoint, accessKeyId, accessKeySecret, securityToken);
	}

	public AlibabaStore(string endpoint, string bucketName, string accessKeyId, string accessKeySecret, ClientConfiguration configuration) {
		if (string.IsNullOrWhiteSpace(endpoint)) throw new ArgumentNullException(nameof(endpoint));
		if (string.IsNullOrWhiteSpace(bucketName)) throw new ArgumentNullException(nameof(bucketName));
		if (string.IsNullOrWhiteSpace(accessKeyId)) throw new ArgumentNullException(nameof(accessKeyId));
		if (string.IsNullOrWhiteSpace(accessKeySecret)) throw new ArgumentNullException(nameof(accessKeySecret));
		if (configuration == null) throw new ArgumentNullException(nameof(configuration));

		_bucketName = bucketName;
		_client = new OssClient(endpoint, accessKeyId, accessKeySecret, configuration);
	}

	// ------------------------------------------------------------------
	// Internal client accessor - validates bucket existence on first use
	// ------------------------------------------------------------------

	private async Task<OssClient> Client() {
		if (!_bucketChecked) {
			await _bucketCheckLock.WaitAsync().ConfigureAwait(false);
			try {
				if (!_bucketChecked) {
					var exists = await Task.Run(() => _client.DoesBucketExist(_bucketName)).ConfigureAwait(false);
					if (!exists)
						throw new StorageException($"Bucket '{_bucketName}' does not exist!");

					_bucketChecked = true;
				}
			}
			finally {
				_bucketCheckLock.Release();
			}
		}

		return _client;
	}

	public override async Task<object> GetClient() {
		return await Client().ConfigureAwait(false);
	}

	// ------------------------------------------------------------------
	// Write
	// ------------------------------------------------------------------

	public override async Task SetObject(string fullPath, Stream dataStream, bool append = false, CancellationToken cancellationToken = default) {
		await SetObject(fullPath, dataStream, null, append, cancellationToken).ConfigureAwait(false);
	}

	public override async Task SetObject(string fullPath, Stream dataStream, string contentType, bool append = false, CancellationToken cancellationToken = default) {
		if (string.IsNullOrWhiteSpace(fullPath)) throw new ArgumentNullException(nameof(fullPath));
		if (dataStream == null) throw new ArgumentNullException(nameof(dataStream));

		var client = await Client().ConfigureAwait(false);
		var key = StoragePath.Normalize(fullPath);

		if (string.IsNullOrWhiteSpace(contentType))
			contentType = MimeUtility.GetMimeMapping(fullPath);

		await Task.Run(() => {
			if (append) {
				long position = 0;

				// Useless/pointless error: probing whether the object already exists to determine
				// the append offset. A "not found" here just means we're starting a new object at 0.
				try {
					var meta = client.GetObjectMetadata(_bucketName, key);
					position = meta.ContentLength;
				}
				catch (OssException ex) when (string.Equals(ex.ErrorCode, "NoSuchKey", StringComparison.OrdinalIgnoreCase)) {
					position = 0;
				}

				var appendRequest = new AppendObjectRequest(_bucketName, key) {
					Content = dataStream,
					Position = position,
					ObjectMetadata = new ObjectMetadata { ContentType = contentType }
				};

				client.AppendObject(appendRequest);
			}
			else {
				var metadata = new ObjectMetadata { ContentType = contentType };
				var request = new PutObjectRequest(_bucketName, key, dataStream, metadata);
				client.PutObject(request);
			}
		}, cancellationToken).ConfigureAwait(false);
	}

	// ------------------------------------------------------------------
	// Read
	// ------------------------------------------------------------------

	public override async Task<Stream> OpenRead(string fullPath, CancellationToken cancellationToken = default) {
		if (string.IsNullOrWhiteSpace(fullPath)) throw new ArgumentNullException(nameof(fullPath));

		var client = await Client().ConfigureAwait(false);
		var key = StoragePath.Normalize(fullPath);

		var ossObject = await Task.Run(() => client.GetObject(_bucketName, key), cancellationToken).ConfigureAwait(false);
		return ossObject.Content;
	}

	public override async Task<Stream> OpenWrite(string fullPath, bool overwrite, CancellationToken cancellationToken = default) {
		if (string.IsNullOrWhiteSpace(fullPath)) throw new ArgumentNullException(nameof(fullPath));

		var client = await Client().ConfigureAwait(false);
		var key = StoragePath.Normalize(fullPath);

		if (!overwrite) {
			var exists = await Task.Run(() => client.DoesObjectExist(_bucketName, key), cancellationToken).ConfigureAwait(false);
			if (exists)
				return null;
		}

		var buffer = new MemoryStream();

		return new FixedStream(buffer, null, async s => {

			// write to cloud on stream dispose
			s.Position = 0;

			var contentType = MimeUtility.GetMimeMapping(fullPath);
			var metadata = new ObjectMetadata { ContentType = contentType };
			var request = new PutObjectRequest(_bucketName, key, s, metadata);

			await Task.Run(() => client.PutObject(request)).ConfigureAwait(false);
		});
	}

	public override async Task<Stream> OpenRange(string fullPath,long offset,long length,CancellationToken cancellationToken = default) {
		if (string.IsNullOrWhiteSpace(fullPath)) throw new ArgumentNullException(nameof(fullPath));

		var client = await Client().ConfigureAwait(false);
		var key = StoragePath.Normalize(fullPath);

		var request = new GetObjectRequest(_bucketName, key);

		request.SetRange(offset,offset + length - 1);

		var stream = new MemoryStream();

		await Task.Run(() => {
			client.GetObject(request, stream);
		}, cancellationToken).ConfigureAwait(false);

		stream.Position = 0;
		return stream;
	}

	public override async Task<bool> IsSeekable() {
		return true;
	}

	public override async Task<long> GetObjectLength(string fullPath, long defaultValue = -1, CancellationToken cancellationToken = default) {
		try {
			if (string.IsNullOrWhiteSpace(fullPath))
				return defaultValue;

			var client = await Client().ConfigureAwait(false);
			var key = StoragePath.Normalize(fullPath);

			ObjectMetadata metadata = client.GetObjectMetadata(_bucketName, key);

			return metadata != null ? metadata.ContentLength : defaultValue;
		}
		catch {
			return defaultValue;
		}
	}

	// ------------------------------------------------------------------
	// Delete
	// ------------------------------------------------------------------

	public override async Task DeleteObjects(IEnumerable<string> fullPaths, CancellationToken cancellationToken = default) {
		if (fullPaths == null) throw new ArgumentNullException(nameof(fullPaths));

		var keys = fullPaths.Select(StoragePath.Normalize).ToList();
		if (keys.Count == 0)
			return;

		var client = await Client().ConfigureAwait(false);

		await Task.Run(() => {
			// OSS DeleteObjects supports at most 1000 keys per request.
			foreach (var batch in Batch(keys, 1000)) {
				var request = new DeleteObjectsRequest(_bucketName, batch, false);
				client.DeleteObjects(request);
			}
		}, cancellationToken).ConfigureAwait(false);
	}

	public override async Task DeleteObject(string fullPath, CancellationToken cancellationToken = default) {
		if (string.IsNullOrWhiteSpace(fullPath)) throw new ArgumentNullException(nameof(fullPath));

		var client = await Client().ConfigureAwait(false);
		var key = StoragePath.Normalize(fullPath);

		await Task.Run(() => client.DeleteObject(_bucketName, key), cancellationToken).ConfigureAwait(false);
	}

	public override async Task DeleteDirectory(string folderPath, bool recursive, CancellationToken cancellationToken = default) {
		if (string.IsNullOrWhiteSpace(folderPath)) throw new ArgumentNullException(nameof(folderPath));

		var items = await ListDirectory(folderPath, recursive, cancellationToken).ConfigureAwait(false);
		if (items == null || items.Count == 0)
			return;

		var filePaths = items
			.Where(i => i.Type == StorageObjectType.File)
			.Select(i => CombinePath(i.FolderPath, i.Name))
			.ToList();

		if (filePaths.Count > 0)
			await DeleteObjects(filePaths, cancellationToken).ConfigureAwait(false);
	}

	// ------------------------------------------------------------------
	// Existence checks
	// ------------------------------------------------------------------

	public override async Task<List<bool>> ObjectsExists(IEnumerable<string> fullPaths, CancellationToken cancellationToken = default) {
		if (fullPaths == null) throw new ArgumentNullException(nameof(fullPaths));

		var client = await Client().ConfigureAwait(false);
		var results = new List<bool>();

		foreach (var path in fullPaths) {
			var key = StoragePath.Normalize(path);
			var exists = await Task.Run(() => client.DoesObjectExist(_bucketName, key), cancellationToken).ConfigureAwait(false);
			results.Add(exists);
		}

		return results;
	}

	public override async Task<bool> ObjectExists(string fullPath, CancellationToken cancellationToken = default) {
		if (string.IsNullOrWhiteSpace(fullPath)) throw new ArgumentNullException(nameof(fullPath));

		var client = await Client().ConfigureAwait(false);
		var key = StoragePath.Normalize(fullPath);

		return await Task.Run(() => client.DoesObjectExist(_bucketName, key), cancellationToken).ConfigureAwait(false);
	}

	// ------------------------------------------------------------------
	// Metadata / info
	// ------------------------------------------------------------------

	public override async Task<List<StoreObject>> GetObjectsInfo(IEnumerable<string> fullPaths, CancellationToken cancellationToken = default) {
		if (fullPaths == null) throw new ArgumentNullException(nameof(fullPaths));

		var results = new List<StoreObject>();

		foreach (var path in fullPaths) {
			var info = await GetObjectInfo(path, cancellationToken).ConfigureAwait(false);
			if (info != null)
				results.Add(info);
		}

		return results;
	}

	public override async Task<StoreObject> GetObjectInfo(string fullPath, CancellationToken cancellationToken = default) {
		if (string.IsNullOrWhiteSpace(fullPath)) throw new ArgumentNullException(nameof(fullPath));

		var client = await Client().ConfigureAwait(false);
		var key = StoragePath.Normalize(fullPath);

		ObjectMetadata meta;
		try {
			meta = await Task.Run(() => client.GetObjectMetadata(_bucketName, key), cancellationToken).ConfigureAwait(false);
		}
		catch (OssException ex) when (string.Equals(ex.ErrorCode, "NoSuchKey", StringComparison.OrdinalIgnoreCase)) {
			return null;
		}

		var obj = new StoreObject(GetFolderPath(key), GetName(key), StorageObjectType.File) {
			Size = meta.ContentLength,
			MD5 = meta.ETag?.Trim('"'),
			DateModified = meta.LastModified == default
				? (DateTimeOffset?)null
				: new DateTimeOffset(meta.LastModified.ToUniversalTime())
		};

		obj.TryAddProperties(
			"ContentType", meta.ContentType,
			"ETag", meta.ETag
			//"StorageClass", meta.StorageClass.ToString()
		);

		if (meta.UserMetadata != null) {
			foreach (var kv in meta.UserMetadata)
				obj.Metadata[kv.Key] = kv.Value;
		}

		return obj;
	}

	public override async Task SetObjectInfo(StoreObject obj, CancellationToken cancellationToken = default) {
		if (obj == null) throw new ArgumentNullException(nameof(obj));

		var client = await Client().ConfigureAwait(false);
		var key = StoragePath.Normalize(CombinePath(obj.FolderPath, obj.Name));

		var metadata = new ObjectMetadata();
		foreach (var kv in obj.Metadata)
			metadata.UserMetadata[kv.Key] = kv.Value;

		await Task.Run(() => {
			// OSS has no in-place metadata update API; the standard approach is a same-key copy
			// with MetadataDirective set to replace.
			var request = new CopyObjectRequest(_bucketName, key, _bucketName, key) {
				NewObjectMetadata = metadata
			};

			client.CopyObject(request);
		}, cancellationToken).ConfigureAwait(false);
	}

	public override async Task SetObjectsInfo(IEnumerable<StoreObject> objs, CancellationToken cancellationToken = default) {
		if (objs == null) throw new ArgumentNullException(nameof(objs));

		foreach (var obj in objs)
			await SetObjectInfo(obj, cancellationToken).ConfigureAwait(false);
	}

	// ------------------------------------------------------------------
	// Presigned URLs
	// ------------------------------------------------------------------

	public override async Task<string> GetPresignedUrl(string fullPath, bool forDownload, bool https, int expiresInSeconds = 86000) {
		if (string.IsNullOrWhiteSpace(fullPath)) throw new ArgumentNullException(nameof(fullPath));

		var client = await Client().ConfigureAwait(false);
		var key = StoragePath.Normalize(fullPath);

		var request = new GeneratePresignedUriRequest(_bucketName, key, forDownload ? SignHttpMethod.Get : SignHttpMethod.Put) {
			Expiration = DateTime.Now.AddSeconds(expiresInSeconds)
		};

		if (forDownload) {
			// Ensures a sensible Content-Type is presented to clients that honor the response header
			// override, computing it if the caller has not already supplied/stored one.
			var contentType = MimeUtility.GetMimeMapping(fullPath);
			if (request.ResponseHeaders == null)
				request.ResponseHeaders = new ResponseHeaderOverrides();
			request.ResponseHeaders.ContentType = contentType;
		}

		var uri = await Task.Run(() => client.GeneratePresignedUri(request)).ConfigureAwait(false);
		var url = uri.ToString();

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
			(int)options.ExpiresIn.TotalSeconds
		).ConfigureAwait(false);
	}

	// ------------------------------------------------------------------
	// Move
	// ------------------------------------------------------------------

	public override async Task<bool> MoveObject(string oldPath, string newPath, bool overwrite, CancellationToken cancellationToken = default) {
		if (string.IsNullOrWhiteSpace(oldPath)) throw new ArgumentNullException(nameof(oldPath));
		if (string.IsNullOrWhiteSpace(newPath)) throw new ArgumentNullException(nameof(newPath));

		var client = await Client().ConfigureAwait(false);
		var oldKey = StoragePath.Normalize(oldPath);
		var newKey = StoragePath.Normalize(newPath);

		if (!overwrite) {
			var exists = await Task.Run(() => client.DoesObjectExist(_bucketName, newKey), cancellationToken).ConfigureAwait(false);
			if (exists)
				return false;
		}

		var copy = new CopyObjectRequest(_bucketName, oldKey, _bucketName, newKey);
		await Task.Run(() => {
			client.CopyObject(copy);
			client.DeleteObject(_bucketName, oldKey);
		}, cancellationToken).ConfigureAwait(false);

		return true;
	}

	// ------------------------------------------------------------------
	// Listing
	// ------------------------------------------------------------------

	public override async Task<List<StoreObject>> ListObjects(StorageListOptions options = null, CancellationToken cancellationToken = default) {
		options ??= new StorageListOptions();

		var client = await Client().ConfigureAwait(false);
		var results = new List<StoreObject>();

		var rootPrefix = StoragePath.Normalize(options.FolderPath ?? string.Empty);
		if (rootPrefix.Length > 0 && !rootPrefix.EndsWith("/"))
			rootPrefix += "/";

		var filePrefix = options.FilePrefix ?? string.Empty;
		var pageSize = options.PageSize ?? 1000;

		await Task.Run(() => {
			if (options.Recurse && options.RecursionMode == StorageRecursion.Remote) {
				// Remote recursion: a single flat listing (no delimiter) filtered by the combined prefix.
				ListFlat(client, rootPrefix, filePrefix, pageSize, options, results, cancellationToken);
			}
			else {
				// Local/manual recursion (or no recursion at all): walk one virtual folder level
				// at a time using the "/" delimiter, recursing into CommonPrefixes when requested.
				ListFolderLevel(client, rootPrefix, filePrefix, pageSize, options, results, options.Recurse, cancellationToken);
			}
		}, cancellationToken).ConfigureAwait(false);

		return results;
	}

	private void ListFlat(OssClient client,string rootPrefix,string filePrefix,int pageSize,
		StorageListOptions options,List<StoreObject> results,CancellationToken cancellationToken) {
		string marker = null;

		do {
			cancellationToken.ThrowIfCancellationRequested();

			var request = new ListObjectsRequest(_bucketName) {
				Prefix = rootPrefix + filePrefix,
				Marker = marker,
				MaxKeys = pageSize
			};

			var listing = client.ListObjects(request);

			foreach (var summary in listing.ObjectSummaries) {
				if (IsFolderMarker(summary.Key, summary.Size))
					continue;

				AddFileObject(summary, options, results);

				if (options.MaxResults.HasValue && results.Count >= options.MaxResults.Value)
					return;
			}

			marker = listing.NextMarker;
		}
		while (!string.IsNullOrEmpty(marker));
	}

	private void ListFolderLevel(OssClient client,string currentPrefix,string filePrefix,
		int pageSize,StorageListOptions options,List<StoreObject> results,bool recurse,
		CancellationToken cancellationToken) {
		string marker = null;

		do {
			cancellationToken.ThrowIfCancellationRequested();

			var request = new ListObjectsRequest(_bucketName) {
				Prefix = currentPrefix + filePrefix,
				Marker = marker,
				MaxKeys = pageSize,
				Delimiter = "/"
			};

			var listing = client.ListObjects(request);

			foreach (var summary in listing.ObjectSummaries) {
				if (IsFolderMarker(summary.Key, summary.Size))
					continue;

				AddFileObject(summary, options, results);

				if (options.MaxResults.HasValue && results.Count >= options.MaxResults.Value)
					return;
			}

			foreach (var commonPrefix in listing.CommonPrefixes) {
				var folderKey = commonPrefix.TrimEnd('/');

				var folderObj = new StoreObject(GetFolderPath(folderKey), GetName(folderKey), StorageObjectType.Folder);
				results.Add(folderObj);

				if (options.MaxResults.HasValue && results.Count >= options.MaxResults.Value)
					return;

				if (recurse) {
					ListFolderLevel(client, commonPrefix, filePrefix, pageSize, options, results, true, cancellationToken);

					if (options.MaxResults.HasValue && results.Count >= options.MaxResults.Value)
						return;
				}
			}

			marker = listing.NextMarker;
		}
		while (!string.IsNullOrEmpty(marker));
	}

	private static bool IsFolderMarker(string key, long size) {
		return key.EndsWith("/") && size == 0;
	}

	private static void AddFileObject(OssObjectSummary summary, StorageListOptions options, List<StoreObject> results) {
		var obj = new StoreObject(GetFolderPath(summary.Key), GetName(summary.Key), StorageObjectType.File) {
			Size = summary.Size,
			MD5 = summary.ETag?.Trim('"'),
			DateModified = summary.LastModified == default
				? (DateTimeOffset?)null
				: new DateTimeOffset(summary.LastModified.ToUniversalTime())
		};

		if (options.IncludeAttributes) {
			obj.TryAddProperties(
				"ETag", summary.ETag,
				"StorageClass", summary.StorageClass.ToString(),
				"Owner", summary.Owner?.DisplayName
			);
		}

		results.Add(obj);
	}

	// ------------------------------------------------------------------
	// Helpers
	// ------------------------------------------------------------------

	private static string GetFolderPath(string key) {
		var idx = key.LastIndexOf('/');
		return idx >= 0 ? key.Substring(0, idx + 1) : string.Empty;
	}

	private static string GetName(string key) {
		var idx = key.LastIndexOf('/');
		return idx >= 0 ? key.Substring(idx + 1) : key;
	}

	private static string CombinePath(string folderPath, string name) {
		folderPath ??= string.Empty;
		if (folderPath.Length > 0 && !folderPath.EndsWith("/"))
			folderPath += "/";

		return folderPath + name;
	}

	private static IEnumerable<List<T>> Batch<T>(List<T> source, int size) {
		for (var i = 0; i < source.Count; i += size)
			yield return source.GetRange(i, Math.Min(size, source.Count - i));
	}

	// ------------------------------------------------------------------
	// Tagging
	// ------------------------------------------------------------------

	/// <summary>
	/// Returns all tags associated with the specified object.
	/// Returns an empty collection if no tags exist.
	/// </summary>
	public override async Task<Dictionary<string, string>> GetObjectTags(string objectPath, CancellationToken cancellationToken = default) {
		if (string.IsNullOrWhiteSpace(objectPath))
			throw new ArgumentNullException(nameof(objectPath));

		var client = await Client().ConfigureAwait(false);

		string key = StoragePath.Normalize(objectPath);

		try {
			var result = client.GetObjectTagging(_bucketName, key);

			return result.Tags?
				       .ToDictionary(x => x.Key, x => x.Value)
			       ?? new Dictionary<string, string>();
		}
		catch (OssException ex) when (ex.ErrorCode == "NoSuchKey") {
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

		var client = await Client().ConfigureAwait(false);

		string key = StoragePath.Normalize(objectPath);

		try {
			var tagData = tags?.Select(x => new Tag {Key = x.Key,Value = x.Value}).ToList() ?? new List<Tag>();

			client.SetObjectTagging(new SetObjectTaggingRequest(_bucketName, key, tagData));

			return true;
		}
		catch (OssException ex) when (ex.ErrorCode == "NoSuchKey") {
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

		var client = await Client().ConfigureAwait(false);

		string key = StoragePath.Normalize(objectPath);

		try {
			client.DeleteObjectTagging(_bucketName, key);

			return true;
		}
		catch (OssException ex) when (ex.ErrorCode == "NoSuchKey") {
			// Returns true if succeeded, or false if the object cannot be found.
			return false;
		}
	}

	public override async Task<bool> IsTagged() {
		return true;
	}

	/// <summary>
	/// Fastest possible implemention to check if a virtual directory exists in a Alibaba OSS store.
	/// Perform a `ListObjects` with the directory prefix and `MaxKeys = 1`.
	/// </summary>
	public override async Task<bool> DirectoryExists(string fullPath, CancellationToken cancellationToken = default) {
		if (fullPath == null) throw new ArgumentNullException(nameof(fullPath));
		fullPath = StoragePath.Normalize(fullPath);

		// do not check root directory
		if (fullPath.Length == 0) return true;

		OssClient client = await Client().ConfigureAwait(false);

		if (!fullPath.EndsWith("/"))
			fullPath += "/";

		ObjectListing result = client.ListObjects(new ListObjectsRequest(_bucketName) {
			Prefix = fullPath, MaxKeys = 1
		});

		return result.ObjectSummaries.Count() > 0;
	}

}
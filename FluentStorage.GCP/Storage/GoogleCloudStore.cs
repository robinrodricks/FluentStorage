using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using FluentStorage.Enums;
using FluentStorage.Exceptions;
using FluentStorage.GCP.Utils;
using FluentStorage.Model;
using FluentStorage.Storage;
using FluentStorage.Streaming;
using FluentStorage.Utils.Extensions;
using FluentStorage.Utils.Hashing;
using FluentStorage.Utils.Validation;
using Google;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Storage.v1;
using Google.Apis.Storage.v1.Data;
using Google.Cloud.Storage.V1;
using GObject = Google.Apis.Storage.v1.Data.Object;
using GObjects = Google.Apis.Storage.v1.Data.Objects;

namespace FluentStorage.GCP.Storage;

/// <summary>
/// Manages a single Google Cloud Storage bucket.
/// </summary>
public class GoogleCloudStore : StoreBase {

	private readonly StorageClient _client;
	private readonly UrlSigner _urlSigner;
	private readonly string _bucketName;

	public GoogleCloudStore(string bucketName, GoogleCredential credential = null, EncryptionKey encryptionKey = null) : base() {
		_client = StorageClient.Create(credential, encryptionKey);
		_urlSigner = UrlSigner.FromCredential(credential);
		_bucketName = bucketName;
	}

	/// <summary>
	/// Returns the StorageClient instance for this store.
	/// </summary>
	public override async Task<object> GetClient() {
		return _client;
	}

	protected override async Task<List<StoreObject>> ListPath(
		string path, StorageListOptions options, CancellationToken cancellationToken = default) {

		ObjectsResource.ListRequest request = _client.Service.Objects.List(_bucketName);
		request.Prefix = StoragePath.IsRootPath(path) ? null : (StoragePath.Normalize(path) + "/");
		request.Delimiter = "/";
		request.MaxResults = options.PageSize ?? StorageListOptions.PAGE_SIZE;
			
		var page = new List<StoreObject>();
		do {
			GObjects serviceObjects = await request.ExecuteAsync(cancellationToken: cancellationToken).ConfigureAwait(false);

			if (serviceObjects.Items != null) {
				page.AddRange(GConvert.ToBlobs(serviceObjects.Items, options));
			}

			if (serviceObjects.Prefixes != null) {
				//the only info we have about prefixes is it's name
				page.AddRange(serviceObjects.Prefixes.Select(p => new StoreObject(p, StorageObjectType.Folder)));
			}


			request.PageToken = serviceObjects.NextPageToken;
		}
		while (request.PageToken != null);

		return page;
	}

	public override async Task SetObjectsInfo(IEnumerable<StoreObject> blobs, CancellationToken cancellationToken = default) {
		ArgValidator.AssertFullPaths(blobs);

		await Task.WhenAll(blobs.Select(b => SetObjectInfo(b, cancellationToken))).ConfigureAwait(false);
	}

	public override async Task SetObjectInfo(StoreObject blob, CancellationToken cancellationToken = default) {
		GObject item = await _client.GetObjectAsync(_bucketName, StoragePath.Normalize(blob.FullPath), cancellationToken: cancellationToken).ConfigureAwait(false);

		if (item.Metadata == null) {
			item.Metadata = new Dictionary<string, string>();
		}

		foreach (KeyValuePair<string, string> metadata in blob.Metadata) {
			if (item.Metadata.ContainsKey(metadata.Key)) {
				item.Metadata[metadata.Key] = metadata.Value;
			}
			else {
				item.Metadata.Add(metadata.Key, metadata.Value);
			}
		}

		await _client.UpdateObjectAsync(item, cancellationToken: cancellationToken).ConfigureAwait(false);
	}

	public override async Task<StoreObject> GetObjectInfo(string fullPath, CancellationToken cancellationToken = default) {
		fullPath = StoragePath.Normalize(fullPath);

		try {
			GObject obj = await _client.GetObjectAsync(_bucketName, fullPath,
				new GetObjectOptions {
					//todo
				},
				cancellationToken).ConfigureAwait(false);

			return GConvert.ToBlob(obj);
		}
		catch (GoogleApiException ex) when (ex.HttpStatusCode == HttpStatusCode.NotFound) {
			return null;
		}
	}

	public override async Task DeleteObject(string fullPath, CancellationToken cancellationToken = default) {
		try {
			await _client.DeleteObjectAsync(_bucketName, StoragePath.Normalize(fullPath), cancellationToken: cancellationToken).ConfigureAwait(false);
		}
		catch (GoogleApiException ex) when (ex.HttpStatusCode == HttpStatusCode.NotFound) {
			//when not found, just ignore
			/*
			//try delete everything recursively
			List<StoreObject?> childObjects = await ListPath(fullPath, new StorageListOptions { Recurse = true }, cancellationToken).ConfigureAwait(false);

			foreach (StoreObject? blob in childObjects) {
				if (blob == null) {
					continue;
				}

				try {
					await _client.DeleteObjectAsync(_bucketName, StoragePath.Normalize(blob.FullPath), cancellationToken: cancellationToken).ConfigureAwait(false);
				}
				catch (GoogleApiException exc) when (exc.HttpStatusCode == HttpStatusCode.NotFound) {

				}
			}*/
		}
	}

	public override async Task<bool> ObjectExists(string fullPath, CancellationToken cancellationToken = default) {
		if (fullPath == null) throw new ArgumentNullException(nameof(fullPath));

		try {
			await _client.GetObjectAsync(
				_bucketName, StoragePath.Normalize(fullPath),
				null,
				cancellationToken).ConfigureAwait(false);

			return true;
		}
		catch (GoogleApiException ex) when (ex.HttpStatusCode == HttpStatusCode.NotFound) {
			return false;
		}
	}


	public override async Task SetObject(string fullPath, Stream dataStream,
		bool append = false, CancellationToken cancellationToken = default) {
		if (append)
			throw new NotSupportedException();
		if (fullPath == null) throw new ArgumentNullException(nameof(fullPath));
		fullPath = StoragePath.Normalize(fullPath);

		await _client.UploadObjectAsync(_bucketName, fullPath, null, dataStream, cancellationToken: cancellationToken).ConfigureAwait(false);
	}

	/// <summary>
	/// Opens an object for reading and returns its content stream.
	/// </summary>
	public override async Task<Stream> OpenRead(string fullPath, CancellationToken cancellationToken = default) {
		if (fullPath == null) throw new ArgumentNullException(nameof(fullPath));
		fullPath = StoragePath.Normalize(fullPath);

		// no read streaming support in this crappy SDK

		try {
			var ms = new MemoryStream();
			await _client.DownloadObjectAsync(_bucketName, fullPath, ms, cancellationToken: cancellationToken).ConfigureAwait(false);
			ms.Position = 0;
			return ms;
		}
		catch (GoogleApiException ex) when (ex.HttpStatusCode == HttpStatusCode.NotFound) {
			return null;
		}
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

			await _client.UploadObjectAsync(_bucketName,fullPath,
				null,s, null, cancellationToken).ConfigureAwait(false);
		});
	}

	/// <summary>
	/// Opens a readable stream beginning at the specified byte offset.
	/// </summary>
	public override async Task<Stream> OpenRange(string path,long offset,long length,CancellationToken cancellationToken = default) {
		if (path == null) throw new ArgumentNullException(nameof(path));

		path = StoragePath.Normalize(path);

		var stream = new MemoryStream();
		try {

			var options = new DownloadObjectOptions {
				Range = new RangeHeaderValue(offset, offset + length - 1)
			};

			await _client.DownloadObjectAsync(_bucketName, path, stream, options, cancellationToken).ConfigureAwait(false);

			stream.Position = 0;
			return stream;
		}
		catch (GoogleApiException ex) when (ex.HttpStatusCode == HttpStatusCode.NotFound) {
			return null;
		}
	}

	public override async Task<bool> IsSeekable() {
		return true;
	}
	public override async Task<long> GetObjectLength(string fullPath, long defaultValue = -1, CancellationToken cancellationToken = default) {
		try {
			if (fullPath == null) throw new ArgumentNullException(nameof(fullPath));

			fullPath = StoragePath.Normalize(fullPath);

			var obj = await _client.GetObjectAsync(_bucketName, fullPath, null, cancellationToken) .ConfigureAwait(false);

			return (obj != null && obj.Size.HasValue) ? (long)obj.Size.Value : defaultValue;
		}
		catch {
			return defaultValue;
		}
	}

	/// <summary>
	/// Generates a pre-signed URL for the specified object.
	/// The URL grants temporary access to the object and expries after the specified duration. MIME type is auto computed.
	/// </summary>
	public override async Task<string> GetPresignedUrl(string fullPath,bool forDownload,bool https,
		int expiresInSeconds = 86000) {

		if (fullPath == null) throw new ArgumentNullException(nameof(fullPath));
		fullPath = StoragePath.Normalize(fullPath);

		UrlSigner.RequestTemplate template = UrlSigner.RequestTemplate
			.FromBucket(_bucketName)
			.WithObjectName(fullPath)
			.WithHttpMethod(forDownload ? HttpMethod.Get : HttpMethod.Put);

		UrlSigner.Options opt = UrlSigner.Options.FromDuration(TimeSpan.FromSeconds(expiresInSeconds));

		// signed URLs cannot be generated from `StorageClient` alone,
		// we need a `UrlSigner` which is created at init time (from a service account credential or IAM signing service)
		string url = await _urlSigner.SignAsync(template, opt);

		return url;
	}

	/// <summary>
	/// Generates a pre-signed URL for the specified object.
	/// The URL grants temporary access to the object and expries after the specified duration. MIME type is auto computed.
	/// </summary>
	public override async Task<string> GetObjectSas(string objectPath, StorageUrlOptions options) {

		if (options == null)
			throw new ArgumentNullException(nameof(options));

		// supports only the common options.
		return await GetPresignedUrl(
				objectPath,
				options.Permissions.HasFlag(StorageUrlPermissions.Read),
				options.RequireHttps,
				(int)options.ExpiresIn.TotalSeconds)
			.ConfigureAwait(false);
	}

	/// <summary>
	/// Move object from one path to another.
	/// </summary>
	public override async Task<bool> MoveObject(string oldPath,string newPath,bool overwrite, CancellationToken cancellationToken = default) {
		if (string.IsNullOrWhiteSpace(oldPath)) throw new ArgumentNullException(nameof(oldPath));
		if (string.IsNullOrWhiteSpace(newPath)) throw new ArgumentNullException(nameof(newPath));

		oldPath = StoragePath.Normalize(oldPath);
		newPath = StoragePath.Normalize(newPath);

		// exit if overwriting not wanted and the object exists
		if (!overwrite) {
			try {
				await _client.GetObjectAsync(_bucketName,newPath, cancellationToken: cancellationToken);

				return false;
			}
			catch (GoogleApiException ex)
				when (ex.HttpStatusCode == HttpStatusCode.NotFound) {
			}
		}

		await _client.CopyObjectAsync(_bucketName,oldPath,_bucketName,newPath,cancellationToken: cancellationToken);

		await _client.DeleteObjectAsync(_bucketName,oldPath, cancellationToken: cancellationToken);

		return true;
	}

	/// <summary>
	/// Enumerate all objects under the prefix and delete it.
	/// </summary>
	public override async Task DeleteDirectory(string folderPath, bool recursive, CancellationToken cancellationToken = default) {

		if (folderPath == null) throw new ArgumentNullException(nameof(folderPath));

		folderPath = StoragePath.IsRootPath(folderPath) ? "" : StoragePath.Normalize(folderPath) + "/";

		if (recursive) {

			await foreach (var obj in
			               _client.ListObjectsAsync(_bucketName, folderPath).WithCancellation(cancellationToken)) {

				await _client.DeleteObjectAsync(obj, cancellationToken: cancellationToken);
			}
		}
		else {

			bool hasFiles = false;

			await foreach (var obj in _client.ListObjectsAsync(_bucketName,folderPath,
					               new ListObjectsOptions {Delimiter = "/"})
				               .WithCancellation(cancellationToken)) {

				hasFiles = true;
				break;
			}

			if (hasFiles)
				throw new StorageException("Directory is not empty and recursive deletion is disabled! Enable recursive deletion and try again.");

			await foreach (var obj in _client.ListObjectsAsync(_bucketName, folderPath).WithCancellation(cancellationToken)) {

				await _client.DeleteObjectAsync(obj, cancellationToken: cancellationToken);
			}
		}
	}

	/// <summary>
	/// Returns all available versions of the specified object.
	/// </summary>
	public override async Task<List<StorageObjectVersion>> ListObjectVersions(string objectPath, CancellationToken cancellationToken = default) {

		if (objectPath == null) throw new ArgumentNullException(nameof(objectPath));
		objectPath = StoragePath.Normalize(objectPath); 

		var result = new List<StorageObjectVersion>();

		var options = new ListObjectsOptions {
			Versions = true
		};

		try {

			await foreach (var obj in _client.ListObjectsAsync(_bucketName,objectPath,
				               options).WithCancellation(cancellationToken).ConfigureAwait(false)) {

				if (!string.Equals(obj.Name, objectPath, StringComparison.Ordinal))
					continue;

				result.Add(VersionToObject(obj));
			}

			return result;
		}
		catch (GoogleApiException ex) when (ex.HttpStatusCode == HttpStatusCode.NotFound) {

			// Returns an empty collection if versioning is not enabled, or no versions exist, or the object does not exist.
			return new List<StorageObjectVersion>();
		}
	}

	private static StorageObjectVersion VersionToObject(GObject obj) {
		return new StorageObjectVersion {
			VersionId = obj.Generation?.ToString() ?? "",
			IsCurrent = false,
			DateCreated = obj.TimeCreatedDateTimeOffset?.UtcDateTime ?? DateTime.MinValue,
			Length = (long?)obj.Size ?? 0,
			ETag = obj.ETag
		};
	}

	/// <summary>
	/// Returns information about a specific version of an object.
	/// </summary>
	public override async Task<StorageObjectVersion> GetObjectVersion(string objectPath, string versionId, CancellationToken cancellationToken = default) {

		if (objectPath == null) throw new ArgumentNullException(nameof(objectPath));
		objectPath = StoragePath.Normalize(objectPath); 

		if (string.IsNullOrWhiteSpace(versionId))throw new ArgumentNullException(nameof(versionId));

		try {

			var obj = await _client.GetObjectAsync(_bucketName,objectPath,
				new GetObjectOptions {
					Generation = long.Parse(versionId)
				},
				cancellationToken).ConfigureAwait(false);

			return VersionToObject(obj);
		}
		catch (GoogleApiException ex) when (ex.HttpStatusCode == HttpStatusCode.NotFound) {

			// Returns null if the version or object does not exist.
			return null;
		}
	}

	/// <summary>
	/// Restores the specified version as the current version of the object.
	/// </summary>
	public override async Task<bool> RestoreObjectVersion(string objectPath, string versionId, CancellationToken cancellationToken = default) {

		if (objectPath == null) throw new ArgumentNullException(nameof(objectPath));
		objectPath = StoragePath.Normalize(objectPath); 

		if (string.IsNullOrWhiteSpace(versionId))throw new ArgumentNullException(nameof(versionId));

		try {

			await _client.CopyObjectAsync(_bucketName,objectPath,_bucketName,objectPath,
				new CopyObjectOptions {
					SourceGeneration = long.Parse(versionId)
				},
				cancellationToken).ConfigureAwait(false);

			return true;
		}
		catch (GoogleApiException ex) when (ex.HttpStatusCode == HttpStatusCode.NotFound) {

			// Returns true if restored, or false if the object was not found.
			return false;
		}
	}

	/// <summary>
	/// Permanently deletes the specified object version. Does not delete other versions.
	/// </summary>
	public override async Task<bool> DeleteObjectVersion(string objectPath, string versionId, CancellationToken cancellationToken = default) {

		if (objectPath == null) throw new ArgumentNullException(nameof(objectPath));
		objectPath = StoragePath.Normalize(objectPath); 

		if (string.IsNullOrWhiteSpace(versionId))throw new ArgumentNullException(nameof(versionId));

		try {

			await _client.DeleteObjectAsync(_bucketName,objectPath,
				new DeleteObjectOptions {
					Generation = long.Parse(versionId)
				},
				cancellationToken).ConfigureAwait(false);

			return true;
		}
		catch (GoogleApiException ex) when (ex.HttpStatusCode == HttpStatusCode.NotFound) {

			// Returns true if deleted, or false if the object or version was not found.
			return false;
		}
	}

	/// <summary>
	/// Returns true if object versioning is enabled on this bucket.
	/// </summary>
	public override async Task<bool> IsVersioned() {

		var bucket = await _client.GetBucketAsync(_bucketName).ConfigureAwait(false);

		return bucket.Versioning?.Enabled == true;
	}

	/// <summary>
	/// Returns all Custom Metadata associated with the specified object.
	/// Returns an empty collection if no tags exist.
	/// </summary>
	public override async Task<Dictionary<string, string>> GetObjectTags(string objectPath, CancellationToken cancellationToken = default) {
		if (string.IsNullOrWhiteSpace(objectPath))throw new ArgumentNullException(nameof(objectPath));

		objectPath = StoragePath.Normalize(objectPath);

		try {
			var storageObject = await _client.GetObjectAsync(_bucketName, objectPath, cancellationToken: cancellationToken).ConfigureAwait(false);

			return storageObject.Metadata?
				       .ToDictionary(x => x.Key, x => x.Value)
			       ?? new Dictionary<string, string>();
		}
		catch (GoogleApiException ex) when (ex.HttpStatusCode == HttpStatusCode.NotFound) {
			// Returns null if the object cannot be found.
			return null;
		}
	}


	/// <summary>
	/// Replaces all Custom Metadata associated with the specified object.
	/// Existing tags are removed before the new tags are applied.
	/// </summary>
	public override async Task<bool> SetObjectTags(string objectPath, Dictionary<string, string> tags, CancellationToken cancellationToken = default) {
		if (string.IsNullOrWhiteSpace(objectPath))throw new ArgumentNullException(nameof(objectPath));

		objectPath = StoragePath.Normalize(objectPath);

		try {
			var storageObject = await _client.GetObjectAsync(_bucketName, objectPath, cancellationToken: cancellationToken).ConfigureAwait(false);

			storageObject.Metadata = tags ?? new Dictionary<string, string>();

			await _client.UpdateObjectAsync(storageObject, cancellationToken: cancellationToken).ConfigureAwait(false);

			return true;
		}
		catch (GoogleApiException ex) when (ex.HttpStatusCode == HttpStatusCode.NotFound) {
			// Returns true if succeeded, or false if the object cannot be found.
			return false;
		}
	}


	/// <summary>
	/// Removes all Custom Metadata from the specified object.
	/// Does nothing if the object has no tags.
	/// </summary>
	public override async Task<bool> DeleteObjectTags(string objectPath, CancellationToken cancellationToken = default) {
		if (string.IsNullOrWhiteSpace(objectPath))throw new ArgumentNullException(nameof(objectPath));

		objectPath = StoragePath.Normalize(objectPath);

		try {
			var storageObject = await _client.GetObjectAsync(_bucketName, objectPath, cancellationToken: cancellationToken).ConfigureAwait(false);

			storageObject.Metadata = null;

			await _client.UpdateObjectAsync(storageObject, cancellationToken: cancellationToken).ConfigureAwait(false);

			return true;
		}
		catch (GoogleApiException ex) when (ex.HttpStatusCode == HttpStatusCode.NotFound) {
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

		objectPath = StoragePath.Normalize(objectPath);

		try {
			var obj = await _client.GetObjectAsync(_bucketName, objectPath, cancellationToken: cancellationToken).ConfigureAwait(false);

			return GoogleTier.ToFluentTier.TryGetValue(obj.StorageClass, out StorageTier tier)
				? tier : StorageTier.Unknown;
		}
		catch (GoogleApiException ex) when (ex.HttpStatusCode == HttpStatusCode.NotFound) {
			// Returns NotFound if the object cannot be found.
			return StorageTier.NotFound;
		}
	}


	/// <summary>
	/// Changes the storage tier or storage class of the specified object.
	/// </summary>
	public override async Task<bool> SetObjectTier(string objectPath, StorageTier tier, CancellationToken cancellationToken = default) {
		if (string.IsNullOrWhiteSpace(objectPath))throw new ArgumentNullException(nameof(objectPath));

		if (!GoogleTier.FromFluentTier.TryGetValue(tier, out string storageClass))
			throw new StorageException($"Google Cloud Storage does not support the tier \"{tier}\". Use a supported tier and try again.");

		objectPath = StoragePath.Normalize(objectPath);

		try {
			var obj = await _client.GetObjectAsync(_bucketName, objectPath, cancellationToken: cancellationToken).ConfigureAwait(false);

			obj.StorageClass = storageClass;

			await _client.UpdateObjectAsync(obj, cancellationToken: cancellationToken).ConfigureAwait(false);

			return true;
		}
		catch (GoogleApiException ex) when (ex.HttpStatusCode == HttpStatusCode.NotFound) {
			// Returns true if succeeded, or false if the object cannot be found.
			return false;
		}
	}

	public override async Task<bool> IsTiered() {
		return true;
	}

	/// <summary>
	/// Fastest possible implemention to check if a virtual directory exists in a GCP bucket.
	/// List at most one object with a directory prefix by setting `PageSize=1`
	/// </summary>
	public override async Task<bool> DirectoryExists(string fullPath, CancellationToken cancellationToken = default) {
		if (fullPath == null) throw new ArgumentNullException(nameof(fullPath));
		fullPath = StoragePath.Normalize(fullPath);

		// do not check root directory
		if (fullPath.Length == 0) return true;

		if (!fullPath.EndsWith("/"))
			fullPath += "/";

		ObjectsResource.ListRequest request = _client.Service.Objects.List(_bucketName);
		request.Prefix = fullPath;
		request.MaxResults = 1;

		Objects objects = await request.ExecuteAsync(cancellationToken).ConfigureAwait(false);

		return objects != null && objects.Items != null && objects.Items.Count > 0;
	}

#if NET5_0_OR_GREATER
		/// <summary>
		/// Fastest implementation of getting an object checksum using GCP metadata when available.
		/// </summary>
		public override async Task<StorageObjectHash> GetObjectChecksum(string fullPath, StorageHash hash = StorageHash.MD5, CancellationToken cancellationToken = default) {

			if (fullPath == null)
				throw new ArgumentNullException(nameof(fullPath));

			fullPath = StoragePath.Normalize(fullPath);

			// GCP natively stores an MD5 checksum in object metadata
			if (hash == StorageHash.MD5) {
				var obj = await _client.GetObjectAsync(_bucketName, fullPath, null, cancellationToken);

				if (string.IsNullOrEmpty(obj.Md5Hash))
					return null;

				string md5 = Convert.FromBase64String(obj.Md5Hash).ToHexString();

				return new StorageObjectHash(fullPath, md5, StorageHash.MD5);
			}

			// fall back to downloading and hashing locally
			var bytes = await GetBytes(fullPath, cancellationToken).ConfigureAwait(false);
			if (bytes == null) return null;
			var checksum = HashUtility.HashBytes(bytes, hash);
			return new StorageObjectHash(StoragePath.Normalize(fullPath), checksum, hash);
		}
#endif

}
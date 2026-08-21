using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Azure;
using Azure.Storage;
using Azure.Storage.Files.Shares;
using Azure.Storage.Files.Shares.Models;
using FluentStorage.Azure.Files.Utils;
using FluentStorage.Model;
using FluentStorage.Storage;
using FluentStorage.Streaming;
using FluentStorage.Utils.Validation;

namespace FluentStorage.Azure.Files.Storage;

/// <summary>
/// Manages a single Azure Files container.
/// </summary>
public class AzureFilesStore : StoreBase {
	private readonly ShareServiceClient _client;
	private readonly ConcurrentDictionary<string, ShareClient> _shareNameToShareClient =
		new ConcurrentDictionary<string, ShareClient>();


	public AzureFilesStore(ShareServiceClient shareServiceClient, string accountName) {
		_client = shareServiceClient ?? throw new ArgumentNullException(nameof(shareServiceClient));
	}

	public static AzureFilesStore CreateFromAccountNameAndKey(string accountName, string key) {
		return CreateFromAccountNameAndKey(accountName, key, null, default);
	}

	public static AzureFilesStore CreateFromAccountNameAndKey(
		string accountName,
		string key,
		Uri serviceUri,
		AzureCloudEnvironment cloudEnvironment) {
		if (accountName == null) {
			throw new ArgumentNullException(nameof(accountName));
		}

		if (key == null) {
			throw new ArgumentNullException(nameof(key));
		}

		var credential = new StorageSharedKeyCredential(accountName, key);
		var client = new ShareServiceClient(serviceUri ?? AzureStorageIdentity.CreateFileServiceUri(accountName, cloudEnvironment), credential);

		return new AzureFilesStore(client, accountName);
	}

	/// <summary>
	/// Returns the ShareServiceClient instance for this store.
	/// </summary>
	public override async Task<object> GetClient() {
		return _client;
	}

	protected override async Task<List<StoreObject>> ListPath(
		string path, StorageListOptions options, CancellationToken cancellationToken = default) {
		if (StoragePath.IsRootPath(path)) {
			var shares = new List<StoreObject>();

			await foreach (ShareItem share in _client.GetSharesAsync(ShareTraits.Metadata, cancellationToken: cancellationToken).ConfigureAwait(false)) {
				shares.Add(AzConvert.ToBlob(share));
			}

			return shares;
		}
		else {
			var chunk = new List<StoreObject>();

			ShareDirectoryClient dir = await GetDirectoryReferenceAsync(path, false, cancellationToken).ConfigureAwait(false);
			if (dir == null) {
				return chunk;
			}

			try {
				var listOptions = new ShareDirectoryGetFilesAndDirectoriesOptions {
					Prefix = options.FilePrefix,
					Traits = ShareFileTraits.All
				};

				await foreach (ShareFileItem item in dir.GetFilesAndDirectoriesAsync(
					               listOptions,
					               cancellationToken: cancellationToken).ConfigureAwait(false)) {
					chunk.Add(AzConvert.ToBlob(path, item));
				}
			}
			catch (RequestFailedException ex) when (ex.ErrorCode == "ShareNotFound") {
				// Ignore missing shares and return an empty result.
			}
			catch (RequestFailedException ex) when (ex.ErrorCode == "ResourceNotFound") {
				// Ignore missing directories and return an empty result.
			}

			return chunk;
		}
	}

	public override async Task SetObject(
		string fullPath,
		Stream dataStream,
		bool append = false,
		CancellationToken cancellationToken = default) {
		if (dataStream == null) {
			throw new ArgumentNullException(nameof(dataStream));
		}

		ShareFileClient file = await GetFileReferenceAsync(fullPath, true, cancellationToken).ConfigureAwait(false);

		bool disposeUploadStream = !dataStream.CanSeek;
		Stream uploadStream = dataStream.CanSeek
			? new StorageSourceStream(dataStream)
			: await CopyToSeekableStreamAsync(dataStream, cancellationToken).ConfigureAwait(false);

		try {
			long length = uploadStream.Length - uploadStream.Position;

			await file.CreateAsync(length, cancellationToken: cancellationToken).ConfigureAwait(false);

			if (length > 0) {
				await file.UploadRangeAsync(
					new HttpRange(0, length),
					uploadStream,
					cancellationToken: cancellationToken).ConfigureAwait(false);
			}
		}
		finally {
			if (disposeUploadStream) {
				uploadStream.Dispose();
			}
		}
	}

	/// <summary>
	/// Opens an object for reading and returns its content stream.
	/// </summary>
	public override async Task<Stream> OpenRead(string fullPath, CancellationToken cancellationToken = default) {
		ShareFileClient file = await GetFileReferenceAsync(fullPath, false, cancellationToken).ConfigureAwait(false);
		if (file == null) {
			return null;
		}

		try {
			Response<ShareFileDownloadInfo> response = await file.DownloadAsync(cancellationToken: cancellationToken).ConfigureAwait(false);
			return response.Value.Content;
		}
		catch (RequestFailedException ex) when (ex.ErrorCode == "ShareNotFound") {
			return null;
		}
		catch (RequestFailedException ex) when (ex.ErrorCode == "ResourceNotFound") {
			return null;
		}
	}

	public override async Task<Stream> OpenRange(string fullPath,long offset,long length,CancellationToken cancellationToken = default) {
		ShareFileClient file = await GetFileReferenceAsync(fullPath, false, cancellationToken).ConfigureAwait(false);

		if (file == null) {
			return null;
		}

		try {
			var opt = new ShareFileDownloadOptions();
			opt.Range = new HttpRange(offset, length);
			var response = await file.DownloadAsync(opt, cancellationToken).ConfigureAwait(false);

			return response.Value.Content;
		}
		catch (RequestFailedException ex) when (ex.ErrorCode == "ShareNotFound") {
			return null;
		}
		catch (RequestFailedException ex) when (ex.ErrorCode == "ResourceNotFound") {
			return null;
		}
	}

	public override async Task<bool> IsSeekable() {
		return true;
	}

	public override async Task<long> GetObjectLength(string fullPath, long defaultValue = -1, CancellationToken cancellationToken = default) {
		try {
			ShareFileClient file = await GetFileReferenceAsync(fullPath, false, cancellationToken).ConfigureAwait(false);

			if (file == null)
				return defaultValue;

			var properties = await file.GetPropertiesAsync(null, cancellationToken).ConfigureAwait(false);

			return properties != null && properties.Value != null ? properties.Value.ContentLength : defaultValue;
		}
		catch {
			return defaultValue;
		}
	}

	/// <summary>
	/// Opens an object for writing and returns its content stream.
	/// Object will be written when the stream is disposed or flushed.
	/// </summary>
	public override async Task<Stream> OpenWrite(string fullPath, bool overwrite, CancellationToken cancellationToken = default) {

		// exit if file exists and overwriting is disabled
		if (!overwrite && await ObjectExists(fullPath, cancellationToken)) return null;

		ShareFileClient file = await GetFileReferenceAsync(fullPath, true, cancellationToken).ConfigureAwait(false);
		try {
			return await file.OpenWriteAsync(overwrite, 0, null, cancellationToken).ConfigureAwait(false);
		}
		catch (RequestFailedException ex) when (ex.ErrorCode == "ShareNotFound") {
			return null;
		}
	}

	public override async Task<StoreObject> GetObjectInfo(string fullPath, CancellationToken cancellationToken = default) {
		ShareFileClient file = await GetFileReferenceAsync(fullPath, false, cancellationToken).ConfigureAwait(false);
		if (file == null) {
			return null;
		}

		try {
			Response<ShareFileProperties> properties = await file.GetPropertiesAsync(cancellationToken: cancellationToken).ConfigureAwait(false);

			return AzConvert.ToBlob(StoragePath.GetParent(fullPath), file.Name, properties.Value);
		}
		catch (RequestFailedException ex) when (ex.ErrorCode == "ShareNotFound") {
			return null;
		}
		catch (RequestFailedException ex) when (ex.ErrorCode == "ResourceNotFound") {
			return null;
		}
	}

	public override async Task DeleteObject(string fullPath, CancellationToken cancellationToken = default) {
		ShareFileClient file = await GetFileReferenceAsync(fullPath, false, cancellationToken).ConfigureAwait(false);
		if (file == null) {
			return;
		}

		try {
			await file.DeleteAsync(cancellationToken: cancellationToken).ConfigureAwait(false);
		}
		catch (RequestFailedException ex) when (ex.ErrorCode == "ResourceNotFound") {
			/*ShareDirectoryClient dir = await GetDirectoryReferenceAsync(fullPath, false, cancellationToken).ConfigureAwait(false);
			if (dir != null && await dir.ExistsAsync(cancellationToken).ConfigureAwait(false)) {
				await DeleteDirectoryAsync(dir, cancellationToken).ConfigureAwait(false);
				await dir.DeleteIfExistsAsync(cancellationToken: cancellationToken).ConfigureAwait(false);
			}*/
		}
	}

	private static async Task DeleteDirectoryAsync(ShareDirectoryClient dir, CancellationToken cancellationToken = default) {
		await foreach (ShareFileItem item in dir.GetFilesAndDirectoriesAsync(cancellationToken: cancellationToken).ConfigureAwait(false)) {
			if (item.IsDirectory) {
				ShareDirectoryClient subdir = dir.GetSubdirectoryClient(item.Name);
				await DeleteDirectoryAsync(subdir, cancellationToken).ConfigureAwait(false);
				await subdir.DeleteIfExistsAsync(cancellationToken: cancellationToken).ConfigureAwait(false);
			}
			else {
				await dir.DeleteFileAsync(item.Name, cancellationToken: cancellationToken).ConfigureAwait(false);
			}
		}
	}

	public override async Task<bool> ObjectExists(string fullPath, CancellationToken cancellationToken = default) {
		ShareFileClient file = await GetFileReferenceAsync(fullPath, false, cancellationToken).ConfigureAwait(false);

		if (file == null) {
			return false;
		}

		return await file.ExistsAsync(cancellationToken).ConfigureAwait(false);
	}

	public override async Task SetObjectsInfo(IEnumerable<StoreObject> blobs, CancellationToken cancellationToken = default) {
		ArgValidator.AssertFullPaths(blobs);

		await Task.WhenAll(blobs.Select(b => SetObjectInfo(b, cancellationToken))).ConfigureAwait(false);
	}

	public override async Task SetObjectInfo(StoreObject blob, CancellationToken cancellationToken = default) {
		if (blob.IsFolder) {
			ShareDirectoryClient dir = await GetDirectoryReferenceAsync(blob.FullPath, false, cancellationToken).ConfigureAwait(false);
			if (dir != null) {
				await dir.SetMetadataAsync(blob.Metadata, cancellationToken: cancellationToken).ConfigureAwait(false);
			}
		}
		else {
			ShareFileClient file = await GetFileReferenceAsync(blob.FullPath, false, cancellationToken).ConfigureAwait(false);
			if (file != null) {
				await file.SetMetadataAsync(blob.Metadata, cancellationToken: cancellationToken).ConfigureAwait(false);
			}
		}
	}

	private async Task<ShareFileClient> GetFileReferenceAsync(string fullPath, bool createParents, CancellationToken cancellationToken = default) {
		string[] parts = StoragePath.Split(fullPath);
		if (parts.Length == 0) {
			return null;
		}

		ShareClient share = await GetShareReferenceAsync(parts[0], createParents, cancellationToken).ConfigureAwait(false);
		if (share == null) {
			return null;
		}

		ShareDirectoryClient dir = share.GetRootDirectoryClient();
		for (int i = 1; i < parts.Length - 1; i++) {
			string sub = parts[i];
			dir = dir.GetSubdirectoryClient(sub);

			if (createParents) {
				await dir.CreateIfNotExistsAsync(cancellationToken: cancellationToken).ConfigureAwait(false);
			}
		}

		return dir.GetFileClient(parts[parts.Length - 1]);
	}

	private async Task<ShareDirectoryClient> GetDirectoryReferenceAsync(string fullPath, bool createParents, CancellationToken cancellationToken = default) {
		string[] parts = StoragePath.Split(fullPath);
		if (parts.Length == 0) {
			return null;
		}

		ShareClient share = await GetShareReferenceAsync(parts[0], createParents, cancellationToken).ConfigureAwait(false);
		if (share == null) {
			return null;
		}

		ShareDirectoryClient dir = share.GetRootDirectoryClient();
		for (int i = 1; i < parts.Length; i++) {
			string sub = parts[i];
			dir = dir.GetSubdirectoryClient(sub);

			if (createParents) {
				await dir.CreateIfNotExistsAsync(cancellationToken: cancellationToken).ConfigureAwait(false);
			}
		}

		return dir;
	}

	private async Task<ShareClient> GetShareReferenceAsync(string shareName, bool createShare, CancellationToken cancellationToken = default) {
		if (!_shareNameToShareClient.TryGetValue(shareName, out ShareClient share)) {
			share = _client.GetShareClient(shareName);
			try {
				await share.GetPropertiesAsync(cancellationToken: cancellationToken).ConfigureAwait(false);
			}
			catch (RequestFailedException ex) when (ex.ErrorCode == "ShareNotFound") {
				if (createShare) {
					await share.CreateIfNotExistsAsync(cancellationToken: cancellationToken).ConfigureAwait(false);
				}
				else {
					return null;
				}
			}

			_shareNameToShareClient[shareName] = share;
		}

		return share;
	}

	private static async Task<Stream> CopyToSeekableStreamAsync(Stream stream, CancellationToken cancellationToken = default) {
		var memoryStream = new MemoryStream();
		await stream.CopyToAsync(memoryStream, 81920, cancellationToken).ConfigureAwait(false);
		memoryStream.Position = 0;
		return memoryStream;
	}
}
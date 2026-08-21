using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Amazon.S3;
using Amazon.S3.Model;
using FluentStorage.Enums;
using FluentStorage.Model;
using FluentStorage.Storage;
using FluentStorage.Utils.Performance;

namespace FluentStorage.AWS.Storage;

/// <summary>
/// Asnc recursive S3 directory lister with support for running a capped amount of async tasks.
/// </summary>
class S3DirectoryBrowser : IDisposable {
	private readonly AmazonS3Client _client;
	private readonly string _bucketName;
	private AsyncLimiter _limiter;

	public S3DirectoryBrowser(AmazonS3Client client, string bucketName) {
		_client = client;
		_bucketName = bucketName;
	}

	public async Task<List<StoreObject>> ListAsync(StorageListOptions options, CancellationToken cancellationToken = default) {
		var container = new List<StoreObject>();

		_limiter = new AsyncLimiter(options.NumberOfRecursionThreads ?? StorageListOptions.MAX_THREADS);

		await ListFolderAsync(container, options.FolderPath, options, cancellationToken).ConfigureAwait(false);

		return options.MaxResults == null
			? container
			: container.Count > options.MaxResults.Value
				? container.Take(options.MaxResults.Value).ToList()
				: container;
	}

	private async Task ListFolderAsync(List<StoreObject> container, string path, StorageListOptions options, CancellationToken cancellationToken = default) {
		var request = new ListObjectsV2Request() {
			MaxKeys = options.PageSize ?? StorageListOptions.PAGE_SIZE,
			BucketName = _bucketName,
			Prefix = FormatFolderPrefix(path),
			Delimiter = options.Recurse ? null : "/"   //this tells S3 not to go into the folder recursively
		};

		// Server side filtering is supported by supplying a FilePrefix
		if (!string.IsNullOrEmpty(options.FilePrefix)) {
			request.Prefix += options.FilePrefix;
		}

		var folderContainer = new List<StoreObject>();

		while (options.MaxResults == null || (container.Count < options.MaxResults)) {
			ListObjectsV2Response response;

			using (await _limiter.AcquireOneAsync().ConfigureAwait(false)) {
				response = await _client.ListObjectsV2Async(request, cancellationToken).ConfigureAwait(false);
			}

			if (response != null) {
				folderContainer.AddRange(response.ToBlobs(options));
			}

			if (response.NextContinuationToken == null) {
				break;
			}

			request.ContinuationToken = response.NextContinuationToken;
		}

		container.AddRange(folderContainer);

		if (options.Recurse && options.RecursionMode == StorageRecursion.Local) {
			List<StoreObject> folders = folderContainer.Where(b => b.Type == StorageObjectType.Folder).ToList();

			await Task.WhenAll(folders.Select(f => ListFolderAsync(container, f.FullPath, options, cancellationToken))).ConfigureAwait(false);
		}
	}


	private static string FormatFolderPrefix(string folderPath) {
		folderPath = StoragePath.Normalize(folderPath).Substring(1);

		if (StoragePath.IsRootPath(folderPath))
			return null;

		if (!folderPath.EndsWith("/"))
			folderPath += "/";

		return folderPath;
	}


	public async Task DeleteRecursiveAsync(string fullPath, CancellationToken cancellationToken = default) {
		var request = new ListObjectsV2Request() {
			BucketName = _bucketName,
			Prefix = fullPath + "/"
		};

		while (true) {
			ListObjectsV2Response response = await _client.ListObjectsV2Async(request, cancellationToken).ConfigureAwait(false);

			if (response?.S3Objects == null)
				break;

			await _client.DeleteObjectsAsync(new DeleteObjectsRequest() {
				BucketName = _bucketName,
				Objects = response.S3Objects.Select(s3 => new KeyVersion() { Key = s3.Key }).ToList()
			}, cancellationToken).ConfigureAwait(false);

			if (response.NextContinuationToken == null)
				break;

			request.ContinuationToken = response.NextContinuationToken;
		}
	}

	public void Dispose() {
		_limiter?.Dispose();
	}
}
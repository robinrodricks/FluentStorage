using FluentFTP;
using FluentFTP.Exceptions;

using FluentStorage.Blobs;

using Polly;
using Polly.Retry;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace FluentStorage.FTP {
	class FluentFtpBlobStorage : IBlobStorage {
		private readonly AsyncFtpClient _client;
		private readonly bool _dispose;
		private static readonly AsyncRetryPolicy retryPolicy = Policy.Handle<FtpException>().RetryAsync(3);

		public FluentFtpBlobStorage(string hostNameOrAddress, NetworkCredential credentials, FtpDataConnectionType dataConnectionType = FtpDataConnectionType.AutoActive) {
			_client = new AsyncFtpClient(hostNameOrAddress, credentials);
			_client.Config.DataConnectionType = dataConnectionType;
			_dispose = true;
		}

		public FluentFtpBlobStorage(AsyncFtpClient ftpClient, bool dispose = false) {
			_client = ftpClient ?? throw new ArgumentNullException(nameof(ftpClient));
			_dispose = dispose;
		}

		private async Task<AsyncFtpClient> GetClientAsync() {
			if (!_client.IsConnected) {
				await _client.Connect().ConfigureAwait(false);

				//not supported on this platform?
				//await _client.SetHashAlgorithmAsync(FtpHashAlgorithm.MD5);
			}

			return _client;
		}

		public async Task<IReadOnlyCollection<IBlob>> ListAsync(ListOptions options = null, CancellationToken cancellationToken = default) {
			AsyncFtpClient client = await GetClientAsync().ConfigureAwait(false);

			if (options == null)
				options = new ListOptions();

			FtpListItem[] items = await client.GetListing(options.FolderPath).ConfigureAwait(false);

			List<IBlob> results = new List<IBlob>();
			foreach (FtpListItem item in items) {
				if (options.FilePrefix != null && !item.Name.StartsWith(options.FilePrefix)) {
					continue;
				}

				Blob blob = ToBlobId(item);
				if (blob == null)
					continue;

				if (options.BrowseFilter != null) {
					bool include = options.BrowseFilter(blob);
					if (!include)
						continue;
				}

				results.Add(blob);

				if (options.MaxResults != null && results.Count >= options.MaxResults.Value)
					break;
			}

			return results;
		}

		private Blob ToBlobId(FtpListItem ff) {
			if (ff.Type != FtpObjectType.Directory && ff.Type != FtpObjectType.File)
				return null;

			Blob id = new Blob(ff.FullName,
			   ff.Type == FtpObjectType.File
			   ? BlobItemKind.File
			   : BlobItemKind.Folder);

			if (ff.RawPermissions != null) {
				id.Properties["RawPermissions"] = ff.RawPermissions;
			}

			return id;
		}

		public async Task DeleteAsync(IEnumerable<string> fullPaths, CancellationToken cancellationToken = default) {
			AsyncFtpClient client = await GetClientAsync().ConfigureAwait(false);

			foreach (string path in fullPaths) {
				try {
					await client.DeleteFile(path).ConfigureAwait(false);
				}
				catch (FtpCommandException ex) when (ex.CompletionCode == "550") {
					await client.DeleteDirectory(path, cancellationToken).ConfigureAwait(false);
					//550 stands for "file not found" or "permission denied".
					//"not found" is fine to ignore, however I'm not happy about ignoring the second error.
				}
			}
		}

		public async Task<IReadOnlyCollection<bool>> ExistsAsync(IEnumerable<string> ids, CancellationToken cancellationToken = default) {
			AsyncFtpClient client = await GetClientAsync().ConfigureAwait(false);

			List<bool> results = new List<bool>();
			foreach (string path in ids) {
				bool e = await client.FileExists(path).ConfigureAwait(false);
				results.Add(e);
			}

			return results;
		}

		public async Task<IReadOnlyCollection<IBlob>> GetBlobsAsync(IEnumerable<string> ids, CancellationToken cancellationToken = default) {
			AsyncFtpClient client = await GetClientAsync().ConfigureAwait(false);

			List<IBlob> results = new List<IBlob>();
			foreach (string path in ids) {
				string cpath = StoragePath.Normalize(path);
				string parentPath = StoragePath.GetParent(cpath);

				FtpListItem[] all = await client.GetListing(parentPath).ConfigureAwait(false);
				FtpListItem foundItem = all.FirstOrDefault(i => i.FullName == cpath);

				if (foundItem == null) {
					results.Add(null);
					continue;
				}

				Blob r = new Blob(path) {
					Size = foundItem.Size,
					LastModificationTime = foundItem.Modified
				};
				results.Add(r);
			}
			return results;
		}

		public Task SetBlobsAsync(IEnumerable<IBlob> blobs, CancellationToken cancellationToken = default) {
			throw new NotSupportedException();
		}

		public async Task<Stream> OpenReadAsync(string fullPath, CancellationToken cancellationToken = default) {
			AsyncFtpClient client = await GetClientAsync().ConfigureAwait(false);

			try {
				return await client.OpenRead(fullPath, FtpDataType.Binary, 0, true).ConfigureAwait(false);
			}
			catch (FtpCommandException ex) when (ex.CompletionCode == "550") {
				return null;
			}
		}

		public Task<ITransaction> OpenTransactionAsync() => Task.FromResult(EmptyTransaction.Instance);

		public async Task WriteAsync(string fullPath, Stream dataStream,
		   bool append = false, CancellationToken cancellationToken = default) {

			AsyncFtpClient client = await GetClientAsync().ConfigureAwait(false);

			await retryPolicy.ExecuteAsync(async () => {
				string directory = Path.GetDirectoryName(fullPath);

				if (!string.IsNullOrWhiteSpace(directory) && !(await client.DirectoryExists(directory).ConfigureAwait(false))) {
					await client.CreateDirectory(directory, cancellationToken);
				}

				using Stream dest = append
					? await client.OpenAppend(fullPath, FtpDataType.Binary, true, token: cancellationToken).ConfigureAwait(false)
					: await client.OpenWrite(fullPath, FtpDataType.Binary, true, token: cancellationToken).ConfigureAwait(false);

#if NETSTANDARD2_0
				await dataStream.CopyToAsync(dest).ConfigureAwait(false);
#else
				await dataStream.CopyToAsync(dest, cancellationToken).ConfigureAwait(false);
#endif
			}).ConfigureAwait(false);
		}

		public void Dispose() {
			if (_dispose && _client.IsDisposed) {
				_client.Dispose();
			}
		}
	}
}

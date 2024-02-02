using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Polly;
using Polly.Retry;
using Renci.SshNet;
using Renci.SshNet.Sftp;
using FluentStorage.Blobs;

namespace FluentStorage.SFTP {
	public class SshNetSftpBlobStorage : IExtendedBlobStorage {
		/// <summary>
		/// The retry policy
		/// </summary>
		private static readonly AsyncRetryPolicy _retryPolicy = Policy.Handle<Exception>().RetryAsync(3);

		/// <summary>
		/// Holds a reference to the <see cref="T:FluentStorage.SFTP.SshNetSftpBlobStorage" /> instance.
		/// </summary>
		private readonly SftpClient _client;

		/// <summary>
		/// A boolean flag indicating whether to dispose the client instance upon disposing this object.
		/// </summary>
		private readonly bool _disposeClient;

		/// <summary>
		/// A boolean flag indicating whether this instance is disposed.
		/// </summary>
		private bool _disposed = false;

		/// <summary>
		/// Object used in in ListDirectoryAsync to avoid accessing collections from multiple threads at the same time.
		/// </summary>
		private readonly object _listDirectoryLockObject = new object();

		/// <summary>
		/// Gets or sets the maximum retry count.
		/// </summary>
		/// <value>
		/// The maximum retry count.
		/// </value>
		public int MaxRetryCount { get; set; } = 3;

		/// <summary>
		/// Root directory, relative to which all paths will resolve to.
		/// </summary>
		/// <value>
		/// Directory required or null.
		/// </value>
		public string RootDirectory { get; private set; }

		/// <summary>
		/// Initializes a new instance of the <see cref="T:FluentStorage.SFTP.SshNetSftpBlobStorage" /> class.
		/// </summary>
		/// <param name="connectionInfo">The connection info.</param>
		/// <exception cref="T:System.ArgumentNullException"><paramref name="connectionInfo" /> is <b>null</b>.</exception>
		public SshNetSftpBlobStorage(ConnectionInfo connectionInfo)
		  : this(new SftpClient(connectionInfo), true) {
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="T:FluentStorage.SFTP.SshNetSftpBlobStorage" /> class.
		/// </summary>
		/// <param name="host">Connection host.</param>
		/// <param name="port">Connection port.</param>
		/// <param name="username">Authentication username.</param>
		/// <param name="password">Authentication password.</param>
		/// <param name="path">Starting root directory or null.</param>
		/// <exception cref="T:System.ArgumentNullException"><paramref name="password" /> is <b>null</b>.</exception>
		/// <exception cref="T:System.ArgumentException"><paramref name="host" /> is invalid. <para>-or-</para> <paramref name="username" /> is <b>null</b> or contains only whitespace characters.</exception>
		/// <exception cref="T:System.ArgumentOutOfRangeException"><paramref name="port" /> is not within <see cref="F:System.Net.IPEndPoint.MinPort" /> and <see cref="F:System.Net.IPEndPoint.MaxPort" />.</exception>
		public SshNetSftpBlobStorage(string host, int port, string username, string password, string path)
		  : this(new SftpClient(host, port, username, password), true) {
			RootDirectory = path;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="T:FluentStorage.SFTP.SshNetSftpBlobStorage" /> class.
		/// </summary>
		/// <param name="host">Connection host.</param>
		/// <param name="username">Authentication username.</param>
		/// <param name="password">Authentication password.</param>
		/// <exception cref="T:System.ArgumentNullException"><paramref name="password" /> is <b>null</b>.</exception>
		/// <exception cref="T:System.ArgumentException"><paramref name="host" /> is invalid. <para>-or-</para> <paramref name="username" /> is <b>null</b> contains only whitespace characters.</exception>
		public SshNetSftpBlobStorage(string host, string username, string password)
		  : this(new SftpClient(host, username, password), true) {
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="T:FluentStorage.SFTP.SshNetSftpBlobStorage" /> class.
		/// </summary>
		/// <param name="host">Connection host.</param>
		/// <param name="port">Connection port.</param>
		/// <param name="username">Authentication username.</param>
		/// <param name="keyFiles">Authentication private key file(s) .</param>
		/// <exception cref="T:System.ArgumentNullException"><paramref name="keyFiles" /> is <b>null</b>.</exception>
		/// <exception cref="T:System.ArgumentException"><paramref name="host" /> is invalid. <para>-or-</para> <paramref name="username" /> is nu<b>null</b>ll or contains only whitespace characters.</exception>
		/// <exception cref="T:System.ArgumentOutOfRangeException"><paramref name="port" /> is not within <see cref="F:System.Net.IPEndPoint.MinPort" /> and <see cref="F:System.Net.IPEndPoint.MaxPort" />.</exception>
		public SshNetSftpBlobStorage(string host, int port, string username, params PrivateKeyFile[] keyFiles)
		  : this(new SftpClient(host, port, username, keyFiles), true) {
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="T:FluentStorage.SFTP.SshNetSftpBlobStorage" /> class.
		/// </summary>
		/// <param name="host">Connection host.</param>
		/// <param name="username">Authentication username.</param>
		/// <param name="keyFiles">Authentication private key file(s) .</param>
		/// <exception cref="T:System.ArgumentNullException"><paramref name="keyFiles" /> is <b>null</b>.</exception>
		/// <exception cref="T:System.ArgumentException"><paramref name="host" /> is invalid. <para>-or-</para> <paramref name="username" /> is <b>null</b> or contains only whitespace characters.</exception>
		public SshNetSftpBlobStorage(string host, string username, params PrivateKeyFile[] keyFiles)
		  : this(new SftpClient(host, username, keyFiles), true) {
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="T:FluentStorage.SFTP.SshNetSftpBlobStorage" /> class.
		/// </summary>
		/// <param name="sftpClient">The SFTP client.</param>
		/// <param name="disposeClient">if set to <see langword="true" /> [dispose client].</param>
		/// <exception cref="System.ArgumentNullException">sftpClient</exception>
		public SshNetSftpBlobStorage(SftpClient sftpClient, bool disposeClient = false) {
			_client = sftpClient ?? throw new ArgumentNullException(nameof(sftpClient));
			_client.HostKeyReceived += (sender, args) => { };
			_disposeClient = disposeClient;
		}

		/// <summary>
		/// Deletes a list of objects by their full path.
		/// </summary>
		/// <param name="fullPaths">The collection of full paths to delete. If this paths points to a folder, the folder is deleted recursively.</param>
		/// <param name="cancellationToken"></param>
		/// <returns></returns>
		public async Task DeleteAsync(IEnumerable<string> fullPaths, CancellationToken cancellationToken = default) {
			ThrowIfDisposed();

			SftpClient client = GetClient();

			await Task.WhenAll(fullPaths.Select(fullPath => DeleteAsync(fullPath, client, cancellationToken))).ConfigureAwait(false);
		}

		/// <summary>
		/// Deletes an object by it's full path.
		/// </summary>
		/// <param name="fullPath">The full path.</param>
		/// <param name="client">The sftp client to use.</param>
		/// <param name="cancellationToken">The cancellation token.</param>
		/// <returns></returns>
		private Task DeleteAsync(string fullPath, SftpClient client, CancellationToken cancellationToken = default) {
			if (cancellationToken.IsCancellationRequested) {
				return Task.FromCanceled(cancellationToken);
			}

			fullPath = StoragePath.Combine(RootDirectory, StoragePath.Normalize(fullPath));

			// Todo: Support recursive deleting of folders with files.
			client.Delete(fullPath);

			return Task.CompletedTask;
		}

		/// <summary>
		/// Determine whether the blobs exists in the storage
		/// </summary>
		/// <param name="fullPaths">List of paths to blobs</param>
		/// <param name="cancellationToken"></param>
		/// <returns>
		/// List of results of true and false indicating existence
		/// </returns>
		public async Task<IReadOnlyCollection<bool>> ExistsAsync(IEnumerable<string> fullPaths, CancellationToken cancellationToken = default) {
			ThrowIfDisposed();

			SftpClient client = GetClient();

			return await Task.WhenAll(fullPaths.Select(fullPath => ExistsAsync(fullPath, client, cancellationToken))).ConfigureAwait(false);
		}

		/// <summary>
		/// Determine whether the blobs exists in the storage
		/// </summary>
		/// <param name="fullPath">List of paths to blobs</param>
		/// <param name="client">The sftp client to use.</param>
		/// <param name="cancellationToken"></param>
		/// <returns>
		/// List of results of true and false indicating existence
		/// </returns>
		private Task<bool> ExistsAsync(string fullPath, SftpClient client, CancellationToken cancellationToken = default) {
			if (cancellationToken.IsCancellationRequested) {
				return Task.FromCanceled<bool>(cancellationToken);
			}

			fullPath = StoragePath.Combine(RootDirectory, StoragePath.Normalize(fullPath));

			bool fullPathExists = client.Exists(fullPath);

			return Task.FromResult(fullPathExists);
		}

		/// <summary>
		/// Gets blob information which is useful for retrieving blob metadata
		/// </summary>
		/// <param name="fullPaths"></param>
		/// <param name="cancellationToken"></param>
		/// <returns>
		/// List of blob IDs
		/// </returns>
		public async Task<IReadOnlyCollection<Blob>> GetBlobsAsync(IEnumerable<string> fullPaths, CancellationToken cancellationToken = default) {
			ThrowIfDisposed();

			SftpClient client = GetClient();

			var results = new List<Blob>();
			var fullPathsWithRoot = fullPaths.Select(fullPath => StoragePath.Combine(RootDirectory, fullPath));
			foreach (IGrouping<string, string> fullPathGrouping in fullPathsWithRoot.GroupBy(StoragePath.GetParent)) {
				string fullPath = fullPathGrouping.SingleOrDefault();

				if (cancellationToken.IsCancellationRequested) {
					break;
				}

				try {
					List<Blob> blobCollection = new List<Blob>();

					await foreach (SftpFile sftpFile in client.ListDirectoryAsync(fullPathGrouping.Key, cancellationToken)) {
						if ((sftpFile.IsDirectory || sftpFile.IsRegularFile) && sftpFile.FullName == fullPath) {
							blobCollection.Add(ConvertSftpFileToBlob(sftpFile));
						}
					}

					if (blobCollection.Any()) {
						// If using a RoodDirectory, remove from full path.
						if (RootDirectory != null) {
							foreach (var b in blobCollection) {
								b.SetFullPath(b.FullPath.Substring(RootDirectory.Length + 1));
							}
						}
						results.AddRange(blobCollection);
					}
					else {
						results.Add(null);
					}
				}
				catch (Renci.SshNet.Common.SftpPathNotFoundException) {
					// If the directoy did not exists, the SSH client will return this exception. To
					// normalize with other storage implementations, we'll return null without
					// raising an error.
					results.Add(null);
				}
			}

			return results;
		}

		/// <summary>
		/// Returns the list of available blobs
		/// </summary>
		/// <param name="options"></param>
		/// <param name="cancellationToken"></param>
		/// <returns>
		/// List of blob IDs
		/// </returns>
		public async Task<IReadOnlyCollection<Blob>> ListAsync(ListOptions options = null, CancellationToken cancellationToken = default) {
			ThrowIfDisposed();

			options ??= new ListOptions();
			options.MaxResults ??= int.MaxValue;
			options.BrowseFilter ??= _ => true;

			SftpClient client = GetClient();

			var folder = StoragePath.Combine(RootDirectory, StoragePath.Normalize(options.FolderPath));

			var blobCollection = await ListDirectoryAsync(client, folder, options, cancellationToken);

			if (RootDirectory != null) {
				foreach (var b in blobCollection) {
					b.SetFullPath(b.FullPath.Substring(RootDirectory.Length + 1));
				}
			}

			return blobCollection;
		}


		/// <summary>
		/// Used internally. Returns a list of available blobs. 
		/// </summary>
		/// <param name="client"></param>
		/// <param name="folderToList"></param>
		/// <param name="options"></param>
		/// <param name="cancellationToken"></param>
		/// <returns>List of blob IDs</returns>
		async Task<IReadOnlyCollection<Blob>> ListDirectoryAsync(SftpClient client, string folderToList, ListOptions options, CancellationToken cancellationToken) {

			List<Blob> blobCollection = new List<Blob>();

			// Note: options.FolderPath is not used here, we use the folderToList which is passed in.
			List<SftpFile> directoryContents = new List<SftpFile>();
			await foreach (SftpFile sftpFile in client.ListDirectoryAsync(folderToList, cancellationToken)) {
				if ((options.FilePrefix == null || sftpFile.Name.StartsWith(options.FilePrefix))
							 && (sftpFile.IsDirectory || sftpFile.IsRegularFile || sftpFile.OwnerCanRead)
							 && !cancellationToken.IsCancellationRequested
							 && sftpFile.Name != "."
							 && sftpFile.Name != "..") {
					directoryContents.Add(sftpFile);
				}
			}

			var tempBlobCollection = directoryContents
				.Take(options.MaxResults.Value)
				.Select(ConvertSftpFileToBlob)
				.Where(options.BrowseFilter).ToList();

			blobCollection.AddRange(tempBlobCollection);
			
			if (options.Recurse == true) {
				IEnumerable<string> subFoldersToList = tempBlobCollection
					.Where(x => x.IsFolder == true)
					.Select(x => x.FullPath);

#if NET6_0_OR_GREATER
				await Parallel.ForEachAsync(subFoldersToList, async (subFolder, token) => {
					var tempForEachBlobCollection = await ListDirectoryAsync(client, subFolder, options, cancellationToken);
					lock (_listDirectoryLockObject) {
						blobCollection.AddRange(tempForEachBlobCollection);
					}
				});
#else
				foreach (string subFolder in subFoldersToList) {
					var tempForEachBlobCollection = await ListDirectoryAsync(client, subFolder, options, cancellationToken);
					blobCollection.AddRange(tempForEachBlobCollection);
				}
#endif
			}

			return blobCollection;
		}

		/// <summary>
		/// Opens the blob stream to read.
		/// </summary>
		/// <param name="fullPath">Blob's full path</param>
		/// <param name="cancellationToken"></param>
		/// <returns>
		/// Stream in an open state, or null if blob doesn't exist by this ID. It is your responsibility to close and dispose this
		/// stream after use.
		/// </returns>
		public async Task<Stream> OpenReadAsync(string fullPath, CancellationToken cancellationToken = default) {
			ThrowIfDisposed();

			fullPath = StoragePath.Combine(RootDirectory, StoragePath.Normalize(fullPath));

			SftpClient client = GetClient();

			try {
				byte[] fileBytes = await Task.FromResult(Policy.Handle<Exception>().Retry(MaxRetryCount).Execute(() => client.ReadAllBytes(fullPath)));
				return new MemoryStream(fileBytes);
			}
			catch (Exception /*exception*/) {
				return null;
			}
		}

		/// <summary>
		/// Starts a new transaction
		/// </summary>
		/// <returns></returns>
		public Task<ITransaction> OpenTransactionAsync() {
			ThrowIfDisposed();
			return Task.FromResult(EmptyTransaction.Instance);
		}

		/// <summary>
		/// Rename a blob (folder or file)
		/// </summary>
		/// <param name="oldPath"></param>
		/// <param name="newPath"></param>
		/// <param name="cancellationToken"></param>
		/// <returns></returns>
		public Task RenameAsync(string oldPath, string newPath, CancellationToken cancellationToken = default) {
			ThrowIfDisposed();

			oldPath = StoragePath.Combine(RootDirectory, StoragePath.Normalize(oldPath));
			newPath = StoragePath.Combine(RootDirectory, StoragePath.Normalize(newPath));

			SftpClient client = GetClient();

			client.RenameFile(oldPath, newPath);

			return Task.CompletedTask;
		}

		/// <summary>
		/// Set blob information which is useful for setting blob attributes (user metadata etc.)
		/// </summary>
		/// <param name="blobs"></param>
		/// <param name="cancellationToken"></param>
		/// <returns></returns>
		/// <exception cref="System.NotSupportedException"></exception>
		public Task SetBlobsAsync(IEnumerable<Blob> blobs, CancellationToken cancellationToken = default) {
			ThrowIfDisposed();
			throw new NotSupportedException();
		}

		/// <summary>
		/// Uploads data to a blob from stream.
		/// </summary>
		/// <param name="fullPath">Blob metadata</param>
		/// <param name="dataStream">Stream to upload from</param>
		/// <param name="append">When true, appends to the file instead of writing a new one.</param>
		/// <param name="cancellationToken"></param>
		/// <returns>
		/// Writeable stream
		/// </returns>
		public async Task WriteAsync(string fullPath, Stream dataStream, bool append = false, CancellationToken cancellationToken = default) {
			ThrowIfDisposed();

			SftpClient client = GetClient();
			var fullPathWithRoot = StoragePath.Combine(RootDirectory, StoragePath.Normalize(fullPath));
			var fileMode = append ? FileMode.Append : FileMode.OpenOrCreate;

			// First, for speed, let's try to write the file assuming the directory requested already exists.

			try {
				using (Stream dest = client.Open(fullPathWithRoot, fileMode, FileAccess.Write)) {
					await dataStream.CopyToAsync(dest).ConfigureAwait(false);
					if (append == false) {
						dest.SetLength(dataStream.Length);
					}
				}
				return;
			}
			catch (Renci.SshNet.Common.SftpPathNotFoundException) {
				// If the folder did not exist, continue below.
			}

			// Create any non-existing directories. We'll need to recursively check each part and
			// create if it does not exist.
			var parts = StoragePath.Split(fullPath).ToList();
			parts.RemoveAt(parts.Count - 1);

			await _retryPolicy.ExecuteAsync(async () => {
				var fullFolder = RootDirectory;
				foreach (var folder in parts) {
					fullFolder = StoragePath.Combine(fullFolder, folder);
					if (!client.Exists(fullFolder))
						client.CreateDirectory(fullFolder);
				}

				using (Stream dest = client.Open(fullPathWithRoot, fileMode, FileAccess.Write)) {
					await dataStream.CopyToAsync(dest).ConfigureAwait(false);
					if (append == false) {
						dest.SetLength(dataStream.Length);
					}
				}
			}).ConfigureAwait(false);
		}

		/// <summary>
		/// Gets the <see cref="T:Renci.SshNet.SftpClient" /> instance.
		/// </summary>
		/// <returns>The <see cref="T:Renci.SshNet.SftpClient" /> instance.</returns>
		protected SftpClient GetClient() {
			ThrowIfDisposed();

			if (!_client.IsConnected) {
				_client.Connect();
			}

			return _client;
		}

		/// <summary>
		/// Converts the specified <see cref="T:Renci.SshNet.Sftp.SftpFile"/> into a <see cref="T:FluentStorage.Blobs.Blob"/> instance.
		/// </summary>
		/// <param name="file">The file.</param>
		/// <returns></returns>
		private static Blob ConvertSftpFileToBlob(SftpFile file) {
			if (file.IsDirectory || file.IsRegularFile || file.OwnerCanRead) {
				BlobItemKind itemKind = file.IsDirectory
				   ? BlobItemKind.Folder
				   : BlobItemKind.File;

				return new Blob(file.FullName, itemKind) {
					Size = file.Length,
					LastModificationTime = file.LastWriteTime
				};
			}

			return null;
		}

		/// <summary>
		/// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
		/// </summary>
		public void Dispose() {
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		/// <summary>
		/// Releases unmanaged and - optionally - managed resources.
		/// </summary>
		/// <param name="disposing"><see langword="true" /> to release both managed and unmanaged resources; <see langword="false" /> to release only unmanaged resources.</param>
		protected virtual void Dispose(bool disposing) {
			if (_disposed) {
				return;
			}

			// Release any managed resources here.
			if (disposing && _disposeClient) {
				_client.Dispose();
			}

			_disposed = true;
		}

		/// <summary>
		/// Throws an <see cref="T:System.ObjectDisposedException" /> if this object has been disposed.
		/// </summary>
		/// <exception cref="T:System.ObjectDisposedException">The current instance is disposed.</exception>
		protected void ThrowIfDisposed() {
			if (_disposed) {
				throw new ObjectDisposedException(GetType().FullName);
			}
		}
	}
}
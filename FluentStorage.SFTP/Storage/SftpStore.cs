using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentStorage.Enums;
using FluentStorage.Model;
using FluentStorage.SFTP.Shell;
using FluentStorage.Storage;
using FluentStorage.Utils.Hashing;
using Polly;
using Polly.Retry;
using Renci.SshNet;
using Renci.SshNet.Common;
using Renci.SshNet.Sftp;

namespace FluentStorage.SFTP;

/// <summary>
/// Manages a single connected SFTP server using SSH.NET. Exclusively synchronous.
/// </summary>
public class SftpStore : StoreBase {
	/// <summary>
	/// The retry policy
	/// </summary>
	private static readonly AsyncRetryPolicy _retryPolicy = Policy.Handle<Exception>().RetryAsync(3);

	/// <summary>
	/// Connected SFTP client
	/// </summary>
	private readonly SftpClient _client;

	/// <summary>
	/// Connected SSH client context
	/// </summary>
	private readonly SshContext _sshContext;

	/// <summary>
	/// A boolean flag indicating whether to dispose the client instance upon disposing this object.
	/// </summary>
	private readonly bool _disposeClient;

	/// <summary>
	/// A boolean flag indicating whether this instance is disposed.
	/// </summary>
	private bool _disposed = false;

	/// <summary>
	/// Gets or sets the maximum retry count.
	/// </summary>
	/// <value>
	/// The maximum retry count.
	/// </value>
	public int MaxRetryCount { get; set; } = 3;

	private uint _transferBufferSize = 128 * 1024;
	/// <summary>
	/// Buffer size used when uploading or downloading files, in bytes. Default: 128KB.
	/// </summary>
	public uint TransferBufferSize {
		get { return _transferBufferSize; }
		set {
			if (value == 0) throw new ArgumentOutOfRangeException(nameof(value));
			_transferBufferSize = value;
			if(_client != null) _client.BufferSize = _transferBufferSize;
		}
	}

	/// <summary>
	/// Root directory, relative to which all paths will resolve to. Default: null.
	/// </summary>
	public string RootDirectory { get; private set; }

	/// <summary>
	/// Initializes a new instance of the <see cref="T:FluentStorage.SFTP.SshNetSftpBlobStorage" /> class.
	/// </summary>
	/// <param name="connectionInfo">The connection info.</param>
	public SftpStore(ConnectionInfo connectionInfo)
		: this(new SftpClient(connectionInfo),
			new SshClient(connectionInfo), true) {
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="T:FluentStorage.SFTP.SshNetSftpBlobStorage" /> class.
	/// </summary>
	/// <param name="host">Connection host.</param>
	/// <param name="port">Connection port.</param>
	/// <param name="username">Authentication username.</param>
	/// <param name="password">Authentication password.</param>
	/// <param name="path">Starting root directory or null.</param>
	public SftpStore(string host, int port, string username, string password, string path)
		: this(new SftpClient(host, port, username, password),
			new SshClient(host, port, username, password), true) {
		RootDirectory = StoragePath.Normalize(path);
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="T:FluentStorage.SFTP.SshNetSftpBlobStorage" /> class.
	/// </summary>
	/// <param name="host">Connection host.</param>
	/// <param name="username">Authentication username.</param>
	/// <param name="password">Authentication password.</param>
	public SftpStore(string host, string username, string password)
		: this(new SftpClient(host, username, password),
			new SshClient(host, username, password), true) {
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="T:FluentStorage.SFTP.SshNetSftpBlobStorage" /> class.
	/// </summary>
	/// <param name="host">Connection host.</param>
	/// <param name="port">Connection port.</param>
	/// <param name="username">Authentication username.</param>
	/// <param name="keyFiles">Authentication private key file(s) .</param>
	public SftpStore(string host, int port, string username, params PrivateKeyFile[] keyFiles)
		: this(new SftpClient(host, port, username, keyFiles),
			new SshClient(host, port, username, keyFiles), true) {
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="T:FluentStorage.SFTP.SshNetSftpBlobStorage" /> class.
	/// </summary>
	/// <param name="host">Connection host.</param>
	/// <param name="username">Authentication username.</param>
	/// <param name="keyFiles">Authentication private key file(s) .</param>
	public SftpStore(string host, string username, params PrivateKeyFile[] keyFiles)
		: this(new SftpClient(host, username, keyFiles),
			new SshClient(host, username, keyFiles), true) {
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="T:FluentStorage.SFTP.SshNetSftpBlobStorage" /> class.
	/// </summary>
	/// <param name="sftpClient">The SFTP client.</param>
	/// <param name="disposeClient">if set to true [dispose client].</param>
	public SftpStore(SftpClient sftpClient, SshClient sshClient, bool disposeClient = false) {
		_client = sftpClient ?? throw new ArgumentNullException(nameof(sftpClient));
		_client.HostKeyReceived += (sender, args) => { };
		_disposeClient = disposeClient;

		// FIX: improve peformance by increasing buffer size
		_client.BufferSize = TransferBufferSize;

		// add support for SSH command execution
		_sshContext = new SshContext(sshClient);
	}

	public override async Task<bool> IsFileSystem() {
		return true;
	}

	/// <summary>
	/// Normalize path and add SFTP root directory. One way process. Not idempotent.
	/// </summary>
	private string AddRootDirAndNormalize(string path) {
		return "/" + StoragePath.Combine(RootDirectory, StoragePath.Normalize(path));
	}
	/// <summary>
	/// Convert to SFTP absolute path. One way process. Not idempotent.
	/// </summary>
	private string AddAbsolutePrefix(string path) {
		return "/" + StoragePath.Normalize(path);
	}

	/// <summary>
	/// Deletes a list of objects by their full path.
	/// </summary>
	/// <param name="fullPaths">The collection of full paths to delete. If this paths points to a folder, the folder is deleted recursively.</param>
	/// <param name="cancellationToken"></param>
	/// <returns></returns>
	public override async Task DeleteObjects(IEnumerable<string> fullPaths, CancellationToken cancellationToken = default) {
		ThrowIfDisposed();

		await Task.WhenAll(fullPaths.Select(fullPath => DeleteObject(fullPath, cancellationToken))).ConfigureAwait(false);
	}

	/// <summary>
	/// Deletes an object by its full path.
	/// </summary>
	/// <param name="fullPath">The full path.</param>
	/// <param name="cancellationToken">The cancellation token.</param>
	/// <returns></returns>
	public override async Task DeleteObject(string fullPath, CancellationToken cancellationToken = default) {
		if (cancellationToken.IsCancellationRequested) {
			return;
		}

		SftpClient client = Client();

		await client.DeleteAsync(AddRootDirAndNormalize(fullPath), cancellationToken);
	}

	/// <summary>
	/// Determine whether the blobs exists in the storage
	/// </summary>
	/// <param name="fullPaths">List of paths to blobs</param>
	/// <param name="cancellationToken"></param>
	/// <returns>
	/// List of results of true and false indicating existence
	/// </returns>
	public override async Task<List<bool>> ObjectsExists(IEnumerable<string> fullPaths, CancellationToken cancellationToken = default) {
		ThrowIfDisposed();

		return (await Task.WhenAll(fullPaths.Select(fullPath => ObjectExists(fullPath, cancellationToken))).ConfigureAwait(false)).ToList();
	}

	/// <summary>
	/// Determine whether the blobs exists in the storage
	/// </summary>
	/// <param name="fullPath">List of paths to blobs</param>
	/// <param name="cancellationToken"></param>
	/// <returns>
	/// List of results of true and false indicating existence
	/// </returns>
	public override async Task<bool> ObjectExists(string fullPath, CancellationToken cancellationToken = default) {

		if (cancellationToken.IsCancellationRequested) {
			return false;
		}

		SftpClient client = Client();

		return await client.ExistsAsync(AddRootDirAndNormalize(fullPath), cancellationToken);
	}

	public override async Task<StoreObject> GetObjectInfo(string path, CancellationToken cancellationToken = default) {
		return (await GetObjectsInfo(new List<string> { path }, cancellationToken).ConfigureAwait(false)).FirstOrDefault();
	}
	public override async Task<List<StoreObject>> GetObjectsInfo(IEnumerable<string> fullPaths, CancellationToken cancellationToken = default) {
		ThrowIfDisposed();

		SftpClient client = Client();

		var results = new List<StoreObject>();

		// clean paths
		var fullPathsWithRoot = fullPaths.Select(AddRootDirAndNormalize);

		// compute common dirs
		var groups = fullPathsWithRoot.GroupBy(p => AddAbsolutePrefix(StoragePath.GetParent(p)));

		// per dir
		foreach (var fullPathGrouping in groups) {

			string fullPath = fullPathGrouping.SingleOrDefault();

			// stop if cancelled
			if (cancellationToken.IsCancellationRequested) {
				break;
			}

			try {

				// get the listing for the dir and collect paths where it matches the required object paths
				List<StoreObject> dirListing = new List<StoreObject>();
				await foreach (SftpFile sftpFile in client.ListDirectoryAsync(fullPathGrouping.Key, cancellationToken)) {
					if ((sftpFile.IsDirectory || sftpFile.IsRegularFile) && sftpFile.FullName == fullPath) {
						dirListing.Add(ConvertSftpFileToBlob(sftpFile));
					}
				}

				if (dirListing.Any()) {
					// If using a RootDirectory, remove it from the object full path
					if (RootDirectory != null) {
						foreach (var b in dirListing) {
							b.SetFullPath(b.FullPath.Substring(RootDirectory.Length + 1));
						}
					}
					results.AddRange(dirListing);
				}
				else {
					results.Add(null);
				}
			}
			catch (SftpPathNotFoundException) {
				// If the directory did not exist, the SFTP client will return this exception.
				// To normalize with other storage implementations, we'll add null to the results.
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
	public override async Task<List<StoreObject>> ListObjects(StorageListOptions options = null, CancellationToken cancellationToken = default) {
		ThrowIfDisposed();

		options ??= new StorageListOptions();

		SftpClient client = Client();

		var results = await ListDirectoryInternal(client, AddRootDirAndNormalize(options.FolderPath), options, cancellationToken);

		if (RootDirectory != null) {
			foreach (var b in results) {
				b.SetFullPath(b.FullPath.Substring(RootDirectory.Length + 1));
			}
		}

		return results;
	}


	/// <summary>
	/// Used internally to list directory contents recursively
	/// </summary>
	private async Task<List<StoreObject>> ListDirectoryInternal(SftpClient client, string folderToList, StorageListOptions options, CancellationToken cancellationToken = default) {

		List<StoreObject> results = new List<StoreObject>();

		// Note: options.FolderPath is not used here, we use the folderToList which is passed in.
		List<SftpFile> directoryContents = new List<SftpFile>();

		try {
			await foreach (SftpFile sftpFile in client.ListDirectoryAsync(folderToList, cancellationToken)) {
				if ((options.FilePrefix == null || sftpFile.Name.StartsWith(options.FilePrefix))
				    && (sftpFile.IsDirectory || sftpFile.IsRegularFile || sftpFile.OwnerCanRead)
				    && !cancellationToken.IsCancellationRequested
				    && sftpFile.Name != "."
				    && sftpFile.Name != "..") {
					directoryContents.Add(sftpFile);
				}
			}
		}
		catch (SftpPathNotFoundException) {
			// If the directory did not exist, catch it as its non-critical,
			// and quickly exit since the dir is blank, nothing more to do here.
			return results;
		}

		// FILTERING: pass the dir contents via the `MaxResults` and `BrowseFilter` filters
		List<SftpFile> tempList1 = directoryContents;
		if (options.MaxResults.HasValue) {
			tempList1 = tempList1.Take(options.MaxResults.Value).ToList();
		}
		List<StoreObject> tempList2 = tempList1.Select(ConvertSftpFileToBlob).ToList();
		if (options.BrowseFilter != null) {
			tempList2 = tempList2.Where(options.BrowseFilter).ToList();
		}
		results.AddRange(tempList2);


		// RECURSE: if enabled then recurse all sub directories
		if (options.Recurse == true) {

			// per subfolder
			for (int i = 0; i < tempList2.Count; i++) {
				if (!tempList2[i].IsFolder)
					continue;

				// recurse into subfolder
				var subListing = await ListDirectoryInternal(client,AddAbsolutePrefix(tempList2[i].FullPath),options,cancellationToken);

				results.AddRange(subListing);
			}

		}

		return results;
	}

	/// <summary>
	/// Opens a file for reading and returns its content stream.
	/// It is your responsibility to close and dispose this stream after use.
	/// </summary>
	public override async Task<Stream> OpenRead(string fullPath, CancellationToken cancellationToken = default) {
		ThrowIfDisposed();

		SftpClient client = Client();

		MemoryStream stream = new MemoryStream();

		try {
			await Task.Run(() => Policy.Handle<Exception>().Retry(MaxRetryCount).Execute(
				async () => await client.DownloadFileAsync(AddRootDirAndNormalize(fullPath), stream, cancellationToken))
			);
			stream.Position = 0;
			return stream;
		}
		catch (Exception) {
			stream?.Dispose();
			return null;
		}
	}

	public override async Task<Stream> OpenRange(string fullPath, long offset, long length, CancellationToken cancellationToken = default) {
		ThrowIfDisposed();

		SftpClient client = Client();

		Stream stream = client.OpenRead(AddRootDirAndNormalize(fullPath));
		stream.Seek(offset, SeekOrigin.Begin);

		return stream;
	}

	public override async Task<bool> IsSeekable() {
		return true;
	}
	public override async Task<long> GetObjectLength(string fullPath, long defaultValue = -1, CancellationToken cancellationToken = default) {
		try {
			ThrowIfDisposed();

			SftpClient client = Client();

			var attrib = await client.GetAttributesAsync(AddRootDirAndNormalize(fullPath), cancellationToken);

			return attrib != null ? attrib.Size : defaultValue;
		}
		catch {
			return defaultValue;
		}
	}

	/// <summary>
	/// Rename a file on the SFTP server.
	/// </summary>
	/// <param name="oldPath">Existing Remote file path</param>
	/// <param name="newPath">New Remote file path</param>
	public override async Task<bool> MoveObject(string oldPath, string newPath, bool overwrite, CancellationToken cancellationToken = default) {
		ThrowIfDisposed();
		if (string.IsNullOrWhiteSpace(oldPath)) throw new ArgumentNullException(nameof(oldPath));
		if (string.IsNullOrWhiteSpace(newPath)) throw new ArgumentNullException(nameof(newPath));

		oldPath = AddRootDirAndNormalize(oldPath);
		newPath = AddRootDirAndNormalize(newPath);

		SftpClient client = Client();

		if (!overwrite && await ObjectExists(newPath)) return false;

		await client.RenameFileAsync(oldPath, newPath, cancellationToken);

		return true;
	}

	/// <summary>
	/// Returns the SftpClient instance for this store.
	/// </summary>
	public override async Task<object> GetClient() {
		return Client();
	}

	private SftpClient Client() {
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
	private static StoreObject ConvertSftpFileToBlob(SftpFile file) {
		if (file.IsDirectory || file.IsRegularFile || file.OwnerCanRead) {
			StorageObjectType itemKind = file.IsDirectory
				? StorageObjectType.Folder
				: StorageObjectType.File;

			return new StoreObject(file.FullName, itemKind) {
				Size = file.Length,
				DateModified = file.LastWriteTime
			};
		}

		return null;
	}

	/// <summary>
	/// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
	/// </summary>
	public override void Dispose() {
		Dispose(true);
		GC.SuppressFinalize(this);
	}

	/// <summary>
	/// Releases unmanaged and - optionally - managed resources.
	/// </summary>
	/// <param name="disposing">true to release both managed and unmanaged resources; false to release only unmanaged resources.</param>
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
	protected void ThrowIfDisposed() {
		if (_disposed) {
			throw new ObjectDisposedException(GetType().FullName);
		}
	}

	/// <summary>
	/// Gets information about the connected SFTP server.
	///
	/// Returns a dictionary with `ProtocolVersion`, `ServerVersion`, `ClientVersion`,
	/// `ClientEncryption`, `ServerEncryption`, `KeyExchangeAlgorithm`,
	/// `ClientHmacAlgorithm`, `ServerHmacAlgorithm`,
	/// `ClientCompressionAlgorithm`, `ServerCompressionAlgorithm`.
	/// </summary>
	public override async Task<Dictionary<string, object>> GetServer(CancellationToken cancellationToken = default) {

		SftpClient client = Client();

		return new Dictionary<string, object> {

			// SFTP
			["ProtocolVersion"] = client.ProtocolVersion,

			// Server
			["ServerVersion"] = client.ConnectionInfo.ServerVersion,
			["ClientVersion"] = client.ConnectionInfo.ClientVersion,

			// Encryption
			["ClientEncryption"] = client.ConnectionInfo.CurrentClientEncryption,
			["ServerEncryption"] = client.ConnectionInfo.CurrentServerEncryption,
			["KeyExchangeAlgorithm"] = client.ConnectionInfo.CurrentKeyExchangeAlgorithm,

			// HMAC / Integrity
			["ClientHmacAlgorithm"] = client.ConnectionInfo.CurrentClientHmacAlgorithm,
			["ServerHmacAlgorithm"] = client.ConnectionInfo.CurrentServerHmacAlgorithm,

			// Compression
			["ClientCompressionAlgorithm"] = client.ConnectionInfo.CurrentClientCompressionAlgorithm,
			["ServerCompressionAlgorithm"] = client.ConnectionInfo.CurrentServerCompressionAlgorithm,
		};
	}

	/// <summary>
	/// Creates a new folder on the SFTP server.
	/// </summary>
	/// <param name="folderPath">Path to the new folder.</param>
	public override async Task CreateDirectory(string folderPath, bool force, CancellationToken cancellationToken = default) {

		SftpClient client = Client();

		try {

			// exit quickly if directory exists
			if (await DirectoryExists(folderPath, cancellationToken)) {
				return;
			}

			// create directory part by part
			await EnsureDirectoryExists(folderPath, client, cancellationToken);
		}
		catch (Exception ex) {
			// FIX: no error is thrown if the folder already exists
		}
	}

	/// <summary>
	/// Deletes a folder.
	/// </summary>
	/// <param name="folderPath">Path to the folder.</param>
	/// <param name="recursive">Whether to delete all child files and folders.</param>
	public override async Task DeleteDirectory(string folderPath, bool recursive, CancellationToken cancellationToken = default) {

		SftpClient client = Client();

		if (await DirectoryExists(folderPath, cancellationToken)) {
			if (recursive) {
				await DeleteDirectoryRecursive(client, AddRootDirAndNormalize(folderPath), cancellationToken);
			}
			else {
				await client.DeleteDirectoryAsync(AddRootDirAndNormalize(folderPath), cancellationToken);
			}
		}
	}

	private static async Task DeleteDirectoryRecursive(SftpClient client, string folderPath, CancellationToken cancellationToken = default) {

		await foreach (var entry in client.ListDirectoryAsync(folderPath, cancellationToken)) {

			if (entry.Name == "." || entry.Name == "..")
				continue;

			if (entry.IsDirectory) {
				await DeleteDirectoryRecursive(client, entry.FullName, cancellationToken);
			}
			else {
				await client.DeleteFileAsync(entry.FullName, cancellationToken);
			}
		}

		await client.DeleteDirectoryAsync(folderPath, cancellationToken);
	}

	/// <summary>
	/// Returns true if the specified directory exists on the SFTP server. Returns false if it is a file path.
	/// </summary>
	/// <param name="folderPath">Path to the directory.</param>
	public override async Task<bool> DirectoryExists(string folderPath, CancellationToken cancellationToken = default) {

		SftpClient client = Client();

		folderPath = AddRootDirAndNormalize(folderPath);

		try {
			return (await client.GetAttributesAsync(folderPath, cancellationToken)).IsDirectory;
		}
		catch (SftpPathNotFoundException) {
			return false;
		}
	}

	/// <summary>
	/// Moves a directory to a new location on the SFTP server.
	/// </summary>
	/// <param name="sourceFolderPath">Source directory path.</param>
	/// <param name="destinationFolderPath">Destination directory path.</param>
	public override async Task MoveDirectory(string sourceFolderPath, string destinationFolderPath, CancellationToken cancellationToken = default) {

		SftpClient client = Client();

		sourceFolderPath = AddRootDirAndNormalize(sourceFolderPath);
		destinationFolderPath = AddRootDirAndNormalize(destinationFolderPath);

		await client.RenameFileAsync(sourceFolderPath, destinationFolderPath, cancellationToken);
	}

	/// <summary>
	/// Gets the Unix CHMOD permissions of a file on the SFTP server.
	/// </summary>
	/// <param name="filePath">Path to the file.</param>
	/// <returns>
	/// The file permissions as a numeric CHMOD value (for example, 644 or 755).
	/// </returns>
	public override async Task<int> GetFilePermissions(string filePath, CancellationToken cancellationToken = default) {

		SftpClient client = Client();

		var attributes = await client.GetAttributesAsync(AddRootDirAndNormalize(filePath), cancellationToken);

		// Convert the permission flags to a traditional octal CHMOD value.
		int chmod =
			((attributes.OwnerCanRead ? 4 : 0) + (attributes.OwnerCanWrite ? 2 : 0) + (attributes.OwnerCanExecute ? 1 : 0)) * 100 +
			((attributes.GroupCanRead ? 4 : 0) + (attributes.GroupCanWrite ? 2 : 0) + (attributes.GroupCanExecute ? 1 : 0)) * 10 +
			((attributes.OthersCanRead ? 4 : 0) + (attributes.OthersCanWrite ? 2 : 0) + (attributes.OthersCanExecute ? 1 : 0));

		return chmod;
	}

	/// <summary>
	/// Sets the Unix CHMOD permissions of a file on the SFTP server.
	/// </summary>
	/// <param name="filePath">Path to the file.</param>
	/// <param name="permissions">Permissions as a numeric CHMOD value (for example, 644 or 755).</param>
	public override async Task SetFilePermissions(string filePath, int permissions, CancellationToken cancellationToken = default) {

		SftpClient client = Client();

		client.ChangePermissions(AddRootDirAndNormalize(filePath), (short)permissions);
	}

	/// <summary>
	/// Downloads a file from the SFTP server.
	/// </summary>
	/// <param name="fullPath">Remote file path.</param>
	/// <param name="filePath">Destination path of the local file.</param>
	/// <param name="cancellationToken">Cancellation token.</param>
	public override async Task DownloadObject(string fullPath, string filePath, bool overwrite, CancellationToken cancellationToken = default) {

		// skip if overwriting disabled and local file exists
		if (!overwrite && File.Exists(filePath)) return;

		// exit if remote file doesnt exist
		if (!await ObjectExists(fullPath, cancellationToken)) return;

		// ensure parent directory exists
		string parentDir = Path.GetDirectoryName(filePath);
		if (!string.IsNullOrEmpty(parentDir)) {
			Directory.CreateDirectory(parentDir);
		}

		// download
		SftpClient client = Client();
		using var stream = File.Create(filePath);
		await client.DownloadFileAsync(AddRootDirAndNormalize(fullPath), stream, cancellationToken);
	}

	/// <summary>
	/// Uploads a local file to the SFTP server.
	/// </summary>
	/// <param name="fullPath">Remote file path.</param>
	/// <param name="filePath">Local file path.</param>
	/// <param name="cancellationToken">Cancellation token.</param>
	public override async Task UploadObject(string fullPath, string filePath, bool overwrite, CancellationToken cancellationToken = default) {

		// exit if local file doesnt exist
		if (!File.Exists(filePath)) return;

		// exit if remote file exists and overwriting not wanted
		if (!overwrite && await ObjectExists(fullPath, cancellationToken)) return;

		SftpClient client = Client();

		// First, for speed, let's try to write the file assuming the directory requested already exists
		try {
			using (FileStream stream = File.OpenRead(filePath)) {
				await client.UploadFileAsync(stream, AddRootDirAndNormalize(fullPath), cancellationToken);
			}
			return;
		}
		catch (SftpPathNotFoundException) {
			// If the folder did not exist, continue below.
		}

		// create any non-existing SFTP directories
		await EnsureDirectoryExists(StoragePath.GetParent(fullPath), client, cancellationToken);

		// Retry writing the file
		using (FileStream stream = File.OpenRead(filePath)) {
			await client.UploadFileAsync(stream, AddRootDirAndNormalize(fullPath), cancellationToken);
		}
	}

	/// <summary>
	/// Downloads a file from the SFTP server into a byte array.
	/// </summary>
	/// <param name="fullPath">Remote file path.</param>
	/// <param name="cancellationToken">Cancellation token.</param>
	/// <returns>The contents of the object.</returns>
	public override async Task<byte[]> GetBytes(string fullPath, CancellationToken cancellationToken = default) {

		SftpClient client = Client();

		using MemoryStream stream = new();

		await client.DownloadFileAsync(AddRootDirAndNormalize(fullPath), stream, cancellationToken);

		return stream.ToArray();
	}

	/// <summary>
	/// Uploads a file byte array to the SFTP server.
	/// </summary>
	/// <param name="fullPath">Remote file path.</param>
	/// <param name="data">Data to write.</param>
	/// <param name="append">
	/// <c>true</c> to append to the existing object; otherwise, overwrites the object.
	/// </param>
	/// <param name="cancellationToken">Cancellation token.</param>
	public override async Task SetBytes(string fullPath,byte[] data,bool append = false,
		CancellationToken cancellationToken = default) {

		// exit if invalid data (FIX: allow writing zero byte files)
		if (data == null) return;

		SftpClient client = Client();

		FileMode fileMode = append ? FileMode.Append : FileMode.OpenOrCreate;

		// First, for speed, let's try to write the file assuming the directory requested already exists
		try {
			using (MemoryStream dataStream = new MemoryStream(data)) {
				await SetObjectInternal(dataStream, append, client, AddRootDirAndNormalize(fullPath), fileMode, cancellationToken).ConfigureAwait(false);
			}
			return;
		}
		catch (SftpPathNotFoundException) {
			// If the folder did not exist, continue below.
		}

		// create any non-existing SFTP directories
		await EnsureDirectoryExists(StoragePath.GetParent(fullPath), client, cancellationToken);

		// Retry writing the file.
		using (MemoryStream dataStream = new MemoryStream(data)) {
			await SetObjectInternal(dataStream, append, client, AddRootDirAndNormalize(fullPath), fileMode, cancellationToken).ConfigureAwait(false);
		}
	}


	public override async Task SetObject(string fullPath, Stream dataStream, bool append, CancellationToken cancellationToken = default) {
		await SetObject(fullPath, dataStream, null, append, cancellationToken).ConfigureAwait(false);
	}
	/// <summary>
	/// Uploads data to a file.
	/// </summary>
	/// <param name="fullPath">Remote file path</param>
	/// <param name="dataStream">Stream to upload from</param>
	/// <param name="append">When true, appends to the file instead of writing a new one.</param>
	/// <param name="cancellationToken"></param>
	public override async Task SetObject(string fullPath, Stream dataStream, string contentType, bool append = false, CancellationToken cancellationToken = default) {
		ThrowIfDisposed();

		SftpClient client = Client();
		var fileMode = append ? FileMode.Append : FileMode.OpenOrCreate;

		// First, for speed, let's try to write the file assuming the directory requested already exists
		// [only do this if the input stream is seekable]
		if (dataStream.CanSeek) {
			var origPos = dataStream.Position;
			try {
				// write this stream to SFTP file
				await SetObjectInternal(dataStream, append, client, AddRootDirAndNormalize(fullPath), fileMode, cancellationToken).ConfigureAwait(false);
				return;
			}
			catch (SftpPathNotFoundException) {
				// If the folder did not exist, continue below.
				dataStream.Position = origPos;
			}
		}

		// create any non-existing SFTP directories
		await EnsureDirectoryExists(StoragePath.GetParent(fullPath), client, cancellationToken);

		// write this stream to SFTP file
		await SetObjectInternal(dataStream, append, client, AddRootDirAndNormalize(fullPath), fileMode, cancellationToken).ConfigureAwait(false);

	}

	/// <summary>
	/// Foolproof way to create an entire directory path. Do NOT call the native `CreateDirectory` API as it will only create the last path segment.
	/// </summary>
	private async Task EnsureDirectoryExists(string fullPath, SftpClient client, CancellationToken cancellationToken) {

		// get dir parts
		string[] parts = StoragePath.Split(fullPath);
		string currentFolder = string.Empty;

		// Create any non-existing directories.
		// (recursively check each part and create if it does not exist)
		foreach (string folder in parts) {
			currentFolder = StoragePath.Combine(currentFolder, folder);
			string sftpFolder = AddRootDirAndNormalize(currentFolder);
			try {
				await client.CreateDirectoryAsync(sftpFolder, cancellationToken);
			}
			catch { }
		}
	}

	private async Task SetObjectInternal(Stream dataStream, bool append, SftpClient client, string fullPath, FileMode fileMode, CancellationToken cancellationToken) {
		if (append) {
			using (Stream dest = await client.OpenAsync(AddRootDirAndNormalize(fullPath), fileMode, FileAccess.Write, cancellationToken).ConfigureAwait(false)) {
				await dataStream.CopyToAsync(dest, (int)_transferBufferSize, cancellationToken).ConfigureAwait(false);
			}
		}
		else {
			await client.UploadFileAsync(dataStream, AddRootDirAndNormalize(fullPath), cancellationToken).ConfigureAwait(false);
		}
	}


	/// <summary>
	/// Fastest implementation of getting a checksum using native SFTP/SSH commands.
	/// </summary>
	public override async Task<StorageObjectHash> GetObjectChecksum(string fullPath, StorageHash hash = StorageHash.MD5, CancellationToken cancellationToken = default) {
		if (fullPath == null) throw new ArgumentNullException(nameof(fullPath));

		// compute object checksum remotely [FAST]
		try {

			// if the path is safe to run
			var fullPathWithRoot = AddRootDirAndNormalize(fullPath);
			if (SshSecurityValidator.IsPathSafe(fullPathWithRoot)) {

				// compute a checksum by using SSH shell commands
				var remoteHash = _sshContext.GetRemoteHash(fullPathWithRoot, hash);
				if (remoteHash != null) {

					// convert to common model
					return new StorageObjectHash(fullPath, remoteHash, hash);
				}
			}
		}
		catch (Exception) {
			// absorb all exceptions here, this is just a fast path and not mandatory
		}

		// if not available, download and hash locally [SLOW]
		var bytes = await GetBytes(fullPath, cancellationToken).ConfigureAwait(false);
		if (bytes == null) return null;
		var checksum = HashUtility.HashBytes(bytes, hash);
		return new StorageObjectHash(StoragePath.Normalize(fullPath), checksum, hash);
	}

}
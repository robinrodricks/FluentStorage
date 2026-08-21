using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using FluentFTP;
using FluentFTP.Exceptions;
using FluentStorage.Enums;
using FluentStorage.Exceptions;
using FluentStorage.FTP.Utils;
using FluentStorage.Model;
using FluentStorage.Rules;
using FluentStorage.Storage;
using Polly;
using Polly.Retry;

namespace FluentStorage.FTP.Storage;

/// <summary>
/// Manages a single connected FTP server using FluentFTP. Exclusively async.
/// </summary>
public class FtpStore : StoreBase {
	private readonly AsyncFtpClient _client;
	private readonly bool _dispose;
	private static readonly AsyncRetryPolicy retryPolicy = Policy.Handle<FtpException>().RetryAsync(3);

	public FtpStore(string hostNameOrAddress, NetworkCredential credentials, FtpDataConnectionType dataConnectionType = FtpDataConnectionType.AutoActive) {
		_client = new AsyncFtpClient(hostNameOrAddress, credentials);
		_client.Config.DataConnectionType = dataConnectionType;
		_dispose = true;
	}

	public FtpStore(AsyncFtpClient ftpClient, bool dispose = false) {
		_client = ftpClient ?? throw new ArgumentNullException(nameof(ftpClient));
		_dispose = dispose;
	}

	public override async Task<bool> IsFileSystem() {
		return true;
	}
	/// <summary>
	/// Returns the AsyncFtpClient instance for this store.
	/// </summary>
	public override async Task<object> GetClient() {
		return await Client();
	}
	private async Task<AsyncFtpClient> Client() {
		if (!_client.IsConnected) {
			await _client.Connect().ConfigureAwait(false);

			//not supported on this platform?
			//await _client.SetHashAlgorithmAsync(FtpHashAlgorithm.MD5);
		}

		return _client;
	}

	/// <summary>
	/// List all the files within the given directory (`options.FolderPath`)
	/// and optionally include size and date modified (`options.IncludeAttributes`).
	/// </summary>
	public override async Task<List<StoreObject>> ListObjects(StorageListOptions options = null, CancellationToken cancellationToken = default) {
		AsyncFtpClient client = await Client().ConfigureAwait(false);

		if (options == null)
			options = new StorageListOptions();

		FtpListOption ftpListOption = FtpListOption.Auto;

		if (options.Recurse) {
			ftpListOption |= FtpListOption.Recursive;
		}
		if (options.IncludeAttributes) {
			ftpListOption |= FtpListOption.SizeModify;
		}

		var path = StoragePath.Normalize(options.FolderPath);

		FtpListItem[] items = await client.GetListing(path, ftpListOption, cancellationToken).ConfigureAwait(false);

		List<StoreObject> results = new List<StoreObject>();
		foreach (FtpListItem item in items) {
			if (options.FilePrefix != null && !item.Name.StartsWith(options.FilePrefix)) {
				continue;
			}

			StoreObject blob = ListItemToStoreObject(item);
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

	private StoreObject ListItemToStoreObject(FtpListItem ff) {
		if (ff.Type != FtpObjectType.Directory && ff.Type != FtpObjectType.File)
			return null;

		StoreObject id = new StoreObject(ff.FullName,
			ff.Type == FtpObjectType.File
				? StorageObjectType.File
				: StorageObjectType.Folder);

		if (ff.RawPermissions != null) {
			id.Properties["RawPermissions"] = ff.RawPermissions;
		}
		if (ff.Chmod != 0) {
			id.Properties["Chmod"] = ff.Chmod;
		}
		if (ff.Size != 0) {
			id.Properties["Size"] = ff.Size;
			id.Properties["Modified"] = ff.Modified;
			id.Properties["RawModified"] = ff.RawModified;
		}

		return id;
	}

	public override async Task DeleteObjects(IEnumerable<string> fullPaths, CancellationToken cancellationToken = default) {
		AsyncFtpClient client = await Client().ConfigureAwait(false);

		foreach (string path in fullPaths) {
			await DeleteObject(path, cancellationToken);
		}
	}

	public override async Task DeleteObject(string fullPath, CancellationToken cancellationToken = default) {
		AsyncFtpClient client = await Client().ConfigureAwait(false);

		fullPath = StoragePath.Normalize(fullPath);

		if (await client.FileExists(fullPath, cancellationToken)) {
			await client.DeleteFile(fullPath, cancellationToken).ConfigureAwait(false);
		}
		/*else if (await client.DirectoryExists(fullPath, cancellationToken)) {
			await client.DeleteDirectory(fullPath, FtpListOption.Recursive, cancellationToken).ConfigureAwait(false);
		}*/
	}

	public override async Task<List<bool>> ObjectsExists(IEnumerable<string> paths, CancellationToken cancellationToken = default) {
		AsyncFtpClient client = await Client().ConfigureAwait(false);

		List<bool> results = new List<bool>();
		foreach (string path in paths) {

			var ftpPath = StoragePath.Normalize(path);

			bool e = await client.FileExists(ftpPath).ConfigureAwait(false);
			results.Add(e);
		}

		return results;
	}

	public override async Task<bool> ObjectExists(string fullPath, CancellationToken cancellationToken = default) {
		AsyncFtpClient client = await Client().ConfigureAwait(false);

		fullPath = StoragePath.Normalize(fullPath);

		return await client.FileExists(fullPath).ConfigureAwait(false);
	}

	public override async Task<StoreObject> GetObjectInfo(string path, CancellationToken cancellationToken = default) {
		return (await GetObjectsInfo(new List<string> { path }, cancellationToken).ConfigureAwait(false)).FirstOrDefault();
	}
	public override async Task<List<StoreObject>> GetObjectsInfo(IEnumerable<string> paths, CancellationToken cancellationToken = default) {
		AsyncFtpClient client = await Client().ConfigureAwait(false);

		List<StoreObject> results = new List<StoreObject>();
		foreach (string path in paths) {
			string cpath = StoragePath.Normalize(path);
			string parentPath = StoragePath.GetParent(cpath);

			FtpListItem[] all = await client.GetListing(parentPath, FtpListOption.SizeModify).ConfigureAwait(false);
			FtpListItem foundItem = all.FirstOrDefault(i => i.FullName == cpath);

			if (foundItem == null) {
				results.Add(null);
				continue;
			}

			StoreObject r = new StoreObject(path) {
				Size = foundItem.Size,
				DateModified = foundItem.Modified
			};
			results.Add(r);
		}
		return results;
	}

	/// <summary>
	/// Opens a file for reading and returns its content stream.
	/// </summary>
	public override async Task<Stream> OpenRead(string fullPath, CancellationToken cancellationToken = default) {
		AsyncFtpClient client = await Client().ConfigureAwait(false);

		fullPath = StoragePath.Normalize(fullPath);

		try {
			return await client.OpenRead(fullPath, FtpDataType.Binary, 0, true).ConfigureAwait(false);
		}
		catch (FtpCommandException ex) when (ex.CompletionCode == "550") {
			return null;
		}
	}

	public override async Task<Stream> OpenRange(string fullPath,long offset,long length,CancellationToken cancellationToken = default) {

		fullPath = StoragePath.Normalize(fullPath);

		AsyncFtpClient client = await Client().ConfigureAwait(false);

		Stream stream = await client.OpenRead(fullPath,restart: offset,token: cancellationToken).ConfigureAwait(false);

		return stream;
	}

	public override async Task<bool> IsSeekable() {
		return true;
	}

	public override async Task<long> GetObjectLength(string fullPath, long defaultValue = -1, CancellationToken cancellationToken = default) {
		try {

			fullPath = StoragePath.Normalize(fullPath);

			AsyncFtpClient client = await Client().ConfigureAwait(false);

			return await client.GetFileSize(fullPath, defaultValue, cancellationToken).ConfigureAwait(false);
		}
		catch {
			return defaultValue;
		}
	}

	public override async Task SetObject(string fullPath, Stream dataStream, bool append, CancellationToken cancellationToken = default) {
		await SetObject(fullPath, dataStream, null, append, cancellationToken).ConfigureAwait(false);
	}
	public override async Task SetObject(string fullPath, Stream dataStream, string contentType, bool append = false, CancellationToken cancellationToken = default) {

		fullPath = StoragePath.Normalize(fullPath);

		AsyncFtpClient client = await Client().ConfigureAwait(false);

		await retryPolicy.ExecuteAsync(async () => {
			string directory = Path.GetDirectoryName(fullPath);

			if (!string.IsNullOrWhiteSpace(directory) && !await client.DirectoryExists(directory).ConfigureAwait(false)) {
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

	public override void Dispose() {
		if (_dispose && !_client.IsDisposed) {
			_client.Dispose();
		}
		GC.SuppressFinalize(this);
	}

	/// <summary>
	/// Gets information and capabilities of the connected FTP server.
	/// Returns a `Dictionary` with the following keys: `ServerOS`, `ServerType`, `SystemType`, `Capabilities`.
	/// </summary>
	public override async Task<Dictionary<string, object>> GetServer(CancellationToken cancellationToken = default) {

		AsyncFtpClient client = await Client().ConfigureAwait(false);

		return new Dictionary<string, object> {
			["ServerOS"] = client.ServerOS,
			["ServerType"] = client.ServerType,
			["SystemType"] = client.SystemType,
			["Capabilities"] = client.Capabilities,
		};
	}

	/// <summary>
	/// Creates a new folder on the FTP server.
	/// </summary>
	/// <param name="folderPath">Path to the new folder.</param>
	public override async Task CreateDirectory(string folderPath, bool force, CancellationToken cancellationToken = default) {

		folderPath = StoragePath.Normalize(folderPath);

		AsyncFtpClient client = await Client().ConfigureAwait(false);

		try {
			await client.CreateDirectory(folderPath, force, cancellationToken).ConfigureAwait(false);
		}
		catch (Exception) {
			// FIX: no error is thrown if the folder already exists
		}
	}

	/// <summary>
	/// Deletes a folder on the FTP server.
	/// </summary>
	/// <param name="folderPath">Path to the folder.</param>
	/// <param name="recursive">Whether to delete all child files and folders.</param>
	public override async Task DeleteDirectory(string folderPath, bool recursive, CancellationToken cancellationToken = default) {

		folderPath = StoragePath.Normalize(folderPath);

		AsyncFtpClient client = await Client().ConfigureAwait(false);

		await client.DeleteDirectory(folderPath, recursive ? FtpListOption.Recursive : FtpListOption.Auto, cancellationToken).ConfigureAwait(false);
	}

	/// <summary>
	/// Returns true if the specified directory exists on the FTP server.
	/// </summary>
	/// <param name="folderPath">Path to the directory.</param>
	/// <returns>
	public override async Task<bool> DirectoryExists(string folderPath, CancellationToken cancellationToken = default) {

		folderPath = StoragePath.Normalize(folderPath);

		AsyncFtpClient client = await Client().ConfigureAwait(false);
		return await client.DirectoryExists(folderPath, cancellationToken).ConfigureAwait(false);
	}

	/// <summary>
	/// Moves a directory to a new location on the FTP server.
	/// </summary>
	/// <param name="sourceFolderPath">Source directory path.</param>
	/// <param name="destinationFolderPath">Destination directory path.</param>
	public override async Task MoveDirectory(string sourceFolderPath, string destinationFolderPath, CancellationToken cancellationToken = default) {

		sourceFolderPath = StoragePath.Normalize(sourceFolderPath);
		destinationFolderPath = StoragePath.Normalize(destinationFolderPath);

		AsyncFtpClient client = await Client().ConfigureAwait(false);
		await client.MoveDirectory(sourceFolderPath, destinationFolderPath, FtpRemoteExists.Overwrite, cancellationToken).ConfigureAwait(false);
	}

	/// <summary>
	/// Gets the Unix CHMOD permissions of a file on the FTP server.
	/// </summary>
	/// <param name="filePath">Path to the file.</param>
	/// <returns>
	/// The file permissions as a numeric CHMOD value (for example, 644 or 755).
	/// </returns>
	public override async Task<int> GetFilePermissions(string filePath, CancellationToken cancellationToken = default) {

		filePath = StoragePath.Normalize(filePath);

		AsyncFtpClient client = await Client().ConfigureAwait(false);

		FtpListItem item = await client.GetFilePermissions(filePath, cancellationToken).ConfigureAwait(false);
		return item?.Chmod ?? 0;
	}

	/// <summary>
	/// Sets the Unix CHMOD permissions of a file on the FTP server.
	/// </summary>
	/// <param name="filePath">Path to the file.</param>
	/// <param name="permissions">Permissions as a numeric CHMOD value (for example, 644 or 755).</param>
	public override async Task SetFilePermissions(string filePath, int permissions, CancellationToken cancellationToken = default) {

		filePath = StoragePath.Normalize(filePath);

		AsyncFtpClient client = await Client().ConfigureAwait(false);
		await client.Chmod(filePath, permissions, cancellationToken).ConfigureAwait(false);
	}

	/// <summary>
	/// Downloads a file from the FTP server.
	/// </summary>
	/// <param name="fullPath">Remote file path.</param>
	/// <param name="filePath">Destination path of the local file.</param>
	/// <param name="cancellationToken">Cancellation token.</param>
	public override async Task DownloadObject(string fullPath, string filePath, bool overwrite, CancellationToken cancellationToken = default) {

		// skip if overwriting disabled and local file exists
		if (!overwrite && File.Exists(filePath)) return;

		// exit if remote file doesnt exist
		if (!await ObjectExists(fullPath, cancellationToken)) return;

		// download
		fullPath = StoragePath.Normalize(fullPath);
		AsyncFtpClient client = await Client().ConfigureAwait(false);
		await client.DownloadFile(filePath, fullPath,
			overwrite ? FtpLocalExists.Overwrite : FtpLocalExists.Skip, FtpVerify.None, null, cancellationToken).ConfigureAwait(false);
	}

	/// <summary>
	/// Uploads a local file to the FTP server.
	/// </summary>
	/// <param name="fullPath">Remote file path.</param>
	/// <param name="filePath">Local file path.</param>
	/// <param name="cancellationToken">Cancellation token.</param>
	public override async Task UploadObject(string fullPath, string filePath, bool overwrite, CancellationToken cancellationToken = default) {

		// exit if local file doesnt exist
		if (!File.Exists(filePath)) return;

		// upload
		fullPath = StoragePath.Normalize(fullPath);
		AsyncFtpClient client = await Client().ConfigureAwait(false);
		await client.UploadFile(filePath, fullPath,
			overwrite ? FtpRemoteExists.Overwrite : FtpRemoteExists.Skip, false, FtpVerify.None, null, cancellationToken).ConfigureAwait(false);
	}

	/// <summary>
	/// Downloads a file from the FTP server into a byte array.
	/// </summary>
	/// <param name="fullPath">Remote file path.</param>
	/// <param name="cancellationToken">Cancellation token.</param>
	/// <returns>The contents of the object.</returns>
	public override async Task<byte[]> GetBytes(string fullPath, CancellationToken cancellationToken = default) {

		fullPath = StoragePath.Normalize(fullPath);

		AsyncFtpClient client = await Client().ConfigureAwait(false);

		return await client.DownloadBytes(fullPath, 0).ConfigureAwait(false);
	}

	/// <summary>
	/// Uploads a file byte array to the FTP server.
	/// </summary>
	/// <param name="fullPath">Remote file path.</param>
	/// <param name="data">Data to write.</param>
	/// <param name="append">
	/// <c>true</c> to append to the existing object; otherwise, overwrites the object.
	/// </param>
	/// <param name="cancellationToken">Cancellation token.</param>
	public override async Task SetBytes(string fullPath, byte[] data, bool append = false, CancellationToken cancellationToken = default) {

		// exit if invalid data (FIX: allow writing zero byte files)
		if (data == null) return;

		fullPath = StoragePath.Normalize(fullPath);

		AsyncFtpClient client = await Client().ConfigureAwait(false);

		// FIX: Any file uploads/writes will automatically create the directory structure as required
		var createDirs = true;

		await client.UploadBytes(data, fullPath, FtpRemoteExists.Overwrite, createDirs, null, cancellationToken).ConfigureAwait(false);
	}

	/// <summary>
	/// Rename a file on the FTP server.
	/// </summary>
	/// <param name="oldPath">Current remote path.</param>
	/// <param name="newPath">New remote path.</param>
	/// <param name="cancellationToken">Cancellation token.</param>
	public override async Task<bool> MoveObject(string oldPath, string newPath, bool overwrite, CancellationToken cancellationToken = default) {
		if (string.IsNullOrWhiteSpace(oldPath)) throw new ArgumentNullException(nameof(oldPath));
		if (string.IsNullOrWhiteSpace(newPath)) throw new ArgumentNullException(nameof(newPath));

		oldPath = StoragePath.Normalize(oldPath);
		newPath = StoragePath.Normalize(newPath);

		AsyncFtpClient client = await Client().ConfigureAwait(false);

		return await client.MoveFile(oldPath, newPath,
			overwrite ? FtpRemoteExists.Overwrite : FtpRemoteExists.Skip, cancellationToken).ConfigureAwait(false);
	}

	/// <summary>
	/// Upload a local disk folder onto the FTP server.
	/// </summary>
	public override async Task<List<StorageProgress>> UploadDirectory(string localFolder,string remoteFolder,StorageExists existsMode = StorageExists.Skip,
		Action<StorageProgress>? progress = null, IList<StorageRule> rules = null,CancellationToken cancellationToken = default) {

		if (string.IsNullOrWhiteSpace(localFolder)) throw new ArgumentNullException(nameof(localFolder));
		if (string.IsNullOrWhiteSpace(remoteFolder)) throw new ArgumentNullException(nameof(remoteFolder));
		if (rules != null) throw new Exception("Rules are not yet supported in FTP!");

		// exit if local folder doesnt exist
		if (!Directory.Exists(localFolder)) return new List<StorageProgress>();

		remoteFolder = StoragePath.Normalize(remoteFolder);

		AsyncFtpClient client = await Client().ConfigureAwait(false);

		// manually support the "Throw" mode which FluentFTP does not 
		if (existsMode == StorageExists.Throw) {
			foreach (string file in Directory.EnumerateFiles(localFolder, "*", SearchOption.AllDirectories)) {
				cancellationToken.ThrowIfCancellationRequested();

				string relative = StoragePath.Normalize(StoragePath.GetRelativeDiskPath(localFolder, file));
				string remoteFile = StoragePath.Combine(remoteFolder, relative);

				if (await ObjectExists(remoteFile, cancellationToken).ConfigureAwait(false))
					throw new IOException($"Object '{remoteFile}' already exists.");
			}
		}


		// add a progress handler to FluentFTP call if its given
		IProgress<FtpProgress>? ftpProgress = null;
		if (progress != null) {
			ftpProgress = new Progress<FtpProgress>(p =>{progress(FtpFolderUtils.ConvertProgress(p));});
		}

		// use FluentFTP `UploadDirectory` to handle the entire operation
		await client.UploadDirectory(localFolder,remoteFolder,FtpFolderSyncMode.Update,
			FtpFolderUtils.UploadFolderMap[existsMode],FtpVerify.None,null, ftpProgress, cancellationToken);

		// TODO: support progress objects
		return new List<StorageProgress>();
	}

	/// <summary>
	/// Download a folder from the FTP server to disk.
	/// </summary>
	public override async Task<List<StorageProgress>> DownloadDirectory(string remoteFolder,string localFolder,StorageExists existsMode = StorageExists.Skip,
		Action<StorageProgress>? progress = null, IList<StorageRule> rules = null,CancellationToken cancellationToken = default) {

		if (string.IsNullOrWhiteSpace(localFolder)) throw new ArgumentNullException(nameof(localFolder));
		if (string.IsNullOrWhiteSpace(remoteFolder)) throw new ArgumentNullException(nameof(remoteFolder));
		if (rules != null) throw new Exception("Rules are not yet supported in FTP!");

		remoteFolder = StoragePath.Normalize(remoteFolder);

		AsyncFtpClient client = await Client().ConfigureAwait(false);

		// too inefficient to support the "Throw" mode
		if (existsMode == StorageExists.Throw) {
			throw new StorageException("FluentFTP does not support throwing errors during folder download, so FluentStorage cannot support this feature. Open a ticket if you need it.");
		}


		/*if (existsMode == StorageExistsMode.Throw) {
			List<StoreObject> objects = await ListDirectory(remoteFolder, true, cancellationToken).ConfigureAwait(false);

			foreach (StoreObject obj in objects) {
				cancellationToken.ThrowIfCancellationRequested();

				if (obj.Type != StorageObjectType.File)
					continue;

				string relative = StoragePath.GetRelativePath(remoteFolder, obj.Path);
				string localFile = Path.Combine(localFolder, relative);

				if (File.Exists(localFile))
					throw new IOException($"File '{localFile}' already exists.");
			}
		}*/

		Directory.CreateDirectory(localFolder);

		// add a progress handler to FluentFTP call if its given
		IProgress<FtpProgress>? ftpProgress = null;
		if (progress != null) {
			ftpProgress = new Progress<FtpProgress>(p => { progress(FtpFolderUtils.ConvertProgress(p)); });
		}

		// use FluentFTP `DownloadDirectory` to handle the entire operation
		await client.DownloadDirectory(localFolder,remoteFolder,FtpFolderSyncMode.Update,
			FtpFolderUtils.DownloadFolderMap[existsMode],FtpVerify.None,null, ftpProgress, cancellationToken);

		// TODO: support progress objects
		return new List<StorageProgress>();
	}

	/// <summary>
	/// Fast implementation of getting an object checksum using native FTP commands.
	/// </summary>
	public override async Task<StorageObjectHash> GetObjectChecksum(string fullPath, StorageHash hash = StorageHash.MD5, CancellationToken cancellationToken = default) {
		if (fullPath == null) throw new ArgumentNullException(nameof(fullPath));

		// check if algo supported on FTP
		FtpHashAlgorithm ftpAlgorithm;
		if (!FtpHashUtils.FromFluentStorage.TryGetValue(hash, out ftpAlgorithm)) {
			throw new NotSupportedException($"Hash algorithm {hash} is not supported by FTP.");
		}

		// compute object hash using FluentFTP native API
		fullPath = StoragePath.Normalize(fullPath);
		AsyncFtpClient client = await Client().ConfigureAwait(false);
		FtpHash ftpHash = await client.GetChecksum(fullPath, ftpAlgorithm, cancellationToken).ConfigureAwait(false);

		// exit if hash is invalid
		if (ftpHash == null || !ftpHash.IsValid) return null;

		// convert to common model
		return new StorageObjectHash(fullPath, ftpHash.Value, hash);
	}

}
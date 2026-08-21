using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.ExceptionServices;
using System.Threading;
using System.Threading.Tasks;
using FluentStorage.Enums;
using FluentStorage.Exceptions;
using FluentStorage.Git.Utils;
using FluentStorage.Model;
using FluentStorage.Storage;
using LibGit2Sharp;
using LibGit2Sharp.Handlers;

namespace FluentStorage.Git.Storage;

/// <summary>
/// Manages a git repository as a FluentStorage store. The repository is cloned into a local working directory and
/// all file operations are performed against the working tree. Commits and pushes are performed either automatically
/// (see <see cref="AutoCommit"/>/<see cref="AutoPush"/>) or explicitly via <see cref="GitCommit"/>,
/// <see cref="GitCommitAndPush"/> and <see cref="GitPush"/>.
/// </summary>
public class GitStore : StoreBase {
	private const string AttributesFileExtension = ".attr";

	private readonly GitStorageOptions _options;
	private readonly Repository _repository;
	private readonly string _workingDirectory;
	private readonly string _rootOsPath;
	private readonly bool _deleteLocalOnDispose;
	private readonly Signature _signature;
	private readonly CredentialsHandler _credentialsProvider;
	private readonly SemaphoreSlim _gitLock = new SemaphoreSlim(1, 1);
	private bool _disposed;

	/// <summary>
	/// Creates a new git store by cloning the configured repository.
	/// </summary>
	public GitStore(GitStorageOptions options) {
		_options = options ?? throw new ArgumentNullException(nameof(options));
		if (string.IsNullOrWhiteSpace(_options.Url)) {
			throw new ArgumentException("Repository URL is required.", nameof(options));
		}

		_signature = _options.BuildSignature();
		_credentialsProvider = _options.BuildCredentialsProvider();

		bool isTemp = string.IsNullOrEmpty(_options.LocalWorkingDirectory);
		_workingDirectory = isTemp
			? Path.Combine(Path.GetTempPath(), "FluentStorage.Git", Guid.NewGuid().ToString("N"))
			: Path.GetFullPath(_options.LocalWorkingDirectory);
		_deleteLocalOnDispose = isTemp && _options.DeleteLocalOnDispose;

		Directory.CreateDirectory(_workingDirectory);

		try {
			_repository = OpenOrClone();
		}
		catch (Exception ex) {
			ExceptionDispatchInfo.Capture(GitExceptionMapper.Map(ex)).Throw();
			throw; // unreachable
		}

		_rootOsPath = ResolveRootOsPath();
	}

	/// <summary>
	/// When true, each write operation automatically commits the changes to the repository.
	/// </summary>
	public bool AutoCommit => _options.AutoCommit || _options.AutoPush;

	/// <summary>
	/// When true, each write operation automatically commits and pushes the changes to the remote.
	/// </summary>
	public bool AutoPush => _options.AutoPush;

	/// <summary>
	/// Root sub-folder within the repository that acts as the FluentStorage root. Null means the repository root.
	/// </summary>
	public string RootPath => _options.RootPath;

	/// <summary>
	/// Local working directory of the clone.
	/// </summary>
	public string LocalWorkingDirectory => _workingDirectory;

	// ---------------------------------------------------------------------
	// System
	// ---------------------------------------------------------------------

	/// <inheritdoc />
	public override Task<bool> IsFileSystem() {
		return Task.FromResult(true);
	}

	/// <inheritdoc />
	public override Task<bool> IsSeekable() {
		return Task.FromResult(true);
	}

	/// <inheritdoc />
	public override Task<bool> IsVersioned() {
		return Task.FromResult(true);
	}

	/// <inheritdoc />
	public override Task<object> GetClient() {
		try
		{
			ThrowIfDisposed();
			return Task.FromResult<object>(_repository);
		}
		catch (Exception exception)
		{
			return Task.FromException<object>(exception);
		}
	}

	/// <summary>
	/// Returns information about the connected git repository.
	/// </summary>
	public override async Task<Dictionary<string, object>> GetServer(CancellationToken cancellationToken = default) {
		ThrowIfDisposed();

		await _gitLock.WaitAsync(cancellationToken).ConfigureAwait(false);
		try {
			Commit head = _repository.Head?.Tip;
			Branch branch = _repository.Head;

			return new Dictionary<string, object> {
				["RemoteUrl"] = _options.Url,
				["Branch"] = branch?.FriendlyName,
				["Head"] = head?.Sha,
				["HeadMessage"] = head?.MessageShort,
				["HeadAuthor"] = head?.Author?.Name,
				["HeadCommittedAt"] = head?.Committer.When.DateTime,
				["RootPath"] = _options.RootPath ?? string.Empty,
				["WorkingDirectory"] = _workingDirectory,
				["IsDetached"] = _repository.Info.IsHeadDetached,
			};
		}
		finally {
			_gitLock.Release();
		}
	}

	// ---------------------------------------------------------------------
	// Listing
	// ---------------------------------------------------------------------

	/// <inheritdoc />
	public override Task<List<StoreObject>> ListObjects(StorageListOptions options = null, CancellationToken cancellationToken = default) {
		try
		{
			ThrowIfDisposed();

			if (options == null) {
				options = new StorageListOptions { Recurse = true };
			}

			var result = new List<StoreObject>();

			string targetOsPath = string.IsNullOrEmpty(options.FolderPath) ? _rootOsPath : MapToOsPath(options.FolderPath);

			if (!Directory.Exists(targetOsPath)) {
				return Task.FromResult(result);
			}

			string filePattern = string.IsNullOrEmpty(options.FilePrefix) ? "*" : options.FilePrefix + "*";
			SearchOption searchOption = options.Recurse ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly;

			foreach (string dir in Directory.GetDirectories(targetOsPath, "*", searchOption)) {
				result.Add(ToStoreObject(dir, StorageObjectType.Folder, options.IncludeAttributes));
			}

			foreach (string file in Directory.GetFiles(targetOsPath, filePattern, searchOption)) {
				if (file.EndsWith(AttributesFileExtension)) {
					continue;
				}

				result.Add(ToStoreObject(file, StorageObjectType.File, options.IncludeAttributes));
			}

			if (options.BrowseFilter != null) {
				result = result.Where(i => options.BrowseFilter(i)).ToList();
			}

			if (options.MaxResults != null) {
				result = result.Take(options.MaxResults.Value).ToList();
			}

			return Task.FromResult(result);
		}
		catch (Exception exception)
		{
			return Task.FromException<List<StoreObject>>(exception);
		}
	}

	// ---------------------------------------------------------------------
	// Metadata & Existence
	// ---------------------------------------------------------------------

	/// <inheritdoc />
	public override Task<bool> ObjectExists(string fullPath, CancellationToken cancellationToken = default) {
		try
		{
			ThrowIfDisposed();
			if (fullPath == null) {
				return Task.FromResult(false);
			}

			return Task.FromResult(File.Exists(MapToOsPath(fullPath)));
		}
		catch (Exception exception)
		{
			return Task.FromException<bool>(exception);
		}
	}

	/// <inheritdoc />
	public override Task<List<bool>> ObjectsExists(IEnumerable<string> fullPaths, CancellationToken cancellationToken = default) {
		try
		{
			ThrowIfDisposed();
			return Task.FromResult(fullPaths.Select(p => p != null && File.Exists(MapToOsPath(p))).ToList());
		}
		catch (Exception exception)
		{
			return Task.FromException<List<bool>>(exception);
		}
	}

	/// <inheritdoc />
	public override async Task<StoreObject> GetObjectInfo(string path, CancellationToken cancellationToken = default) {
		return (await GetObjectsInfo(new List<string> { path }, cancellationToken).ConfigureAwait(false)).FirstOrDefault();
	}

	/// <inheritdoc />
	public override Task<List<StoreObject>> GetObjectsInfo(IEnumerable<string> paths, CancellationToken cancellationToken = default) {
		try
		{
			ThrowIfDisposed();

			var result = new List<StoreObject>();
			foreach (string path in paths) {
				if (path == null) {
					throw new ArgumentNullException(nameof(path));
				}

				string osPath = MapToOsPath(path);
				if (!File.Exists(osPath)) {
					result.Add(null);
					continue;
				}

				result.Add(ToStoreObject(osPath, StorageObjectType.File, true));
			}

			return Task.FromResult(result);
		}
		catch (Exception exception)
		{
			return Task.FromException<List<StoreObject>>(exception);
		}
	}

	/// <inheritdoc />
	public override async Task SetObjectInfo(StoreObject obj, CancellationToken cancellationToken = default) {
		await SetObjectsInfo(new List<StoreObject> { obj }, cancellationToken).ConfigureAwait(false);
	}

	/// <inheritdoc />
	public override async Task SetObjectsInfo(IEnumerable<StoreObject> blobs, CancellationToken cancellationToken = default) {
		ThrowIfDisposed();

		foreach (StoreObject blob in blobs.Where(b => b != null)) {
			string osPath = MapToOsPath(blob.FullPath);
			if (!File.Exists(osPath) || blob.Metadata == null) {
				continue;
			}

			string attrPath = osPath + AttributesFileExtension;
			await File.WriteAllBytesAsync(attrPath, blob.AttributesToByteArray(), cancellationToken);
		}

		await MaybeCommitAsync().ConfigureAwait(false);
	}

	// ---------------------------------------------------------------------
	// Read
	// ---------------------------------------------------------------------

	/// <inheritdoc />
	public override Task<Stream> OpenRead(string fullPath, CancellationToken cancellationToken = default) {
		try
		{
			ThrowIfDisposed();
			if (fullPath == null) {
				throw new ArgumentNullException(nameof(fullPath));
			}

			string osPath = MapToOsPath(fullPath);
			if (!File.Exists(osPath)) {
				return Task.FromResult<Stream>(null);
			}

			return Task.FromResult<Stream>(new FileStream(osPath, FileMode.Open, FileAccess.Read, FileShare.Read));
		}
		catch (Exception exception)
		{
			return Task.FromException<Stream>(exception);
		}
	}

	/// <inheritdoc />
	public override Task<Stream> OpenRange(string fullPath, long offset, long length, CancellationToken cancellationToken = default) {
		try
		{
			ThrowIfDisposed();
			if (fullPath == null) {
				throw new ArgumentNullException(nameof(fullPath));
			}

			string osPath = MapToOsPath(fullPath);
			if (!File.Exists(osPath)) {
				return Task.FromResult<Stream>(null);
			}

			FileStream stream = new FileStream(osPath, FileMode.Open, FileAccess.Read, FileShare.Read);
			stream.Seek(offset, SeekOrigin.Begin);
			return Task.FromResult<Stream>(stream);
		}
		catch (Exception exception)
		{
			return Task.FromException<Stream>(exception);
		}
	}

	/// <inheritdoc />
	public override Task<long> GetObjectLength(string fullPath, long defaultValue = -1, CancellationToken cancellationToken = default) {
		try
		{
			try {
				ThrowIfDisposed();

				string osPath = MapToOsPath(fullPath);
				if (!File.Exists(osPath)) {
					return Task.FromResult(defaultValue);
				}

				return Task.FromResult(new FileInfo(osPath).Length);
			}
			catch {
				return Task.FromResult(defaultValue);
			}
		}
		catch (Exception exception)
		{
			return Task.FromException<long>(exception);
		}
	}

	// ---------------------------------------------------------------------
	// Write
	// ---------------------------------------------------------------------

	/// <inheritdoc />
	public override async Task SetObject(string fullPath, Stream dataStream, bool append, CancellationToken cancellationToken = default) {
		await SetObject(fullPath, dataStream, null, append, cancellationToken).ConfigureAwait(false);
	}

	/// <inheritdoc />
	public override async Task SetObject(string fullPath, Stream dataStream, string contentType, bool append = false, CancellationToken cancellationToken = default) {
		ThrowIfDisposed();
		if (dataStream == null) {
			throw new ArgumentNullException(nameof(dataStream));
		}
		if (fullPath == null) {
			throw new ArgumentNullException(nameof(fullPath));
		}

		string osPath = MapToOsPath(fullPath);
		string dir = Path.GetDirectoryName(osPath);
		if (!string.IsNullOrEmpty(dir)) {
			Directory.CreateDirectory(dir);
		}

		await using (Stream dest = append
			             ? new FileStream(osPath, FileMode.Append, FileAccess.Write)
			             : File.Create(osPath)) {
			await dataStream.CopyToAsync(dest, cancellationToken).ConfigureAwait(false);
		}

		await MaybeCommitAsync().ConfigureAwait(false);
	}

	/// <inheritdoc />
	public override Task<Stream> OpenWrite(string fullPath, bool overwrite, CancellationToken cancellationToken = default) {
		try
		{
			ThrowIfDisposed();
			if (fullPath == null) {
				throw new ArgumentNullException(nameof(fullPath));
			}

			string osPath = MapToOsPath(fullPath);
			string dir = Path.GetDirectoryName(osPath);
			if (!string.IsNullOrEmpty(dir)) {
				Directory.CreateDirectory(dir);
			}

			if (!overwrite && File.Exists(osPath)) {
				return Task.FromResult<Stream>(null);
			}

			Stream inner = File.Create(osPath);
			return Task.FromResult<Stream>(new GitWriteStream(inner, MaybeCommitSync));
		}
		catch (Exception exception)
		{
			return Task.FromException<Stream>(exception);
		}
	}

	// ---------------------------------------------------------------------
	// Object Manipulation
	// ---------------------------------------------------------------------

	/// <inheritdoc />
	public override async Task DeleteObject(string fullPath, CancellationToken cancellationToken = default) {
		ThrowIfDisposed();
		if (fullPath == null) {
			return;
		}

		string osPath = MapToOsPath(fullPath);
		if (File.Exists(osPath)) {
			File.Delete(osPath);
		}

		string attrPath = osPath + AttributesFileExtension;
		if (File.Exists(attrPath)) {
			File.Delete(attrPath);
		}

		await MaybeCommitAsync().ConfigureAwait(false);
	}

	/// <inheritdoc />
	public override async Task DeleteObjects(IEnumerable<string> fullPaths, CancellationToken cancellationToken = default) {
		ThrowIfDisposed();
		if (fullPaths == null) {
			return;
		}

		foreach (string fullPath in fullPaths) {
			if (fullPath == null) {
				continue;
			}

			string osPath = MapToOsPath(fullPath);
			if (File.Exists(osPath)) {
				File.Delete(osPath);
			}

			string attrPath = osPath + AttributesFileExtension;
			if (File.Exists(attrPath)) {
				File.Delete(attrPath);
			}
		}

		await MaybeCommitAsync().ConfigureAwait(false);
	}

	/// <inheritdoc />
	public override async Task<bool> MoveObject(string oldPath, string newPath, bool overwrite, CancellationToken cancellationToken = default) {
		ThrowIfDisposed();
		if (string.IsNullOrWhiteSpace(oldPath)) {
			throw new ArgumentNullException(nameof(oldPath));
		}
		if (string.IsNullOrWhiteSpace(newPath)) {
			throw new ArgumentNullException(nameof(newPath));
		}

		string source = MapToOsPath(oldPath);
		string destination = MapToOsPath(newPath);

		if (!File.Exists(source)) {
			return false;
		}

		if (File.Exists(destination)) {
			if (!overwrite) {
				return false;
			}
			File.Delete(destination);
		}

		string destDir = Path.GetDirectoryName(destination);
		if (!string.IsNullOrEmpty(destDir)) {
			Directory.CreateDirectory(destDir);
		}

		File.Move(source, destination);

		string sourceAttr = source + AttributesFileExtension;
		if (File.Exists(sourceAttr)) {
			File.Move(sourceAttr, destination + AttributesFileExtension);
		}

		await MaybeCommitAsync().ConfigureAwait(false);
		return true;
	}

	// ---------------------------------------------------------------------
	// Directory
	// ---------------------------------------------------------------------

	/// <inheritdoc />
	public override async Task CreateDirectory(string folderPath, bool force, CancellationToken cancellationToken = default) {
		ThrowIfDisposed();
		if (folderPath == null) {
			throw new ArgumentNullException(nameof(folderPath));
		}

		string osPath = MapToOsPath(folderPath);
		if (Directory.Exists(osPath)) {
			return;
		}

		Directory.CreateDirectory(osPath);
		await Task.CompletedTask.ConfigureAwait(false);
	}

	/// <inheritdoc />
	public override async Task DeleteDirectory(string folderPath, bool recursive, CancellationToken cancellationToken = default) {
		ThrowIfDisposed();
		if (folderPath == null) {
			throw new ArgumentNullException(nameof(folderPath));
		}

		string osPath = MapToOsPath(folderPath);
		if (Directory.Exists(osPath)) {
			Directory.Delete(osPath, recursive);
		}

		await MaybeCommitAsync().ConfigureAwait(false);
	}

	/// <inheritdoc />
	public override Task<bool> DirectoryExists(string folderPath, CancellationToken cancellationToken = default) {
		try
		{
			ThrowIfDisposed();
			if (folderPath == null) {
				throw new ArgumentNullException(nameof(folderPath));
			}

			return Task.FromResult(Directory.Exists(MapToOsPath(folderPath)));
		}
		catch (Exception exception)
		{
			return Task.FromException<bool>(exception);
		}
	}

	/// <inheritdoc />
	public override async Task MoveDirectory(string sourceFolderPath, string destinationFolderPath, CancellationToken cancellationToken = default) {
		ThrowIfDisposed();
		if (sourceFolderPath == null) {
			throw new ArgumentNullException(nameof(sourceFolderPath));
		}
		if (destinationFolderPath == null) {
			throw new ArgumentNullException(nameof(destinationFolderPath));
		}

		string source = MapToOsPath(sourceFolderPath);
		string destination = MapToOsPath(destinationFolderPath);

		if (Directory.Exists(source))
			Directory.Move(source, destination);

		await MaybeCommitAsync().ConfigureAwait(false);
	}

	// ---------------------------------------------------------------------
	// Versioning (mapped to git history)
	// ---------------------------------------------------------------------

	/// <inheritdoc />
	public override async Task<List<StorageObjectVersion>> ListObjectVersions(string objectPath, CancellationToken cancellationToken = default) {
		ThrowIfDisposed();
		if (objectPath == null) throw new ArgumentNullException(nameof(objectPath));

		string repoPath = MapToRepoPath(objectPath);

		await _gitLock.WaitAsync(cancellationToken).ConfigureAwait(false);
		try {
			var result = new List<StorageObjectVersion>();
			string headSha = _repository.Head?.Tip?.Sha;

			foreach (Commit commit in _repository.Commits) {
				TreeEntry entry = commit[repoPath];
				if (entry == null) continue;

				Blob blob = entry.Target as Blob;
				if (blob == null) continue;

				Commit parent = commit.Parents.FirstOrDefault();
				if (parent != null && parent[repoPath]?.Target?.Sha == blob.Sha)
					continue;

				result.Add(new StorageObjectVersion {
					VersionId = commit.Sha,
					IsCurrent = commit.Sha == headSha,
					DateCreated = commit.Committer.When.DateTime,
					Length = blob.Size,
					ETag = blob.Sha
				});
			}

			return result;
		}
		catch (Exception ex) {
			ExceptionDispatchInfo.Capture(GitExceptionMapper.Map(ex)).Throw();
			throw;
		}
		finally {
			_gitLock.Release();
		}
	}

	/// <inheritdoc />
	public override async Task<StorageObjectVersion> GetObjectVersion(string objectPath, string versionId, CancellationToken cancellationToken = default) {
		ThrowIfDisposed();
		if (objectPath == null) throw new ArgumentNullException(nameof(objectPath));
		if (versionId == null) throw new ArgumentNullException(nameof(versionId));

		string repoPath = MapToRepoPath(objectPath);

		await _gitLock.WaitAsync(cancellationToken).ConfigureAwait(false);
		try {
			Commit commit = _repository.Lookup<Commit>(versionId);
			if (commit == null) return null;

			Blob blob = LookupBlob(commit, repoPath);
			if (blob == null) return null;

			return new StorageObjectVersion {
				VersionId = versionId,
				DateCreated = commit.Committer.When.DateTime,
				Length = blob.Size,
				ETag = blob.Sha
			};
		}
		catch (Exception ex) {
			ExceptionDispatchInfo.Capture(GitExceptionMapper.Map(ex)).Throw();
			throw;
		}
		finally {
			_gitLock.Release();
		}
	}

	/// <inheritdoc />
	public override async Task<bool> RestoreObjectVersion(string objectPath, string versionId, CancellationToken cancellationToken = default) {
		ThrowIfDisposed();
		if (objectPath == null) throw new ArgumentNullException(nameof(objectPath));
		if (versionId == null) throw new ArgumentNullException(nameof(versionId));

		string repoPath = MapToRepoPath(objectPath);
		string osPath = MapToOsPath(objectPath);

		await _gitLock.WaitAsync(cancellationToken).ConfigureAwait(false);
		try {
			Commit commit = _repository.Lookup<Commit>(versionId);
			if (commit == null) return false;

			Blob blob = LookupBlob(commit, repoPath);
			if (blob == null) return false;

			string dir = Path.GetDirectoryName(osPath);
			if (!string.IsNullOrEmpty(dir)) {
				Directory.CreateDirectory(dir);
			}

			await using (Stream content = blob.GetContentStream())
			await using (Stream dest = File.Create(osPath)) {
				await content.CopyToAsync(dest, cancellationToken);
			}
		}
		catch (Exception ex) {
			ExceptionDispatchInfo.Capture(GitExceptionMapper.Map(ex)).Throw();
			throw;
		}
		finally {
			_gitLock.Release();
		}

		await MaybeCommitAsync().ConfigureAwait(false);
		return true;
	}

	/// <inheritdoc />
	public override Task<bool> DeleteObjectVersion(string objectPath, string versionId, CancellationToken cancellationToken = default) {
		// Git history is immutable; deleting a single historical version is not supported without rewriting history.
		return Task.FromResult(false);
	}

	// ---------------------------------------------------------------------
	// Git specific operations (not part of IStore)
	// ---------------------------------------------------------------------

	/// <summary>
	/// Returns the current HEAD commit, or null if the repository has no commits.
	/// </summary>
	public Commit GetCurrentCommit() {
		ThrowIfDisposed();
		return _repository.Head?.Tip;
	}

	/// <summary>
	/// Stages all pending changes under the store's root and commits them.
	/// When <see cref="GitStorageOptions.PullBeforeWrite"/> is true, the remote is pulled (best effort) before committing.
	/// Returns the created commit, or null if there was nothing to commit.
	/// </summary>
	public async Task<Commit> GitCommit(string message = null, CancellationToken cancellationToken = default) {
		ThrowIfDisposed();

		return await Task.Run(() => {
			_gitLock.Wait(cancellationToken);
			try {
				return CommitCore(message);
			}
			catch (Exception ex) {
				ExceptionDispatchInfo.Capture(GitExceptionMapper.Map(ex)).Throw();
				throw;
			}
			finally {
				_gitLock.Release();
			}
		}, cancellationToken).ConfigureAwait(false);
	}

	/// <summary>
	/// Commits all pending changes and pushes them to the remote.
	/// </summary>
	public async Task GitCommitAndPush(string message = null, CancellationToken cancellationToken = default) {
		await GitCommit(message, cancellationToken).ConfigureAwait(false);
		await GitPush(cancellationToken).ConfigureAwait(false);
	}

	/// <summary>
	/// Pushes the current branch to its remote.
	/// </summary>
	public async Task GitPush(CancellationToken cancellationToken = default) {
		ThrowIfDisposed();

		await Task.Run(() => {
			_gitLock.Wait(cancellationToken);
			try {
				PushCore();
			}
			catch (Exception ex) {
				ExceptionDispatchInfo.Capture(GitExceptionMapper.Map(ex)).Throw();
				throw;
			}
			finally {
				_gitLock.Release();
			}
		}, cancellationToken).ConfigureAwait(false);
	}

	/// <summary>
	/// Pulls the latest changes from the remote into the current branch.
	/// </summary>
	public async Task GitPull(CancellationToken cancellationToken = default) {
		ThrowIfDisposed();

		await Task.Run(() => {
			_gitLock.Wait(cancellationToken);
			try {
				PullCore();
			}
			catch (Exception ex) {
				ExceptionDispatchInfo.Capture(GitExceptionMapper.Map(ex)).Throw();
				throw;
			}
			finally {
				_gitLock.Release();
			}
		}, cancellationToken).ConfigureAwait(false);
	}

	// ---------------------------------------------------------------------
	// Internals
	// ---------------------------------------------------------------------

	private Repository OpenOrClone() {
		if (Repository.IsValid(_workingDirectory)) {
			return new Repository(_workingDirectory);
		}

		CloneOptions cloneOptions = _options.CloneOptions ?? new CloneOptions();
		ApplyFetchOptions(cloneOptions.FetchOptions);
		if (!string.IsNullOrEmpty(_options.Branch))
			cloneOptions.BranchName = _options.Branch;

		Repository.Clone(_options.Url, _workingDirectory, cloneOptions);
		return new Repository(_workingDirectory);
	}

	private string ResolveRootOsPath() {
		if (string.IsNullOrEmpty(_options.RootPath))
			return _workingDirectory;

		string root = StoragePath.Normalize(_options.RootPath).Replace('/', Path.DirectorySeparatorChar);
		return Path.GetFullPath(Path.Combine(_workingDirectory, root));
	}

	private string MapToOsPath(string fullPath) {
		string normalized = StoragePath.Normalize(fullPath);
		string osPath = normalized.Length == 0
			? _rootOsPath
			: Path.GetFullPath(Path.Combine(_rootOsPath, normalized.Replace('/', Path.DirectorySeparatorChar)));

		EnsureWithinWorkingDirectory(osPath);
		return osPath;
	}

	private string MapToRepoPath(string fullPath) {
		string normalized = StoragePath.Normalize(fullPath);
		return string.IsNullOrEmpty(_options.RootPath) ? normalized : StoragePath.Combine(_options.RootPath, normalized);
	}

	private void EnsureWithinWorkingDirectory(string osPath) {
		string workingDirectory = Path.GetFullPath(_workingDirectory);

		if (!osPath.Equals(workingDirectory, StringComparison.Ordinal) &&
		    !osPath.StartsWith(workingDirectory + Path.DirectorySeparatorChar, StringComparison.Ordinal)) {
			throw new StorageException($"Path '{osPath}' resolves outside of the repository working directory.");
		}
	}

	private StoreObject ToStoreObject(string osPath, StorageObjectType kind, bool includeMeta) {
		string relativePath = StoragePath.Normalize(osPath.Substring(_rootOsPath.Length));

		var obj = new StoreObject(relativePath, kind);

		if (kind == StorageObjectType.File) {
			var fi = new FileInfo(osPath);
			obj.Size = fi.Length;
			obj.DateModified = new DateTimeOffset(fi.LastWriteTimeUtc);
			obj.DateCreated = new DateTimeOffset(fi.CreationTimeUtc);
		}
		else {
			var di = new DirectoryInfo(osPath);
			obj.DateModified = new DateTimeOffset(di.LastWriteTimeUtc);
			obj.DateCreated = new DateTimeOffset(di.CreationTimeUtc);
		}

		if (includeMeta)
			AddMetadata(obj);

		return obj;
	}

	private void AddMetadata(StoreObject blob) {
		string attrPath = MapToOsPath(blob.FullPath) + AttributesFileExtension;
		if (!File.Exists(attrPath)) return;

		try {
			byte[] content = File.ReadAllBytes(attrPath);
			blob.AppendAttributesFromByteArray(content);
		}
		catch (IOException) {
			// file is locked or inaccessible; ignore
		}
	}

	private void MaybeCommitSync() {
		if (!AutoCommit) return;

		_gitLock.Wait();
		try {
			CommitCore(_options.DefaultCommitMessage);
		}
		catch (Exception ex) {
			ExceptionDispatchInfo.Capture(GitExceptionMapper.Map(ex)).Throw();
			throw;
		}
		finally {
			_gitLock.Release();
		}
	}

	private async Task MaybeCommitAsync() {
		if (!AutoCommit) return;

		await GitCommit(_options.DefaultCommitMessage).ConfigureAwait(false);

		if (AutoPush)
			await GitPush().ConfigureAwait(false);
	}

	private Commit CommitCore(string message) {
		if (message == null)
			message = _options.DefaultCommitMessage;

		if (_options.PullBeforeWrite) {
			// best effort pull to avoid committing on top of a stale history
			try {
				PullCore();
			}
			catch {
				// offline or conflict; continue with a local commit
			}
		}

		StageAllChanges();

		bool hasChanges;
		Tree headTree = _repository.Head?.Tip?.Tree;
		if (headTree == null) {
			hasChanges = _repository.Index.Count > 0;
		}
		else {
			using TreeChanges changes = _repository.Diff.Compare<TreeChanges>(headTree, DiffTargets.Index);
			hasChanges = changes.Any(c => c.Status != ChangeKind.Unmodified);
		}

		if (!hasChanges)
			return null;

		return _repository.Commit(message, _signature, _signature);
	}

	private void PushCore() {
		if (_repository.Head == null) return;

		PushOptions pushOptions = BuildPushOptions();
		_repository.Network.Push(_repository.Head, pushOptions);
	}

	private void PullCore() {
		if (_repository.Head == null) return;

		PullOptions pullOptions = new PullOptions {
			FetchOptions = BuildFetchOptions()
		};

		Commands.Pull(_repository, _signature, pullOptions);
	}

	private void StageAllChanges() {
		RepositoryStatus status = _repository.RetrieveStatus();

		var paths = new List<string>();
		foreach (StatusEntry entry in status) {
			string path = entry.FilePath;

			if (!string.IsNullOrEmpty(_options.RootPath) && !IsUnderRoot(path))
				continue;

			if (IsStageable(entry.State))
				paths.Add(path);
		}

		if (paths.Count > 0)
			Commands.Stage(_repository, paths);
	}

	private bool IsUnderRoot(string repoPath) {
		string root = StoragePath.Normalize(_options.RootPath);
		if (repoPath == root) return true;

		return repoPath.StartsWith(root + StoragePath.PathSeparator, StringComparison.Ordinal);
	}

	private static bool IsStageable(FileStatus state) {
		switch (state) {
			case FileStatus.Unaltered:
			case FileStatus.Ignored:
			case FileStatus.Nonexistent:
			case FileStatus.Unreadable:
				return false;
			default:
				return true;
		}
	}

	private FetchOptions BuildFetchOptions() {
		FetchOptions fetchOptions = _options.FetchOptions ?? new FetchOptions();
		ApplyFetchOptions(fetchOptions);
		return fetchOptions;
	}

	private void ApplyFetchOptions(FetchOptions fetchOptions) {
		if (fetchOptions.CredentialsProvider == null)
			fetchOptions.CredentialsProvider = _credentialsProvider;

		if (fetchOptions.CertificateCheck == null && _options.CertificateCheck != null)
			fetchOptions.CertificateCheck = _options.CertificateCheck;
	}

	private PushOptions BuildPushOptions() {
		PushOptions pushOptions = _options.PushOptions ?? new PushOptions();

		if (pushOptions.CredentialsProvider == null)
			pushOptions.CredentialsProvider = _credentialsProvider;

		if (pushOptions.CertificateCheck == null && _options.CertificateCheck != null)
			pushOptions.CertificateCheck = _options.CertificateCheck;

		return pushOptions;
	}

	private static Blob LookupBlob(Commit commit, string repoPath) {
		TreeEntry entry = commit[repoPath];
		return entry?.Target as Blob;
	}

	/// <inheritdoc />
	public override void Dispose() {
		if (_disposed) return;
		_disposed = true;

		try {
			_repository?.Dispose();
		}
		catch {
			// ignore dispose errors
		}

		_gitLock.Dispose();

		if (_deleteLocalOnDispose) {
			try {
				if (Directory.Exists(_workingDirectory))
					Directory.Delete(_workingDirectory, true);
			}
			catch {
				// ignore cleanup errors
			}
		}

		GC.SuppressFinalize(this);
	}

	private void ThrowIfDisposed() {
		if (_disposed)
			throw new ObjectDisposedException(GetType().FullName);
	}
}
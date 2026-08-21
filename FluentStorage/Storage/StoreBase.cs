using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using FluentStorage.Enums;
using FluentStorage.Exceptions;
using FluentStorage.Model;
using FluentStorage.Rules;
using FluentStorage.Rules.Engine;
using FluentStorage.Streaming;
using FluentStorage.Utils.Extensions;
using FluentStorage.Utils.Hashing;

namespace FluentStorage.Storage;

/// <summary>
/// The base class used for all FluentStorage stores, including cloud buckets and disk-type stores.
/// It implements many high level API methods for file manipulation, upload and download.
/// It helps the provider-specific stores provide concrete implementations for the vast API of IStore.
/// </summary>
public abstract class StoreBase : IStore {

	private const int BufferSize = 81920;

	public virtual void Dispose() {

	}

	/// <summary>Checks whether the storage is a file system.</summary>
	public virtual Task<bool> IsFileSystem() {
		return Task.FromResult(false);
	}

	/// <summary>Checks whether the storage supports seeking.</summary>
	public virtual Task<bool> IsSeekable() {
		return Task.FromResult(false);
	}

	/// <summary>Checks whether the storage supports versioning.</summary>
	public virtual Task<bool> IsVersioned() {
		return Task.FromResult(false);
	}

	/// <summary>Checks whether the storage supports tags.</summary>
	public virtual Task<bool> IsTagged() {
		return Task.FromResult(false);
	}

	/// <summary>Checks whether the storage supports tiers.</summary>
	public virtual Task<bool> IsTiered() {
		return Task.FromResult(false);
	}

	/// <summary>Gets the underlying storage client.</summary>
	public virtual Task<object> GetClient() {
		return Task.FromResult<object>(null);
	}

	public virtual Task<Stream> OpenRead(string objectPath, CancellationToken cancellationToken = default) {
		throw new NotSupportedException();
	}

	public virtual Task<Stream> OpenWrite(string objectPath, bool overwrite, CancellationToken cancellationToken = default) {
		throw new NotSupportedException();
	}

	public virtual Task<Stream> OpenRange(string path, long offset, long length, CancellationToken cancellationToken = default) {
		throw new NotSupportedException();
	}

	/// <summary>
	/// Opens an object as a seekable stream when supported.
	/// </summary>
	public virtual async Task<SeekableStream> OpenSeekable(string path, int bufferSize = 65536, CancellationToken cancellationToken = default) {
		try {
			if (!await IsSeekable()) return null;
			if (!await ObjectExists(path, cancellationToken)) return null;

			var length = await GetObjectLength(path, -1, cancellationToken);

			// FIX #151: do not return a SeekableStream if the object length cannot be determined
			// because all streaming players require knowing the length of the object
			if (length == -1) return null;

			return new SeekableStream(this, path, bufferSize, length);
		}
		catch (Exception) {

			// silently absorb any wierd exceptions and just return null in case the stream cannot be created
			return null;
		}
	}

	public virtual Task<long> GetObjectLength(string fullPath, long defaultValue = -1, CancellationToken cancellationToken = default) {
		return Task.FromResult(defaultValue);
	}

	public virtual async Task<List<StoreObject>> ListObjects(StorageListOptions options = null, CancellationToken cancellationToken = default) {
		var result = new List<StoreObject>();
		if (options == null) options = new StorageListOptions();

		await ListInternal(options.FolderPath, options, result, cancellationToken).ConfigureAwait(false);

		if (options.MaxResults != null && result.Count > options.MaxResults.Value) {
			result = result.Take(options.MaxResults.Value).ToList();
		}

		return result;
	}

	/// <summary>
	/// Returns the list of available files, excluding folders.
	/// </summary>
	public virtual async Task<List<StoreObject>> ListFileObjects(StorageListOptions options,
		CancellationToken cancellationToken = default) {
		List<StoreObject> all = await ListObjects(options, cancellationToken).ConfigureAwait(false);

		return all.Where(i => i != null && i.IsFile).ToList();
	}

	protected virtual Task<List<StoreObject>> ListPath(
		string path, StorageListOptions options, CancellationToken cancellationToken = default) {
		throw new NotSupportedException();
	}

	protected virtual async Task ListInternal(string path, StorageListOptions options, List<StoreObject> container, CancellationToken cancellationToken = default) {
		List<StoreObject> chunk = await ListPath(path, options, cancellationToken).ConfigureAwait(false);

		if (options.BrowseFilter != null) {
			container.AddRange(chunk.Where(b => options.BrowseFilter(b)));
		}
		else {
			container.AddRange(chunk);
		}

		if (options.MaxResults != null && container.Count >= options.MaxResults.Value)
			return;

		if ((this is IStore) && options.Recurse) {
			await Task.WhenAll(
				chunk.Where(c => c.IsFolder).ToList()
					.Select(c => ListInternal(c.FullPath, options, container, cancellationToken))).ConfigureAwait(false);
		}
	}


	public virtual async Task SetObject(string objectPath, Stream sourceStream, bool append, CancellationToken cancellationToken = default) {
		await SetObject(objectPath, sourceStream, null, append, cancellationToken).ConfigureAwait(false);
	}

	public virtual Task SetObject(string objectPath, Stream dataStream, string contentType, bool append, CancellationToken cancellationToken = default) {
		throw new NotImplementedException();
	}

	/// <summary>
	/// Returns the list of objects in a specific directory of this bucket.
	/// </summary>
	/// <param name="folderPath">Remote folder path or virtual folder path to list</param>
	/// <param name="recurse">Recurse into sub folders?</param>
	/// <returns>List of remote object paths</returns>
	public virtual async Task<List<StoreObject>> ListDirectory(string folderPath, bool recurse, CancellationToken cancellationToken = default) {
		var options = new StorageListOptions();
		options.FolderPath = folderPath;
		options.Recurse = recurse;
		return await ListObjects(options, cancellationToken).ConfigureAwait(false);
	}


	/// <summary>
	/// Returns the list of available blobs
	/// </summary>
	/// <param name="folderPath"><see cref="StorageListOptions.FolderPath"/></param>
	/// <param name="browseFilter"><see cref="StorageListOptions.BrowseFilter"/></param>
	/// <param name="filePrefix"><see cref="StorageListOptions.FilePrefix"/></param>
	/// <param name="recurse"><see cref="StorageListOptions.Recurse"/></param>
	/// <param name="recursionMode"><see cref="StorageListOptions.RecursionMode"/></param>
	/// <param name="numberOfRecursionThreads"><see cref="StorageListOptions.NumberOfRecursionThreads"/></param>
	/// <param name="maxResults"><see cref="StorageListOptions.MaxResults"/></param>
	/// <param name="includeAttributes"><see cref="StorageListOptions.IncludeAttributes"/></param>
	public virtual async Task<List<StoreObject>> ListDirectory(string folderPath = null,
		Func<StoreObject, bool> browseFilter = null,
		string filePrefix = null,
		bool recurse = false,
		StorageRecursion recursionMode = StorageRecursion.Remote,
		int numberOfRecursionThreads = StorageListOptions.MAX_THREADS,
		int? maxResults = null,
		bool includeAttributes = false,
		CancellationToken cancellationToken = default) {
		var options = new StorageListOptions();
		if (folderPath != null)
			options.FolderPath = folderPath;
		if (browseFilter != null)
			options.BrowseFilter = browseFilter;
		if (filePrefix != null)
			options.FilePrefix = filePrefix;
		options.Recurse = recurse;
		options.RecursionMode = recursionMode;
		options.NumberOfRecursionThreads = numberOfRecursionThreads;
		if (maxResults != null)
			options.MaxResults = maxResults;
		options.IncludeAttributes = includeAttributes;

		return await ListObjects(options, cancellationToken).ConfigureAwait(false);
	}


	/// <summary>
	/// Dowloads an object from the bucket, decodes it using the given encoding and returns the string.
	/// </summary>
	/// <param name="objectPath">Object path</param>
	/// <param name="textEncoding">Optional text encoding. When not specified, <see cref="UTF8Encoding"/> is used.</param>
	/// <param name="cancellationToken"></param>
	/// <returns></returns>
	public virtual async Task<string> GetText(
		string objectPath,
		Encoding textEncoding = null,
		CancellationToken cancellationToken = default) {
		Stream src = await OpenRead(objectPath, cancellationToken).ConfigureAwait(false);
		if (src == null) return null;

		var ms = new MemoryStream();
		using (src) {
			await src.CopyToAsync(ms).ConfigureAwait(false);
		}

		return (textEncoding ?? Encoding.UTF8).GetString(ms.ToArray());
	}

	/// <summary>
	/// Converts the string to binary using the given encoding and uploads to the bucket.
	/// </summary>
	/// <param name="objectPath">Object to write</param>
	/// <param name="text">Text to write, treated in UTF-8 encoding</param>
	/// <param name="textEncoding">Optional text encoding. When not specified, <see cref="UTF8Encoding"/> is used.</param>
	/// <param name="cancellationToken"></param>
	/// <returns></returns>
	public virtual async Task SetText(
		string objectPath, string text,
		Encoding textEncoding = null,
		CancellationToken cancellationToken = default) {
		using (Stream s = text.ToMemoryStream(textEncoding ?? Encoding.UTF8)) {
			await SetObject(objectPath, s, null, false, cancellationToken).ConfigureAwait(false);
		}
	}



	/// <summary>
	/// Checks if blobs exists in the storage
	/// </summary>
	public virtual async Task<List<bool>> ObjectsExists(IEnumerable<string> objectPaths, CancellationToken cancellationToken = default) {
		return (await (Task.WhenAll(objectPaths.Select(fp => ObjectExists(fp, cancellationToken))).ConfigureAwait(false))).ToList();
	}

	/// <summary>
	/// Checks if blobs exists in the storage
	/// </summary>
	public virtual Task<bool> ObjectExists(string objectPath, CancellationToken cancellationToken = default) {
		throw new NotSupportedException();
	}

	/// <summary>
	/// Deletes a single blob or a folder recursively.
	/// </summary>
	public virtual Task DeleteObject(string objectPath, CancellationToken cancellationToken = default) {
		throw new NotSupportedException();
	}

	/// <summary>
	/// Deletes multiple objects or folders recursively.
	/// </summary>
	public virtual Task DeleteObjects(IEnumerable<string> objectPaths, CancellationToken cancellationToken = default) {
		return Task.WhenAll(objectPaths.Select(fp => DeleteObject(fp, cancellationToken)));
	}

	/// <summary>
	/// Deletes a collection of blobs or folders
	/// </summary>
	public virtual async Task DeleteObjects(
		IEnumerable<StoreObject> blobs,
		CancellationToken cancellationToken = default) {
		await DeleteObjects(blobs.Select(b => b.FullPath), cancellationToken).ConfigureAwait(false);
	}

	/// <summary>
	/// Gets object metadata or null if object doesn't exist
	/// </summary>
	public virtual Task<StoreObject> GetObjectInfo(string objectPath, CancellationToken cancellationToken = default) {
		throw new NotSupportedException();
	}

	/// <summary>
	/// Gets object metadata or null if object doesn't exist
	/// </summary>
	public virtual Task<List<StoreObject>> GetObjectsInfo(IEnumerable<string> objectPaths, CancellationToken cancellationToken = default) {
		throw new NotSupportedException();
	}

	/// <summary>
	/// Set object metadata if the object exists
	/// </summary>
	public virtual Task SetObjectsInfo(IEnumerable<StoreObject> blobs, CancellationToken cancellationToken = default) {
		throw new NotSupportedException();
	}

	/// <summary>
	/// Set object metadata if the object exists
	/// </summary>
	public virtual Task SetObjectInfo(StoreObject obj, CancellationToken cancellationToken = default) {
		throw new NotImplementedException();
	}

	/// <summary>
	/// Writes byte array to the object.
	/// </summary>
	public virtual async Task SetBytes(string objectPath, byte[] data, bool append = false, CancellationToken cancellationToken = default) {
		if (data == null) {
			throw new ArgumentNullException(nameof(data));
		}

		using (var source = new MemoryStream(data)) {
			await SetObject(objectPath, source, null, append, cancellationToken).ConfigureAwait(false);
		}
	}

	/// <summary>
	/// Reads object data as byte array.
	/// </summary>
	public virtual async Task<byte[]> GetBytes(string objectPath, CancellationToken cancellationToken = default) {
		Stream src = await OpenRead(objectPath, cancellationToken).ConfigureAwait(false);
		if (src == null) return null;

		var ms = new MemoryStream();
		using (src) {
			await src.CopyToAsync(ms).ConfigureAwait(false);
		}

		return ms.ToArray();
	}



	/// <summary>
	/// Downloads blob to a stream
	/// </summary>
	/// <param name="objectPath">Object path</param>
	/// <param name="targetStream">Target stream to copy to, required</param>
	/// <exception cref="System.ArgumentNullException">Thrown when any parameter is null</exception>
	/// <exception cref="System.ArgumentException">Thrown when ID is too long. Long IDs are the ones longer than 50 characters.</exception>
	/// <exception cref="StorageException">Thrown when blob does not exist, error code set to <see cref="StorageErrorCode.NotFound"/></exception>
	public virtual async Task GetObject(
		string objectPath, Stream targetStream, CancellationToken cancellationToken = default) {
		if (targetStream == null)
			throw new ArgumentNullException(nameof(targetStream));

		Stream src = await OpenRead(objectPath, cancellationToken).ConfigureAwait(false);
		if (src == null) return;

		using (src) {
			await src.CopyToAsync(targetStream, BufferSize, cancellationToken).ConfigureAwait(false);
		}
	}



	/// <summary>
	/// Downloads an object from the bucket to the local filesystem.
	/// </summary>
	/// <param name="objectPath">Object path to download</param>
	/// <param name="filePath">Full path to the local file to be downloaded to. If the file exists it will be recreated wtih blob data.</param>
	public virtual async Task DownloadObject(string objectPath, string filePath, bool overwrite, CancellationToken cancellationToken = default) {

		// exit if object exists and overwriting is  disabled
		if (!overwrite && File.Exists(filePath)) return;

		// ensure parent directory exists
		string parentDir = Path.GetDirectoryName(filePath);
		if (!string.IsNullOrEmpty(parentDir)) {
			Directory.CreateDirectory(parentDir);
		}

		// open remote object, exit if cannot open
		Stream src = await OpenRead(objectPath, cancellationToken).ConfigureAwait(false);
		if (src == null) return;
		using (src) {

			// open local object
			using (Stream dest = File.Create(filePath)) {

				// download the cloud object to local disk
				await src.CopyToAsync(dest, BufferSize, cancellationToken).ConfigureAwait(false);
				await dest.FlushAsync().ConfigureAwait(false);
			}
		}
	}

	/// <summary>
	/// Uploads a local file to the bucket.
	/// </summary>
	/// <param name="objectPath">Object path to create or overwrite</param>
	/// <param name="filePath">Path to local file</param>
	/// <param name="overwrite"></param>
	/// <param name="cancellationToken"></param>
	public virtual async Task UploadObject(
		string objectPath, string filePath, bool overwrite, CancellationToken cancellationToken = default) {

		// exit if object exists and overwriting is  disabled
		if (!overwrite && await ObjectExists(objectPath, cancellationToken)) return;

		// open file and upload it
		using (Stream src = File.OpenRead(filePath)) {
			await SetObject(objectPath, src, null, false, cancellationToken).ConfigureAwait(false);
		}
	}


	/// <summary>
	/// Writes an object to blob storage using <see cref="JsonSerializer"/>
	/// </summary>
	/// <typeparam name="T">Objec type</typeparam>
	/// <param name="objectPath">Full path to blob</param>
	/// <param name="instance">Object instance to write</param>
	/// <param name="options">Optional serialiser options</param>
	/// <param name="encoding">Text encoding used to write to the blob storage, defaults to <see cref="UTF8Encoding"/></param>
	/// <param name="cancellationToken"></param>
	/// <returns></returns>
	public virtual async Task SetJson<T>(
		string objectPath, T instance,
		JsonSerializerOptions options = null,
		Encoding encoding = null,
		CancellationToken cancellationToken = default) {
		string jsonText = JsonSerializer.Serialize(instance, options);
		await SetText(objectPath, jsonText, encoding, cancellationToken).ConfigureAwait(false);
	}

	/// <summary>
	/// Reads an object from blob storage using <see cref="JsonSerializer"/>
	/// </summary>
	/// <param name="objectPath">Full path to blob</param>
	/// <param name="ignoreInvalidJson">When true, json that cannot be deserialised is ignored and method simply returns default value</param>
	/// <param name="options">Optional serialiser options</param>
	/// <param name="encoding">Text encoding used to write to the blob storage, defaults to <see cref="UTF8Encoding"/></param>
	/// <param name="cancellationToken"></param>
	/// <returns></returns>
	public virtual async Task<T> GetJson<T>(string objectPath,
		bool ignoreInvalidJson = false,
		JsonSerializerOptions options = null,
		Encoding encoding = null,
		CancellationToken cancellationToken = default) {
		string jsonText = await GetText(objectPath, encoding, cancellationToken).ConfigureAwait(false);
		if (string.IsNullOrEmpty(jsonText))
			return default;

		try {
			return JsonSerializer.Deserialize<T>(jsonText, options);
		}
		catch (JsonException) {
			if (ignoreInvalidJson)
				return default;

			throw;
		}
	}



	/// <summary>
	/// Copies an object from one store to another.
	/// </summary>
	/// <param name="blobId">Object path to copy</param>
	/// <param name="targetStorage">Target storage</param>
	/// <param name="newId">Optional, when specified uses this id in the target  If null uses the original ID.</param>
	public virtual async Task CopyObjectTo(
		string blobId, IStore targetStorage, string newId, CancellationToken cancellationToken = default) {
		using (Stream src = await OpenRead(blobId, cancellationToken).ConfigureAwait(false)) {
			if (src == null)
				return;

			await targetStorage.SetObject(newId ?? blobId, src, false, cancellationToken).ConfigureAwait(false);
		}
	}

	/// <summary>
	/// Rename an object or file.
	/// </summary>
	public virtual Task<bool> MoveObject(string oldPath, string newPath, bool overwrite, CancellationToken cancellationToken = default) {
		throw new NotImplementedException();
	}


	/// <summary>
	/// Gets information about the connected FTP/SFTP server.
	/// </summary>
	public virtual Task<Dictionary<string, object>> GetServer(CancellationToken cancellationToken = default) {
		throw new NotImplementedException();
	}

	// ---------------------------------------------------------------------
	// Directory
	// ---------------------------------------------------------------------

	/// <summary>
	/// Downloads all files from a remote folder to a local folder recursively.
	/// Missing local directories are created automatically.
	/// Will call the given `progress` callback per file upon success or failure.
	/// Absorbs all errors internally, and does not abort the entire process if a single file failed to transfer.
	/// </summary>
	public virtual async Task<List<StorageProgress>> DownloadDirectory(string remoteFolder, string localFolder, StorageExists existsMode = StorageExists.Skip,
		Action<StorageProgress>? progress = null, IList<StorageRule> rules = null, CancellationToken cancellationToken = default) {

		if (string.IsNullOrWhiteSpace(localFolder)) throw new ArgumentNullException(nameof(localFolder));
		if (string.IsNullOrWhiteSpace(remoteFolder)) throw new ArgumentNullException(nameof(remoteFolder));

		// collect all progress objects (one per file)
		var results = new List<StorageProgress>();

		remoteFolder = StoragePath.Normalize(remoteFolder);
		Directory.CreateDirectory(localFolder);

		// per file found on remote store
		var objects = (await ListDirectory(remoteFolder, true, cancellationToken).ConfigureAwait(false))
			.Where(x => x.Type == StorageObjectType.File)
			.ToList();

		// exit if nothing to transfer
		if (objects.Count == 0)
			return results;

		cancellationToken.ThrowIfCancellationRequested();

		// if any rules are defined
		if (rules != null && rules.Count > 0) {
			objects = RuleEngine.ProcessDownloadRules(rules, results, objects);
		}

		// exit if nothing to transfer
		if (objects.Count == 0)
			return results;

		// declare a small utility to call the progress handler with error suppression
		void Report(StorageProgress p) {
			results.Add(p);
			if (progress != null) {
				try { progress.Invoke(p); } catch { }
			}
		}

		int ok = 0, skipped = 0, failed = 0;
		long bytes = 0;

		for (int i = 0; i < objects.Count; i++) {
			cancellationToken.ThrowIfCancellationRequested();

			var obj = objects[i];
			string rel = StoragePath.GetRelativeCloudPath(remoteFolder, obj.FullPath);

			// for some reason the object is not within the root folder, so skip it
			if (rel.Length == 0) {
				continue;
			}

			// calc the local path
			string localFile = Path.Combine(localFolder, rel);

			try {
				Directory.CreateDirectory(Path.GetDirectoryName(localFile)!);

				//----------------------------------------------------
				// skip, overwrite, or overwrite if changed
				switch (existsMode) {
					case StorageExists.Skip:
						if (File.Exists(localFile)) {
							skipped++;

							// report skipped transfers
							Report(new StorageProgress {
								LocalPath = localFile,
								RemotePath = obj.FullPath,
								FileIndex = i,
								FileCount = objects.Count,
								Skipped = true,
								SkipReason = StorageReason.Exists,
							});
							continue;
						}
						break;
					case StorageExists.OverwriteByChecksum:
					case StorageExists.OverwriteByTimestamp:
						var checksum = existsMode == StorageExists.OverwriteByChecksum;

						// if it exists
						if (File.Exists(localFile)) {

							// get the length and check if mismatches
							var localInfo = new FileInfo(localFile);
							var localLen = localInfo.Length;
							var remoteLen = await GetObjectLength(obj.FullPath, -1, cancellationToken).ConfigureAwait(false);
							if (remoteLen != -1 && remoteLen == localLen) {

								// in checksum mode
								if (checksum) {

									// get the checksum
									var remoteChecksum = await GetObjectChecksum(obj.FullPath, StorageHash.MD5, cancellationToken).ConfigureAwait(false);
									if (remoteChecksum.VerifyFile(localFile)) {

										// skip since checksum matches
										skipped++;

										// report skipped transfers
										Report(new StorageProgress {
											LocalPath = localFile,
											RemotePath = obj.FullPath,
											FileIndex = i,
											FileCount = objects.Count,
											Skipped = true,
											SkipReason = StorageReason.Checksum,
										});
										continue;
									}
									else {
										// checksum has changed, so overwrite
										break;
									}
								}
								else {

									// date mode

									// get the date and check if matches
									var remoteDate = await GetObjectInfo(obj.FullPath, cancellationToken).ConfigureAwait(false);
									if (IsDateMatch(localInfo, remoteDate)) {

										// skip since date matches
										skipped++;

										// report skipped transfers
										Report(new StorageProgress {
											LocalPath = localFile,
											RemotePath = obj.FullPath,
											FileIndex = i,
											FileCount = objects.Count,
											Skipped = true,
											SkipReason = StorageReason.Timestamp,
										});
										continue;
									}
									else {
										// date has changed, so overwrite
										break;
									}
								}
							}
							else {
								// length has changed, so overwrite
								break;
							}
						}
						break;
					case StorageExists.Throw:
						if (File.Exists(localFile))
							throw new IOException($"File '{localFile}' already exists.");
						break;
				}

				//----------------------------------------------------
				// download file
				await DownloadObject(obj.FullPath, localFile,
					true,
					/*p => {
						p.LocalPath = localFile;
						p.RemotePath = obj.Path;
						p.FileIndex = i + 1;
						p.FileCount = objects.Count;
						Report(p);
					},*/
					cancellationToken).ConfigureAwait(false);

				// report OK transfers
				Report(new StorageProgress {
					LocalPath = localFile,
					RemotePath = obj.FullPath,
					FileIndex = i,
					FileCount = objects.Count,
					Progress = 100
				});

				ok++;
				if (File.Exists(localFile))
					bytes += new FileInfo(localFile).Length;
			}
			catch (OperationCanceledException) { throw; }
			catch (Exception ex) {

				// report failed transfers but do not crash entire process
				failed++;
				Report(new StorageProgress {
					LocalPath = localFile,
					RemotePath = obj.FullPath,
					FileIndex = i,
					FileCount = objects.Count,
					Progress = -1,
					TransferredBytes = 0,
					TransferSpeed = 0,
					ETA = TimeSpan.Zero,
					Error = ex
				});
			}
		}
		return results;
	}

	/// <summary>
	/// Uploads all files from a local folder to a remote folder recursively.
	/// For file system providers, missing remote directories are created automatically.
	/// For object storage providers, files are uploaded as objects using their relative paths.
	/// Will call the given `progress` callback per file upon success or failure.
	/// Absorbs all errors internally, and does not abort the entire process if a single file failed to transfer.
	/// </summary>
	public virtual async Task<List<StorageProgress>> UploadDirectory(string localFolder, string remoteFolder, StorageExists existsMode = StorageExists.Skip,
		Action<StorageProgress>? progress = null, IList<StorageRule> rules = null, CancellationToken cancellationToken = default) {
		remoteFolder = StoragePath.Normalize(remoteFolder);

		if (string.IsNullOrWhiteSpace(localFolder)) throw new ArgumentNullException(nameof(localFolder));
		if (string.IsNullOrWhiteSpace(remoteFolder)) throw new ArgumentNullException(nameof(remoteFolder));

		// collect all progress objects (one per file)
		var results = new List<StorageProgress>();

		// exit if local folder does not exist
		if (!Directory.Exists(localFolder))
			return results;

		bool isFileSystem = await IsFileSystem().ConfigureAwait(false);

		// get all the local files in this folder
		var files = Directory.GetFiles(localFolder, "*", SearchOption.AllDirectories).ToList();

		// exit if nothing to transfer
		if (files.Count == 0)
			return results;

		cancellationToken.ThrowIfCancellationRequested();

		// compute relative paths
		var relativeFiles = new List<string>();
		foreach (var o in files) {
			relativeFiles.Add(StoragePath.Normalize(StoragePath.GetRelativeDiskPath(localFolder, o)));
		}

		// if any rules are defined
		if (rules != null && rules.Count > 0) {
			(files, relativeFiles) = RuleEngine.ProcessUploadRules(rules, results, files, relativeFiles);
		}

		// exit if nothing to transfer
		if (files.Count == 0)
			return results;

		//var createdDirectories = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

		if (isFileSystem) {
			await CreateDirectory(remoteFolder, true, cancellationToken).ConfigureAwait(false);
			//createdDirectories.Add(remoteFolder);
		}

		// declare a small utility to call the progress handler with error suppression
		void Report(StorageProgress p) {
			results.Add(p);
			if (progress != null) {
				try { progress.Invoke(p); } catch { }
			}
		}

		int ok = 0, skipped = 0, failed = 0;

		// per file found in local folder
		for (int f = 0; f < files.Count; f++) {
			cancellationToken.ThrowIfCancellationRequested();

			string localFile = files[f];
			string rel = relativeFiles[f];
			var localInfo = new FileInfo(localFile);
			long localLen = localInfo.Length;

			// calc the remote path
			string objectPath = StoragePath.Normalize(StoragePath.Combine(remoteFolder, rel));

			try {

				// only create parent folders once per run (avoid duplicate calls) - NOT REQUIRED AS ITS HANDLED BY THE PROVIDER
				/*if (isFileSystem) {
					string? dir = StoragePath.GetParent(objectPath);
					if (!string.IsNullOrEmpty(dir) && createdDirectories.Add(dir))
						await CreateDirectory(dir, true, cancellationToken).ConfigureAwait(false);
				}*/

				//----------------------------------------------------
				// skip, overwrite, or overwrite if changed
				switch (existsMode) {
					case StorageExists.Skip:
						if (await ObjectExists(objectPath, cancellationToken).ConfigureAwait(false)) {
							skipped++;

							// report skipped transfers
							Report(new StorageProgress {
								LocalPath = localFile,
								RemotePath = objectPath,
								FileIndex = f,
								FileCount = files.Count,
								Skipped = true,
								SkipReason = StorageReason.Exists,
							});
							continue;
						}
						break;
					case StorageExists.OverwriteByChecksum:
					case StorageExists.OverwriteByTimestamp:
						var checksum = existsMode == StorageExists.OverwriteByChecksum;

						// if it exists
						if (await ObjectExists(objectPath, cancellationToken).ConfigureAwait(false)) {

							// get the length and check if mismatches
							var remoteLen = await GetObjectLength(objectPath, -1, cancellationToken).ConfigureAwait(false);
							if (remoteLen != -1 && remoteLen == localLen) {

								// in checksum mode
								if (checksum) {

									// get the checksum
									var remoteChecksum = await GetObjectChecksum(objectPath, StorageHash.MD5, cancellationToken).ConfigureAwait(false);
									if (remoteChecksum.VerifyFile(localFile)) {

										// skip since checksum matches
										skipped++;

										// report skipped transfers
										Report(new StorageProgress {
											LocalPath = localFile,
											RemotePath = objectPath,
											FileIndex = f,
											FileCount = files.Count,
											Skipped = true,
											SkipReason = StorageReason.Checksum,
										});
										continue;
									}
									else {
										// checksum has changed, so overwrite
										break;
									}
								}
								else {

									// date mode

									// get the date and check if matches
									var remoteDate = await GetObjectInfo(objectPath, cancellationToken).ConfigureAwait(false);
									if (IsDateMatch(localInfo, remoteDate)) {

										// skip since date matches
										skipped++;

										// report skipped transfers
										Report(new StorageProgress {
											LocalPath = localFile,
											RemotePath = objectPath,
											FileIndex = f,
											FileCount = files.Count,
											Skipped = true,
											SkipReason = StorageReason.Timestamp,
										});
										continue;
									}
									else {
										// date has changed, so overwrite
										break;
									}
								}
							}
							else {
								// length has changed, so overwrite
								break;
							}
						}
						break;
					case StorageExists.Throw:
						if (await ObjectExists(objectPath, cancellationToken).ConfigureAwait(false))
							throw new StorageException($"UploadDirectory: Object '{objectPath}' already exists.");
						break;
				}

				//----------------------------------------------------
				// upload file
				await UploadObject(objectPath, localFile,
					true,
					/*p => {
						p.LocalPath = localFile;
						p.RemotePath = objectPath;
						p.FileIndex = i + 1;
						p.FileCount = files.Length;
						if (p.Progress < 0) { p.Progress = 100; p.TransferredBytes = length; }
						Report(p);
					},*/
					cancellationToken).ConfigureAwait(false);

				// report OK transfers
				Report(new StorageProgress {
					LocalPath = localFile,
					RemotePath = objectPath,
					FileIndex = f,
					FileCount = files.Count,
					Progress = 100,
					TransferredBytes = localLen
				});

				ok++;
			}
			catch (OperationCanceledException) { throw; }
			catch (Exception ex) {

				// report failed transfers but do not crash entire process
				failed++;
				Report(new StorageProgress {
					LocalPath = localFile,
					RemotePath = objectPath,
					FileIndex = f,
					FileCount = files.Count,
					Progress = -1,
					TransferredBytes = 0,
					TransferSpeed = 0,
					ETA = TimeSpan.Zero,
					Error = ex
				});
			}
		}
		return results;
	}

	private static bool IsDateMatch(FileInfo local, StoreObject remote) {

		// check if remote date is within 2 seconds of local date [avoids issues with some SFTP servers]
		return remote != null && remote.DateModified.HasValue &&
		       Math.Abs((remote.DateModified.Value - local.LastWriteTimeUtc).TotalSeconds) < 2;
	}


	/// <summary>
	/// Creates a new folder in this file system. Does nothing in cloud storage buckets.
	/// </summary>
	/// <param name="folderPath">Path to the new folder.</param>
	public virtual Task CreateDirectory(string folderPath, bool force, CancellationToken cancellationToken = default) {
		// FIX: do not throw any exception here, as all the cloud stores do not required directory creation
		//		and this is implemented specially for filesystem-based stores like disk/FTP/SFTP
		return Task.CompletedTask;
	}

	/// <summary>
	/// Deletes a folder in this file system. Deletes all objects under the given virtual folder in cloud storage buckets.
	/// </summary>
	/// <param name="folderPath">Path to the folder or virtual folder.</param>
	/// <param name="recursive">Whether to delete all child objects.</param>
	public virtual async Task DeleteDirectory(string folderPath, bool recursive, CancellationToken cancellationToken = default) {

		// list all objects in the folder
		var objects = await ListDirectory(folderPath, recursive, cancellationToken);

		// delete all listed objects
		if (objects.Count > 0) {
			await DeleteObjects(objects, cancellationToken);
		}

	}

	/// <summary>
	/// Returns true if the specified directory or virtual directory exists.
	/// </summary>
	public virtual Task<bool> DirectoryExists(string folderPath, CancellationToken cancellationToken = default) {
		throw new NotImplementedException();
	}

	/// <summary>
	/// Moves a directory or virtual directory.
	/// </summary>
	public virtual Task MoveDirectory(string sourceFolderPath, string destinationFolderPath, CancellationToken cancellationToken = default) {
		throw new NotImplementedException();
	}

	// <summary>
	// Renames a directory or virtual directory.
	// </summary>
	/*public virtual async Task RenameDirectory(string folderPath, string newFolderName, CancellationToken cancellationToken = default) {
		throw new NotImplementedException();
	}*/

	// ---------------------------------------------------------------------
	// Permissions
	// ---------------------------------------------------------------------

	/// <summary>
	/// Gets the CHMOD permissions of a file.
	/// </summary>
	public virtual Task<int> GetFilePermissions(string filePath, CancellationToken cancellationToken = default) {
		throw new NotImplementedException();
	}

	/// <summary>
	/// Sets the CHMOD permissions of a file.
	/// </summary>
	public virtual Task SetFilePermissions(string filePath, int permissions, CancellationToken cancellationToken = default) {
		throw new NotImplementedException();
	}

	// ---------------------------------------------------------------------
	// Presigned URL
	// ---------------------------------------------------------------------

	/// <summary>
	/// Get a pre-signed URL to upload an object to this bucket.
	/// </summary>
	public virtual async Task<string> GetUploadUrl(string objectPath, bool https, int expiresInSeconds = 86000) {
		return await GetPresignedUrl(objectPath, false, https, expiresInSeconds).ConfigureAwait(false);
	}

	/// <summary>
	/// Get a pre-signed URL to download an object from this bucket.
	/// </summary>
	public virtual async Task<string> GetDownloadUrl(string objectPath, bool https, int expiresInSeconds = 86000) {
		return await GetPresignedUrl(objectPath, true, https, expiresInSeconds).ConfigureAwait(false);
	}

	/// <summary>
	/// Generates a pre-signed URL for or SAS the specified object.
	/// The URL grants temporary access to the object and expries after the specified duration. MIME type is auto computed.
	/// </summary>
	public virtual Task<string> GetPresignedUrl(string objectPath, bool forDownload, bool https, int expiresInSeconds = 86000) {
		throw new NotImplementedException();
	}

	/// <summary>
	/// Generates a SAS for the specified object. Azure-friendly API with complete SAS options.
	/// The URL grants temporary access to the object and expries after the specified duration.
	/// </summary>
	public virtual Task<string> GetObjectSas(string objectPath, StorageUrlOptions options) {
		throw new NotImplementedException();
	}


	// ---------------------------------------------------------------------
	// Object Versioning
	// ---------------------------------------------------------------------

	public virtual Task<List<StorageObjectVersion>> ListObjectVersions(string objectPath, CancellationToken cancellationToken = default) {
		throw new NotSupportedException();
	}

	public virtual Task<StorageObjectVersion> GetObjectVersion(string objectPath, string versionId, CancellationToken cancellationToken = default) {
		throw new NotSupportedException();
	}

	public virtual Task<bool> RestoreObjectVersion(string objectPath, string versionId, CancellationToken cancellationToken = default) {
		throw new NotSupportedException();
	}

	public virtual Task<bool> DeleteObjectVersion(string objectPath, string versionId, CancellationToken cancellationToken = default) {
		throw new NotSupportedException();
	}


	// ---------------------------------------------------------------------
	// Object Tags
	// ---------------------------------------------------------------------

	public virtual Task<Dictionary<string, string>> GetObjectTags(string objectPath, CancellationToken cancellationToken = default) {
		throw new NotSupportedException();
	}

	public virtual Task<bool> SetObjectTags(string objectPath, Dictionary<string, string> tags, CancellationToken cancellationToken = default) {
		throw new NotSupportedException();
	}

	public virtual Task<bool> DeleteObjectTags(string objectPath, CancellationToken cancellationToken = default) {
		throw new NotSupportedException();
	}


	// ---------------------------------------------------------------------
	// Storage Tier or Class
	// ---------------------------------------------------------------------

	public virtual Task<StorageTier> GetObjectTier(string objectPath, CancellationToken cancellationToken = default) {
		throw new NotSupportedException();
	}

	public virtual Task<bool> SetObjectTier(string objectPath, StorageTier tier, CancellationToken cancellationToken = default) {
		throw new NotSupportedException();
	}


	// ---------------------------------------------------------------------
	// Retention Policy
	// ---------------------------------------------------------------------

	/*public virtual async Task<StorageRetentionPolicy> GetObjectRetentionPolicy(string objectPath, CancellationToken cancellationToken = default) {
		throw new NotSupportedException();
	}

	public virtual async Task<bool> SetObjectRetentionPolicy(string objectPath, StorageRetentionPolicy policy, CancellationToken cancellationToken = default) {
		throw new NotSupportedException();
	}

	public virtual async Task<bool> ClearObjectRetentionPolicy(string objectPath, CancellationToken cancellationToken = default) {
		throw new NotSupportedException();
	}*/


	// ---------------------------------------------------------------------
	// Object Lock
	// ---------------------------------------------------------------------

	/*public virtual async Task<StorageObjectLock> GetObjectLock(string objectPath, CancellationToken cancellationToken = default) {
		throw new NotSupportedException();
	}

	public virtual async Task<bool> SetObjectLock(string objectPath, StorageObjectLock objectLock, CancellationToken cancellationToken = default) {
		throw new NotSupportedException();
	}

	public virtual async Task<bool> ClearObjectLock(string objectPath, CancellationToken cancellationToken = default) {
		throw new NotSupportedException();
	}*/

	// ---------------------------------------------------------------------
	// Checksum
	// ---------------------------------------------------------------------

	/// <summary>Returns the checksum of an object using the required hash algorithm. Returns null if the object cannot be found.</summary>
	/// <param name="fullPath">Full path of the object.</param>
	/// <param name="hash">Hash algorithm to use. MD5 has the broadest support but CRC and others can also be used.</param>
	/// <param name="cancellationToken">Cancellation token.</param>
	public virtual async Task<StorageObjectHash> GetObjectChecksum(string fullPath, StorageHash hash = StorageHash.MD5, CancellationToken cancellationToken = default){
		if (fullPath == null) throw new ArgumentNullException(nameof(fullPath));

		// try to quickly get MD5 from object info, if the provider supports it
		if (hash == StorageHash.MD5) {
			var info = await GetObjectInfo(fullPath, cancellationToken).ConfigureAwait(false);
			if (info.MD5 != null)
				return new StorageObjectHash(StoragePath.Normalize(fullPath), info.MD5, StorageHash.MD5);
		}

		// get object data and hash it manually
		var bytes = await GetBytes(fullPath, cancellationToken).ConfigureAwait(false);
		if (bytes == null) return null;
		var checksum = HashUtility.HashBytes(bytes, hash);
		return new StorageObjectHash(StoragePath.Normalize(fullPath), checksum, hash);
	}


}
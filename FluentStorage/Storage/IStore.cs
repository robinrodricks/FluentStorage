using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using FluentStorage.Enums;
using FluentStorage.Model;
using FluentStorage.Rules;
using FluentStorage.Streaming;

namespace FluentStorage.Storage;

/// <summary>
/// Interface to manage a single bucket across various cloud providers (AWS/Azure/GCP/etc)
/// The same interface is used to manage a single file system provider (Disk/FTP/FTPS).
/// </summary>
public interface IStore : IDisposable {


	// ---------------------------------------------------------------------
	// System
	// ---------------------------------------------------------------------

	/// <summary>
	/// Returns the base client object used to communicate with the cloud/server.
	/// </summary>
	Task<object> GetClient();

	/// <summary>
	/// Returns true if the given object storage is backed by a file system (Disk/FTP/FTPS).
	/// </summary>
	Task<bool> IsFileSystem();

	/// <summary>
	/// Returns true if the given object storage supports seeking and streaming.
	/// </summary>
	Task<bool> IsSeekable();

	/// <summary>
	/// Returns true if the given object storage supports file versioning, and if versioning is enabled at the bucket level.
	/// </summary>
	Task<bool> IsVersioned();

	/// <summary>
	/// Returns true if the given object storage supports object tags.
	/// </summary>
	Task<bool> IsTagged();

	/// <summary>
	/// Returns true if the given object storage supports multiple storage tiers/classes.
	/// </summary>
	Task<bool> IsTiered();

	// ---------------------------------------------------------------------
	// Listing
	// ---------------------------------------------------------------------

	/// <summary>Returns the list of objects in this bucket.</summary>
	/// <param name="options">Listing options.</param>
	/// <param name="cancellationToken">Cancellation token.</param>
	/// <returns>The matching objects.</returns>
	Task<List<StoreObject>> ListObjects(StorageListOptions options = null, CancellationToken cancellationToken = default);

	/// <summary>
	/// Returns the list of objects in a specific directory of this bucket.
	/// </summary>
	/// <param name="folderPath">Remote folder path or virtual folder path to list</param>
	/// <param name="recurse">Recurse into sub folders?</param>
	/// <returns>List of remote object paths</returns>
	Task<List<StoreObject>> ListDirectory(string folderPath, bool recurse, CancellationToken cancellationToken = default);

	/// <summary>
	/// Returns the list of objects in a specific directory of this bucket.
	/// </summary>
	/// <param name="folderPath"><see cref="StorageListOptions.FolderPath"/></param>
	/// <param name="browseFilter"><see cref="StorageListOptions.BrowseFilter"/></param>
	/// <param name="filePrefix"><see cref="StorageListOptions.FilePrefix"/></param>
	/// <param name="recurse"><see cref="StorageListOptions.Recurse"/></param>
	/// <param name="recursionMode"><see cref="StorageListOptions.RecursionMode"/></param>
	/// <param name="numberOfRecursionThreads"><see cref="StorageListOptions.NumberOfRecursionThreads"/></param>
	/// <param name="maxResults"><see cref="StorageListOptions.MaxResults"/></param>
	/// <param name="includeAttributes"><see cref="StorageListOptions.IncludeAttributes"/></param>
	/// <returns>List of remote object paths</returns>
	Task<List<StoreObject>> ListDirectory(string folderPath = null, Func<StoreObject, bool> browseFilter = null,
		string filePrefix = null, bool recurse = false,
		StorageRecursion recursionMode = StorageRecursion.Remote,
		int numberOfRecursionThreads = StorageListOptions.MAX_THREADS,
		int? maxResults = null, bool includeAttributes = false,
		CancellationToken cancellationToken = default);


	// ---------------------------------------------------------------------
	// Metadata & Existence
	// ---------------------------------------------------------------------

	/// <summary>Checks whether an object exists.</summary>
	/// <param name="objectPath">Full path of the object.</param>
	/// <param name="cancellationToken">Cancellation token.</param>
	/// <returns><see langword="true"/> if the object exists; otherwise <see langword="false"/>.</returns>
	Task<bool> ObjectExists(string objectPath, CancellationToken cancellationToken = default);

	/// <summary>Checks if objects exist in the storage.</summary>
	/// <param name="objectPaths">Full paths of the objects.</param>
	/// <param name="cancellationToken">Cancellation token.</param>
	/// <returns>A collection indicating whether each object exists.</returns>
	Task<List<bool>> ObjectsExists(IEnumerable<string> objectPaths, CancellationToken cancellationToken = default);

	/// <summary>Gets metadata for a single object.</summary>
	/// <param name="objectPath">Full path of the object.</param>
	/// <param name="cancellationToken">Cancellation token.</param>
	/// <returns>The object metadata.</returns>
	Task<StoreObject> GetObjectInfo(string objectPath, CancellationToken cancellationToken = default);

	/// <summary>Gets object information which is useful for retrieving object metadata.</summary>
	/// <param name="objectPaths">Full paths of the objects.</param>
	/// <param name="cancellationToken">Cancellation token.</param>
	/// <returns>The object metadata.</returns>
	Task<List<StoreObject>> GetObjectsInfo(IEnumerable<string> objectPaths, CancellationToken cancellationToken = default);

	/// <summary>Updates metadata for a single object.</summary>
	/// <param name="metadata">Object metadata.</param>
	/// <param name="cancellationToken">Cancellation token.</param>
	Task SetObjectInfo(StoreObject metadata, CancellationToken cancellationToken = default);

	/// <summary>Sets object information which is useful for setting object attributes (user metadata etc.).</summary>
	/// <param name="metadata">Object metadata.</param>
	/// <param name="cancellationToken">Cancellation token.</param>
	Task SetObjectsInfo(IEnumerable<StoreObject> metadata, CancellationToken cancellationToken = default);

	// ---------------------------------------------------------------------
	// Read
	// ---------------------------------------------------------------------

	/// <summary>
	/// Gets the length of an object in bytes.
	/// Catches all exceptions internally.
	/// Returns `defaultValue` if the object cannot be not found or there was an error.
	/// </summary>
	/// <param name="path">The full object path.</param>
	/// <param name="defaultValue">The value to return if the object was not found or there was an error.</param>
	/// <param name="cancellationToken">The cancellation token.</param>
	Task<long> GetObjectLength(string path, long defaultValue = -1, CancellationToken cancellationToken = default);

	/// <summary>
	/// Opens the object stream for reading.
	/// It is your responsibility to close and dispose this stream after use.
	/// </summary>
	/// <param name="objectPath">Full path of the object.</param>
	/// <param name="cancellationToken">Cancellation token.</param>
	/// <returns>An open readable stream.</returns>
	Task<Stream> OpenRead(string objectPath, CancellationToken cancellationToken = default);

	/// <summary>
	/// Opens a stream for writing to the object.
	/// The object will be written to the cloud when the stream is disposed.
	/// It is your responsibility to dispose this stream.
	/// Returns null if the file exists and overwriting is disabled.
	/// </summary>
	/// <param name="objectPath">Full path of the object.</param>
	/// <param name="cancellationToken">Cancellation token.</param>
	/// <returns></returns>
	Task<Stream> OpenWrite(string objectPath, bool overwrite, CancellationToken cancellationToken = default);

	/// <summary>
	/// Opens a readable stream beginning at the specified byte offset.
	/// The returned stream is not guaranteed to stop at the requird length, and might read till the end of the file.
	/// It is your responsibility to dispose this stream.
	/// </summary>
	/// <param name="path">Full path of the object.</param>
	/// <param name="offset">Starting byte offset.</param>
	/// <param name="length">Maximum number of bytes to expose. </param>
	Task<Stream> OpenRange(string path, long offset, long length, CancellationToken cancellationToken = default);

	/// <summary>
	/// Opens a seekable read stream for streaming or video playback of an object.
	/// The object's length is read when the stream is created.
	/// Returns null if the file does not exists.
	/// It is your responsibility to dispose this stream.
	/// </summary>
	Task<SeekableStream> OpenSeekable(string path, int bufferSize = 65536, CancellationToken cancellationToken = default);

	/// <summary>Copies an object into an existing stream.</summary>
	/// <param name="objectPath">Full path of the object.</param>
	/// <param name="targetStream">Destination stream.</param>
	/// <param name="cancellationToken">Cancellation token.</param>
	Task GetObject(string objectPath, Stream targetStream, CancellationToken cancellationToken = default);

	/// <summary>Reads an object into a byte array.</summary>
	/// <param name="objectPath">Full path of the object.</param>
	/// <param name="cancellationToken">Cancellation token.</param>
	/// <returns>The object contents.</returns>
	Task<byte[]> GetBytes(string objectPath, CancellationToken cancellationToken = default);

	/// <summary>Reads an object as text.</summary>
	/// <param name="objectPath">Full path of the object.</param>
	/// <param name="textEncoding">Text encoding. Defaults to UTF-8.</param>
	/// <param name="cancellationToken">Cancellation token.</param>
	/// <returns>The object contents.</returns>
	Task<string> GetText(string objectPath, Encoding textEncoding = null, CancellationToken cancellationToken = default);

	/// <summary>Reads and deserializes a JSON object.</summary>
	/// <param name="objectPath">Full path of the object.</param>
	/// <param name="ignoreInvalidJson">Whether invalid JSON should return the default value instead of throwing.</param>
	/// <param name="options">JSON serializer options.</param>
	/// <param name="encoding">Text encoding. Defaults to UTF-8.</param>
	/// <param name="cancellationToken">Cancellation token.</param>
	/// <returns>The deserialized object.</returns>
	Task<T> GetJson<T>(string objectPath, bool ignoreInvalidJson = false, JsonSerializerOptions options = null, Encoding encoding = null, CancellationToken cancellationToken = default);

	/// <summary>Downloads an object to a local file. Ensures that the local directory exists.</summary>
	/// <param name="objectPath">Full path of the object.</param>
	/// <param name="filePath">Destination file path.</param>
	/// <param name="cancellationToken">Cancellation token.</param>
	Task DownloadObject(string objectPath, string filePath, bool overwrite, CancellationToken cancellationToken = default);


	// ---------------------------------------------------------------------
	// Write
	// ---------------------------------------------------------------------

	/// <summary>Uploads data to an object from a stream.
	/// Existing objects are overwritten if `append` is false.
	/// Ensures that the remote directory exists (for FTP/SFTP).</summary>
	/// <param name="objectPath">Full path of the object.</param>
	/// <param name="dataStream">Source stream.</param>
	/// <param name="append">Whether to append to an existing object.</param>
	/// <param name="cancellationToken">Cancellation token.</param>
	Task SetObject(string objectPath, Stream dataStream, bool append = false, CancellationToken cancellationToken = default);

	/// <summary>Uploads data to an object from a stream.
	/// Existing objects are overwritten if `append` is false.
	/// Ensures that the remote directory exists (for FTP/SFTP).</summary>
	/// <param name="objectPath">Full path of the object.</param>
	/// <param name="dataStream">Source stream.</param>
	/// <param name="contentType">MIME content type.</param>
	/// <param name="append">Whether to append to an existing object.</param>
	/// <param name="cancellationToken">Cancellation token.</param>
	Task SetObject(string objectPath, Stream dataStream, string contentType, bool append = false, CancellationToken cancellationToken = default);

	/// <summary>Writes a byte array to an object.
	/// Existing objects are overwritten if `append` is false.
	/// Ensures that the remote directory exists (for FTP/SFTP).</summary>
	/// <param name="objectPath">Full path of the object.</param>
	/// <param name="data">Data to write.</param>
	/// <param name="append">Whether to append to an existing object.</param>
	/// <param name="cancellationToken">Cancellation token.</param>
	Task SetBytes(string objectPath, byte[] data, bool append = false, CancellationToken cancellationToken = default);

	/// <summary>Writes text to an object.
	/// Existing objects are overwritten if `append` is false.
	/// Ensures that the remote directory exists (for FTP/SFTP).</summary>
	/// <param name="objectPath">Full path of the object.</param>
	/// <param name="text">Text to write.</param>
	/// <param name="textEncoding">Text encoding. Defaults to UTF-8.</param>
	/// <param name="cancellationToken">Cancellation token.</param>
	Task SetText(string objectPath, string text, Encoding textEncoding = null, CancellationToken cancellationToken = default);

	/// <summary>
	/// Writes an object as JSON.
	/// Existing objects are always overwritten.
	/// Ensures that the remote directory exists (for FTP/SFTP).
	/// </summary>
	/// <param name="objectPath">Full path of the object.</param>
	/// <param name="instance">Object to serialize.</param>
	/// <param name="options">JSON serializer options.</param>
	/// <param name="encoding">Text encoding. Defaults to UTF-8.</param>
	/// <param name="cancellationToken">Cancellation token.</param>
	Task SetJson<T>(string objectPath, T instance, JsonSerializerOptions options = null, Encoding encoding = null, CancellationToken cancellationToken = default);

	/// <summary>
	/// Uploads a local file to an object.
	/// Existing objects are overwritten if `overwrite` is true, otherwise it skips the upload.
	/// Ensures that the remote directory exists (for FTP/SFTP).
	/// </summary>
	/// <param name="objectPath">Full path of the object.</param>
	/// <param name="filePath">Source file path.</param>
	/// <param name="cancellationToken">Cancellation token.</param>
	Task UploadObject(string objectPath, string filePath, bool overwrite, CancellationToken cancellationToken = default);


	// ---------------------------------------------------------------------
	// Object Manipulation
	// ---------------------------------------------------------------------

	/// <summary>Copies an object to another bucket.</summary>
	/// <param name="blobId">Source object identifier.</param>
	/// <param name="targetStorage">Destination bucket.</param>
	/// <param name="newId">Destination object identifier.</param>
	/// <param name="cancellationToken">Cancellation token.</param>
	Task CopyObjectTo(string blobId, IStore targetStorage, string newId, CancellationToken cancellationToken = default);

	/// <summary>Renames an object (file or folder).</summary>
	/// <param name="oldPath">Current path.</param>
	/// <param name="newPath">New path.</param>
	/// <param name="cancellationToken">Cancellation token.</param>
	Task<bool> MoveObject(string oldPath, string newPath, bool overwrite, CancellationToken cancellationToken = default);

	/// <summary>Deletes a single object or file. Does NOT delete folders or directories.</summary>
	/// <param name="objectPath">Full path of the object or folder.</param>
	/// <param name="cancellationToken">Cancellation token.</param>
	Task DeleteObject(string objectPath, CancellationToken cancellationToken = default);

	/// <summary>Deletes multiple objects or files. Does NOT delete folders or directories.</summary>
	/// <param name="objectPaths">Full paths of the objects or folders.</param>
	/// <param name="cancellationToken">Cancellation token.</param>
	Task DeleteObjects(IEnumerable<string> objectPaths, CancellationToken cancellationToken = default);

	/// <summary>Deletes multiple objects or files. Does NOT delete folders or directories.</summary>
	/// <param name="blobs">Objects to delete.</param>
	/// <param name="cancellationToken">Cancellation token.</param>
	Task DeleteObjects(IEnumerable<StoreObject> blobs, CancellationToken cancellationToken = default);



	// ---------------------------------------------------------------------
	// Directory Transfer
	// ---------------------------------------------------------------------

	/// <summary>
	/// Downloads all files from a remote folder to a local folder recursively.
	/// Missing local directories are created automatically.
	/// Will call the given `progress` callback per file upon success or failure.
	/// Absorbs all errors internally, and does not abort the entire process if a single file failed to transfer.
	/// Only objects matching the given `rules` are downloaded (if any are given).
	/// Always returns all the per-file progress objects which contain its transfer status.
	/// </summary>
	/// <param name="remoteFolder">Remote folder or virtual folder to download.</param>
	/// <param name="localFolder">Destination local folder.</param>
	/// <param name="existsMode">How to handle files that already exist locally.</param>
	/// <param name="progress">Callback to report the progress of each file transfer.</param>
	/// <param name="rules">Only objects passing all the given whitelist/blacklist rules will be downloaded.</param>
	/// <param name="cancellationToken">Cancellation token.</param>
	Task<List<StorageProgress>> DownloadDirectory(
		string remoteFolder,
		string localFolder,
		StorageExists existsMode = StorageExists.Skip,
		Action<StorageProgress>? progress = null,
		IList<StorageRule> rules = null,
		CancellationToken cancellationToken = default);

	/// <summary>
	/// Uploads all files from a local folder to a remote folder recursively.
	/// For file system providers, missing remote directories are created automatically.
	/// For object storage providers, files are uploaded as objects using their relative paths.
	/// Will call the given `progress` callback per file upon success or failure.
	/// Absorbs all errors internally, and does not abort the entire process if a single file failed to transfer.
	/// Only objects matching the given `rules` are uploaded (if any are given).
	/// Always returns all the per-file progress objects which contain its transfer status.
	/// </summary>
	/// <param name="localFolder">Local folder to upload.</param>
	/// <param name="remoteFolder">Destination remote folder or virtual folder.</param>
	/// <param name="existsMode">How to handle files that already exist on the store.</param>
	/// <param name="progress">Callback to report the progress of each file transfer.</param>
	/// <param name="rules">Only objects passing all the given whitelist/blacklist rules will be uploaded.</param>
	/// <param name="cancellationToken">Cancellation token.</param>
	Task<List<StorageProgress>> UploadDirectory(string localFolder,
		string remoteFolder,
		StorageExists existsMode = StorageExists.Skip,
		Action<StorageProgress>? progress = null,
		IList<StorageRule> rules = null,
		CancellationToken cancellationToken = default);


	// ---------------------------------------------------------------------
	// Presigned URL
	// ---------------------------------------------------------------------

	/// <summary>
	/// Get a pre-signed URL to upload an object to this bucket.
	/// </summary>
	/// <param name="objectPath">Full path of the object</param>
	/// <param name="https">true for to require HTTPS, false to permit HTTP and HTTPS</param>
	/// <param name="expiresInSeconds">Number of seconds until the URL expires.</param>
	Task<string> GetUploadUrl(string objectPath, bool https, int expiresInSeconds = 86000);

	/// <summary>
	/// Get a pre-signed URL to download an object from this bucket.
	/// </summary>
	/// <param name="objectPath">Full path of the object</param>
	/// <param name="https">true for to require HTTPS, false to permit HTTP and HTTPS</param>
	/// <param name="expiresInSeconds">Number of seconds until the URL expires.</param>
	Task<string> GetDownloadUrl(string objectPath, bool https, int expiresInSeconds = 86000);

	/// <summary>
	/// Generates a pre-signed URL or SAS for the specified object. S3-friendly API which is supported on all cloud providers.
	/// The URL grants temporary access to the object and expries after the specified duration. MIME type is auto computed.
	/// </summary>
	/// <param name="objectPath">Full path of the object</param>
	/// <param name="forDownload">true to generate a download URL, false to generate an upload URL.</param>
	/// <param name="https">true for to require HTTPS, false to permit HTTP and HTTPS</param>
	/// <param name="expiresInSeconds">Number of seconds until the URL expires.</param>
	Task<string> GetPresignedUrl(string objectPath, bool forDownload, bool https, int expiresInSeconds = 86000);

	/// <summary>
	/// Generates a pre-signed URL or SAS for the specified object. Azure-friendly API which is supported on all cloud providers.
	/// The URL grants temporary access to the object and expries after the specified duration.
	/// </summary>
	/// <param name="objectPath">Full path of the object</param>
	/// <param name="options">Options controlling permissions, expiration, protocol, and other Shared Access Signature settings.</param>
	Task<string> GetObjectSas(string objectPath, StorageUrlOptions options);


	// ---------------------------------------------------------------------
	// File Systems Only
	// ---------------------------------------------------------------------

	/// <summary>
	/// Gets information about the connected FTP/SFTP server.
	/// </summary>
	Task<Dictionary<string, object>> GetServer(CancellationToken cancellationToken = default);

	/// <summary>Creates a folder on the store, if it does not already exist.</summary>
	/// <param name="folderPath">Path to the new folder.</param>
	Task CreateDirectory(string folderPath, bool force, CancellationToken cancellationToken = default);

	/// <summary>Deletes a folder from the store, if it exists. Deletes all contents if `recursive` is true.</summary>
	/// <param name="folderPath">Path to the folder.</param>
	Task DeleteDirectory(string folderPath, bool recursive, CancellationToken cancellationToken = default);

	/// <summary>
	/// Returns true if the specified directory or virtual directory exists.
	/// </summary>
	Task<bool> DirectoryExists(string folderPath,CancellationToken cancellationToken = default);

	/// <summary>
	/// Moves a directory or virtual directory.
	/// </summary>
	Task MoveDirectory(string sourceFolderPath,string destinationFolderPath,CancellationToken cancellationToken = default);

	/// <summary>
	/// Renames a directory or virtual directory.
	/// </summary>
	//Task RenameDirectory(string folderPath,string newFolderName,CancellationToken cancellationToken = default);

	/// <summary>
	/// Gets the CHMOD permissions of a file (FTP/SFTP only).
	/// </summary>
	Task<int> GetFilePermissions(string filePath,CancellationToken cancellationToken = default);

	/// <summary>
	/// Sets the CHMOD permissions of a file (FTP/SFTP only).
	/// </summary>
	Task SetFilePermissions(string filePath,int permissions,CancellationToken cancellationToken = default);


	// ---------------------------------------------------------------------
	// Versioning
	// ---------------------------------------------------------------------

	/// <summary>
	/// Returns all available versions of the specified object.
	/// Returns an empty collection if versioning is not enabled, or no versions exist, or the object does not exist.
	/// </summary>
	Task<List<StorageObjectVersion>> ListObjectVersions(string objectPath, CancellationToken cancellationToken = default);

	/// <summary>
	/// Returns information about a specific version of an object.
	/// Returns null if the version or object does not exist.
	/// </summary>
	Task<StorageObjectVersion> GetObjectVersion(string objectPath, string versionId, CancellationToken cancellationToken = default);

	/// <summary>
	/// Restores the specified version as the current version of the object.
	/// Provider-specific restore semantics may apply.
	/// Returns true if restored, or false if the object was not found.
	/// </summary>
	Task<bool> RestoreObjectVersion(string objectPath, string versionId, CancellationToken cancellationToken = default);

	/// <summary>
	/// Permanently deletes the specified object version.
	/// Does not delete other versions of the object.
	/// Returns true if deleted, or false if the object was not found.
	/// </summary>
	Task<bool> DeleteObjectVersion(string objectPath, string versionId, CancellationToken cancellationToken = default);



	// ---------------------------------------------------------------------
	// Object Tags
	// ---------------------------------------------------------------------

	/// <summary>
	/// Returns all tags associated with the specified object.
	/// Returns an empty collection if no tags exist.
	/// Returns null if the object cannot be found.
	/// </summary>
	Task<Dictionary<string, string>> GetObjectTags(string objectPath, CancellationToken cancellationToken = default);

	/// <summary>
	/// Replaces all tags associated with the specified object.
	/// Existing tags are removed before the new tags are applied.
	/// Returns true if succeeded, or false if the object cannot be found.
	/// </summary>
	Task<bool> SetObjectTags(string objectPath, Dictionary<string, string> tags, CancellationToken cancellationToken = default);

	/// <summary>
	/// Removes all tags from the specified object.
	/// Does nothing if the object has no tags.
	/// Returns true if succeeded, or false if the object cannot be found.
	/// </summary>
	Task<bool> DeleteObjectTags(string objectPath, CancellationToken cancellationToken = default);


	// ---------------------------------------------------------------------
	// Storage Tier or Class
	// ---------------------------------------------------------------------

	/// <summary>
	/// Returns the storage tier/class of the specified object.
	/// Returns `StorageTier.NotFound` if the object cannot be found, or `StorageTier.Unknown` if the tier is not a known type.
	/// </summary>
	Task<StorageTier> GetObjectTier(string objectPath, CancellationToken cancellationToken = default);

	/// <summary>
	/// Changes the storage tier/class of the specified object.
	/// Returns true if succeeded, or false if the object cannot be found.
	/// Throws `StorageException` if the provider does not support the given tier.
	/// </summary>
	Task<bool> SetObjectTier(string objectPath, StorageTier tier, CancellationToken cancellationToken = default);


	// ---------------------------------------------------------------------
	// Retention Policy
	// ---------------------------------------------------------------------
	/*
	/// <summary>
	/// Returns the retention policy applied to the specified object.
	/// Returns null if no retention policy is configured, or the object cannot be found.
	/// </summary>
	Task<StorageRetentionPolicy> GetObjectRetentionPolicy(string objectPath, CancellationToken cancellationToken = default);

	/// <summary>
	/// Applies or updates the retention policy for the specified object.
	/// Existing retention settings are replaced.
	/// Returns true if succeeded, or false if the object cannot be found.
	/// </summary>
	Task<bool> SetObjectRetentionPolicy(string objectPath, StorageRetentionPolicy policy, CancellationToken cancellationToken = default);

	/// <summary>
	/// Removes the retention policy from the specified object.
	/// The provider may prevent removal while the object is protected.
	/// Returns true if succeeded, or false if the object cannot be found.
	/// </summary>
	Task<bool> ClearObjectRetentionPolicy(string objectPath, CancellationToken cancellationToken = default);
	*/

	// ---------------------------------------------------------------------
	// Object Lock
	// ---------------------------------------------------------------------

	/*/// <summary>
	/// Returns the object lock configuration for the specified object.
	/// Returns null if object locking is not enabled, or the object cannot be found.
	/// Returns true if succeeded, or false if the object cannot be found.
	/// </summary>
	Task<StorageObjectLock> GetObjectLock(string objectPath, CancellationToken cancellationToken = default);

	/// <summary>
	/// Applies or updates the object lock configuration for the specified object.
	/// Returns true if succeeded, or false if the object cannot be found.
	/// </summary>
	Task<bool> SetObjectLock(string objectPath, StorageObjectLock objectLock, CancellationToken cancellationToken = default);

	/// <summary>
	/// Removes the object lock from the specified object.
	/// The provider may prevent removal while the object is protected.
	/// Returns true if succeeded, or false if the object cannot be found.
	/// </summary>
	Task<bool> ClearObjectLock(string objectPath, CancellationToken cancellationToken = default);*/


	// ---------------------------------------------------------------------
	// Checksums
	// ---------------------------------------------------------------------

	/// <summary>Returns the checksum of an object using the required hash algorithm. Returns null if the object cannot be found.</summary>
	/// <param name="fullPath">Full path of the object.</param>
	/// <param name="hash">Hash algorithm to use. MD5 is the most commonly supported but is a weak algorithm, so CRC32 or SHA is recommended.</param>
	/// <param name="cancellationToken">Cancellation token.</param>
	Task<StorageObjectHash> GetObjectChecksum(string fullPath, StorageHash hash = StorageHash.MD5, CancellationToken cancellationToken = default);


}
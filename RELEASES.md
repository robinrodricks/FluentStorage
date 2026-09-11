# Release Notes

**Please read the [Migration Guide](https://github.com/robinrodricks/FluentStorage/wiki/Migration-Guide) to help you migrate from older versions to FluentStorage 8!**

#### FluentStorage 8.0.21
 - **FluentStorage**
   - Fix: `MoveDirectory` on local disk provider no longer throws `ArgumentNullException`
   - Fix: `MoveDirectory` on local disk provider will now throw `IOException` when the destination folder exists
 - **FluentStorage.AWS**
   - Fix: Blank collections returned by S3 and SQS API calls should not throw `NullReferenceException`
   - Fix: `ListObjects`/`ListDirectory` correctly handle bucket paths and do not truncate the first path character (affects S3 and all S3-compatible providers)
 - **FluentStorage.Azure.Blobs**
   - Fix: `ListObjects`/`ListDirectory` correctly handle bucket paths and do not truncate the first path character
 - **FluentStorage.Azure.KeyVault**
   - Fix: `ListObjects`/`ListDirectory` correctly handle bucket paths and do not truncate the first path character

#### FluentStorage 8.0.20
 - **FluentStorage**
   - Change: Use file-scoped namespaces, remove unused using directives and remove unnecessary `this` keyword usage
   - Change: Remove unnecessary async usage and add missing API docs
 - **FluentStorage.AWS**
   - Fix: `MaxResults` loop guard in `S3DirectoryBrowser.ListFolderAsync` to fix timeout errors in a large S3 bucket

#### FluentStorage 8.0.19
 - **FluentStorage.Git**
   - New: Git repository provider that allows users to read/write files within a Git repo using the standard FluentStorage `IStore` interface
   - New: Connection string support for Git repository provider using `git` URL prefix
   - New: Git API prefixed with `Git*` on `GitStore`

#### FluentStorage 8.0.17
 - **FluentStorage.Azure.Blobs**
   - Fix: Azure Blob paths lose their first character due to path handling logic
 
#### FluentStorage 8.0.16
 - **FluentStorage**
   - New: `OverwriteByTimestamp` and `OverwriteByChecksum` modes to overwrite files based on timestamp or checksum
   - New: `UploadDirectory` will return a list of all the per-file progress reports at the end, and will record objects skipped due to rules
   - New: `DownloadDirectory` will return a list of all the per-file progress reports at the end, and will record objects skipped due to rules
   - New: `SkipReason` inside each `StorageProgress` to indicate why the object was skipped
   - New: `SkipRule` inside each `StorageProgress` to indicate which rule caused the object to be rejected
   - Change: Rename `StorageExistsMode` to `StorageExists` to reduce bloat
   - Fix: `UploadDirectory` and `DownloadDirectory` logic that caused it not to overwrite files
 - **FluentStorage.SFTP**
   - Fix: `CreateDirectory` does not create the entire directory path
 
#### FluentStorage 8.0.14
 - **FluentStorage**
   - New: `GetObjectChecksum` API to compute various types of hashes for an object across all providers
   - New: `StorageObjectHash` object to hold a hash value and easily compare it against any local file or stream
   - New: `StorageExistsMode.OverwriteIfChanged` mode to only overwrite files if they have changed (length/checksum checks)
   - New: `UploadDirectory` supports the `OverwriteIfChanged` mode and only uploads files if the length/checksum mismatches
   - New: `DownloadDirectory` supports the `OverwriteIfChanged` mode and only downloads files if the length/checksum mismatches
   - Removed: `GetObjectMD5` is removed in favour of `GetObjectChecksum`
 - **FluentStorage.GCP**
   - New: Implement fast `GetObjectChecksum` by using `GetObjectAsync` API to get MD5 object hash
 - **FluentStorage.FTP**
   - New: Implement fast `GetObjectChecksum` by using native FTP commands
 - **FluentStorage.SFTP**
   - New: Implement fast `GetObjectChecksum` by using native SSH shell commands to remotely compute an object hash.
   - New: Implement a powerful engine built for Windows and Unix to detects which Shell utilities are available once, then quickly use this information to compute a server-side hash of the file without needing to download it
   - New: Security engine to prevent path-injection and command-injection (based on security engine from FluentFTP)
 
#### FluentStorage 8.0.13
 - **FluentStorage**
   - New: Optimize `StoragePath.Combine` when combining only 2 path segments (most common use case)
 - **FluentStorage.SFTP**
   - New: Optimize `SetObject` & `SetBytes` APIs to improve transfer performance (4x faster)
   - New: Optimize `UploadObject` & `UploadDirectory` APIs to improve transfer performance (2x faster)
   - New: `BufferSize` set to a default of 128KB to improve transfer performance
   - New: `TransferBufferSize` setting to change upload/download buffer size
   - Fix: `SetObject` now correctly handles non-seekable streams
   - Fix: `CreateDirectory` now creates the entire directory path instead of just the last segment
   - Improve: `DirectoryExists` uses only one native API call to improve performance
   - Improve: `UploadObject` now uses native `UploadFileAsync` API to improve transfer performance
   - Remove: `SetLengthOnNewStream` as it is no longer supported
 
#### FluentStorage 8.0.12
 - **FluentStorage**
   - New: Add rule-based filtering support in `DownloadDirectory` and `UploadDirectory` for all storage providers
   - New: Add rule engine and rules from FluentFTP: `ObjectNameRule`, `ObjectNameRegexRule`, `DirectoryNameRule`, `DirectoryNameRegexRule`, `ExtensionRule`
   - New: Add new rule types for path checking: `FullPathRule`, `FullPathRegexRule`
   - Fix: Fix all logic issues in rules based on unit test suite
   - Fix: Support case-insensitive extension comparison in `ExtensionRule`
   - Fix: `DownloadObject` ensures parent directory exists and fix existance check
   - Fix: `DownloadDirectory` reports progress on successful transfers too
   - Fix: `UploadDirectory` reports progress on successful transfers too
   - Paths: `StoreObject`: Make `FullPath` instant by computing it at time of construction rather than dynamically
   - Paths: `StoreObject`: Add `Input` property to read the raw path given by the provider
 - **FluentStorage.FTP**
   - Fix: Improve `DownloadObject` API to have consistant behaviour
 - **FluentStorage.SFTP**
   - Fix: Improve `DownloadObject` API to have consistant behaviour
 - **FluentStorage.Alibaba**
   - Fix: All API should use a single constructor for `StoreObject`
 - **FluentStorage.Tests**
   - New: Extensive integration test suite for rule engine under `IStore.DirRules` class
   - New: Extensive unit test suite for rule engine under `RuleTests` class
 
#### FluentStorage 8.0.10
 - **FluentStorage**
   - Fix: `CreateDirectory` will not throw exceptions by default, allowing it to be called on cloud stores
   - Fix: `DeleteDirectory` has a default implementation for all cloud stores, to delete all objects in a virtual folder
   - Change: `DiskStore`: `DeleteObject` will no longer delete objects in a virtual directory
   - Change: `MemoryStore`: `DeleteObject` will no longer delete objects in a virtual directory
   - Fix: `MemoryStore`: `ObjectExists` normalizes the input path
   - New: `MemoryStore`: Implement all missing API: `OpenRange`, `OpenWrite`, `IsSeekable`, `GetObjectLength`, `CreateDirectory`, `DirectoryExists`, `DeleteDirectory`, `MoveDirectory`, `MoveObject`
 - **FluentStorage.AWS**
   - New: Implement fast `DirectoryExists` virtual directory check by using a `ListObjectsV2` query with `MaxKeys` set
   - Fix: Fix access modifiers for `DigitalOceanSpacesStorage` factory API
   - Fix: All API now calls `StoragePath.Normalize` to support the unified path system
   - Change: `DeleteObject` will no longer delete objects in a virtual directory
 - **FluentStorage.Azure.Blobs**
   - New: Implement fast `DirectoryExists` virtual directory check by using a `GetBlobsAsync` query
 - **FluentStorage.Azure.Files**
   - Change: `DeleteObject` will no longer delete objects in a virtual directory
 - **FluentStorage.GCP**
   - New: Implement fast `DirectoryExists` virtual directory check by using a `ExecuteAsync` query with `PageSize` set
   - Fix: All API now calls `StoragePath.Normalize` to support the unified path system
   - Fix: `OpenRange` will not throw when the object is missing (consistant with other API)
   - Change: `DeleteObject` will no longer delete objects in a virtual directory
 - **FluentStorage.FTP**
   - Fix: All API now calls `StoragePath.Normalize` to support the unified path system
   - Change: `DeleteObject` will no longer delete objects in a virtual directory
 - **FluentStorage.SFTP**
   - Fix: `DeleteDirectory` no longer normalizes paths multiple times
 - **FluentStorage.Alibaba**
   - New: Implement fast `DirectoryExists` virtual directory check by using a `ListObjects` query with `MaxKeys` set
 - **FluentStorage.Minio**
   - New: Implement fast `DirectoryExists` virtual directory check by using a `ListObjectsArgs` query
   - Fix: `OpenRead`: Opening non-existant objects will no longer throw exceptions
   - Fix: `OpenRange`: Opening non-existant objects will no longer throw exceptions
   - Fix: `OpenWrite` will return null if the object already exists, instead of throwing exceptions
   - Fix: Consistantly catch object not found exceptions using `ObjectNotFoundException`
 - **FluentStorage.Mongo**
   - New: Implement fast `DirectoryExists` virtual directory check by querying for a single file whose filename begins with the directory prefix
   - Fix: `OpenRead`: Opening non-existant objects will no longer throw exceptions
   - Fix: `OpenRange`: Opening non-existant objects will no longer throw exceptions
   - Fix: `OpenWrite` will return null if the object already exists, instead of throwing exceptions
   - Fix: Credential connection uses `SCRAM-SHA-256` by default
 - **FluentStorage.Tests**
   - New: Add provider-specific config settings for Alibaba OSS, Backblaze B2, Cloudflare R2, DigitalOcean Spaces, Hetzner, Minio S3, Minio Native, Vultr, Wasabi, Mongo GridFS
   - New: Add integration test suites for Alibaba OSS, Backblaze B2, Cloudflare R2, DigitalOcean Spaces, Hetzner, Minio S3, Minio Native, Vultr, Wasabi, Mongo GridFS
   - New: Update `fluentstorage.yaml.template` with new provider-specific settings

#### FluentStorage 8.0.9
 - **FluentStorage**
   - Fix: `UploadDirectory` is more efficient for filesystem-based storage providers
 - **FluentStorage.Tests**
   - Add support for loading test config from YAML file `fluentstorage.yaml`
   - Add `fluentstorage.yaml.template` based on required test config schema
   - Add tests for FTP and SFTP using new config system
   - Add provider-specific assertions to ensure that required test config settings are filled
   - Remove unused `LogMagic` and `Config.Net` packages, and add `YamlDotNet`
   - Split `IStoreTest` into multiple partial classes and improve test codebase organization
   - New tests for directory upload, directory download, roundtrip
   - Fix: Disable tests that are malfunctioning with FTP/SFTP
 - **FluentStorage.FTP**
   - Fix: Any file uploads/writes will automatically create the directory structure as required
   - Fix: `CreateDirectory`: No error is thrown if the folder already exists
   - Fix: `SetBytes` now correctly uploads zero-byte file data
 - **FluentStorage.SFTP**
   - Fix: All API now uses Async API of `SftpClient` rather than the old sync API
   - Fix: `DownloadObject` and `UploadObject` had invalid file existance checks
   - Fix: `DownloadObject` now correctly downloads file data (fix stream disposing issue)
   - Fix: `SetBytes` and `UploadObject` will automatically create the directory structure as required
   - Fix: `SetBytes` now correctly uploads zero-byte file data
   - Fix: `CreateDirectory`: No error is thrown if the folder already exists
   - Fix: `GetObjectInfo` and `GetObjectsInfo` correctly computes SFTP remote paths
   - Fix: `MaxResults` and `BrowseFilter` filtering is more efficient during directory listing
   - Fix: `ListDirectory` and `ListObjects` will return a blank list if nothing is found instead of throwing an error
   - Fix: `ListDirectory` and `ListObjects` will correctly enumerate files inside subdirectories
 - **FluentStorage.Azure.Queues**
   - Remove all usage of Newtonsoft.Json
   - Cleanup JSON conversion and move all static utilities into `FluentStorage.Azure.Queues.Utils` NS

#### FluentStorage 8.0.8
 - **FluentStorage**
   - `OpenSeekable`: Return null if the stream cannot be created or the object length cannot be determined

#### FluentStorage 8.0.7
 - **FluentStorage**
   - Transfer: Add new APIs to `DownloadDirectory` and `UploadDirectory`, with default implementation for all stores
   - Paths: Added `StoragePath.GetRelativeDiskPath` and `StoragePath.GetRelativeCloudPath` to compute relative paths
 - **FluentStorage.FTP**
   - Transfer: Special handling using native FluentFTP APIs for `DownloadDirectory` and `UploadDirectory`

#### FluentStorage 8.0.6
 - **FluentStorage**
   - **Paths: A new [unified path system](https://github.com/robinrodricks/FluentStorage/wiki/Unified-Path-System) has been introduced across all providers!**
   - Paths: `StoragePath.Normalize` has been completely rewritten to enable the unified path system
   - Paths: `StoragePath.Combine`, `.Split` and `.GetParent` are completely rewritten for performance
   - Paths: `StoragePath.ComparePath`, `.RemoveRootFolder`, `.GetRootFolder` and `.Rename` have been removed
   - DiskStore: `GetObjects` will now return all objects in the main directory recursively by default
   - Move all sinks into `FluentStorage.Sinks` NS
   - Move `DelegatedStream` and `NonCloseableStream` into `FluentStorage.Storage.Sinks` NS
   - Rename `SinkedBlobStorage` to `SinkedStore`
   - Remove `RandomGenerator` and move it into testing project
 - **FluentStorage.SFTP**
   - Paths: New SFTP path normalization system that works with updated `StoragePath` normalization
 - **FluentStorage.GCP**
   - Paths: All APIs updated to use the new `StoragePath` normalization to support unified paths
 - **FluentStorage.Minio**
   - Paths: All APIs updated to use the new `StoragePath` normalization to support unified paths
 - **FluentStorage.Mongo**
   - Paths: All APIs updated to use the new `StoragePath` normalization to support unified paths
 - **FluentStorage.Alibaba**
   - Paths: All APIs updated to use the new `StoragePath` normalization to support unified paths
 - **FluentStorage.Tests**
   - Merge all tests into a single project, cleanly seperating unit and integration tests
   - New tests for path normalization (`StoragePath.Normalize`), path combination (`StoragePath.Combine`) and path splitting (`StoragePath.Split`)
   - New tests for `SeekableStream` used in seeking/streaming
   - New tests for `IStore` operations and fixes made to local disk implementation
   
#### FluentStorage 8.0.5
 - **FluentStorage**
   - IStore: Implement new object versioning APIs, tagging APIs and storage tier APIs
   - IStore: Change `IsFileSystem`, `IsSeekable` to async to align with other feature check APIs
   - IStore: Add `IsVersioned`, `IsTagged` and `IsTiered` to check if providers support new features
   - Assertions: Remove `AssertFullPath` and handle it inline in all APIs
 - **FluentStorage.AWS**
   - Add versioning APIs: `IsVersioned`, `ListObjectVersions`, `GetObjectVersion`, `RestoreObjectVersion`, `DeleteObjectVersion`
   - Add tagging APIs: `IsTagged`, `GetObjectTags`, `SetObjectTags`, `DeleteObjectTags`
   - Add storage tier APIs: `IsTiered`, `GetObjectTier`, `SetObjectTier`
 - **FluentStorage.Azure.Blob**
   - Add versioning APIs: `IsVersioned`, `ListObjectVersions`, `GetObjectVersion`, `RestoreObjectVersion`, `DeleteObjectVersion`
   - Add tagging APIs: `IsTagged`, `GetObjectTags`, `SetObjectTags`, `DeleteObjectTags`
   - Add storage tier APIs: `IsTiered`, `GetObjectTier`, `SetObjectTier`
 - **FluentStorage.GCP**
   - Rewrite path normalization to be simpler and not overly aggressive
   - Add versioning APIs: `IsVersioned`, `ListObjectVersions`, `GetObjectVersion`, `RestoreObjectVersion`, `DeleteObjectVersion`
   - Add tagging APIs: `IsTagged`, `GetObjectTags`, `SetObjectTags`, `DeleteObjectTags`
   - Add storage tier APIs: `IsTiered`, `GetObjectTier`, `SetObjectTier`
 - **FluentStorage.Alibaba**
   - Add tagging APIs: `IsTagged`, `GetObjectTags`, `SetObjectTags`, `DeleteObjectTags`
 - **FluentStorage.Minio**
   - Add tagging APIs: `IsTagged`, `GetObjectTags`, `SetObjectTags`, `DeleteObjectTags`
   - Add storage tier APIs: `IsTiered`, `GetObjectTier`, `SetObjectTier`

#### FluentStorage 8.0.4
 - **FluentStorage**
   - Disk: Fix `ListObjects` and add `GetServer` & `MoveObject` APIs
   - Streams: Add `NonSeekableStream` to the core package
   - Rename core classes: `StoreObject`, `MemoryMessenger`, `DiskStore`, `MemoryStore`
   - Move factory classes from `FluentStorage.AWS.Factory`, `FluentStorage.Alibaba.Factory` and `FluentStorage.Minio.Factory` to root NS
 - **FluentStorage.Mongo**
   - New: MongoDB GridFS storage provider using native MongoDB driver. Introduces the factory API `MongoGridStorage` and store class `MongoGridStore`.

#### FluentStorage 8.0.3
 - Add APIs to all providers: `GetObjectLength` and integrate it with streaming/seeking implementation

#### FluentStorage 8.0.2
 - Add streaming/seeking APIs to all providers: `OpenRange`, `OpenSeekable`

#### FluentStorage 8.0.1
 - Fix some issues with the original v8 release.

#### FluentStorage 8.0.0
 - **FluentStorage**
   - All [cloud storage API](https://github.com/robinrodricks/FluentStorage#polycloud-api) methods have been redesigned to improve productivity and ease of use.
   - All extension-method factory API has been removed: For example `StorageFactory.Blobs` and `StorageFactory.Messages` no longer exist.
   - Factory classes have been introduced on a [per-provided basis](https://github.com/robinrodricks/FluentStorage#storage-providers).
   - Factory classes can be accessed directly to construct new `IStore` objects: For example `AwsS3Storage.FromCredentials` rather than `StorageFactory.Blobs.AwsS3`.
   - All API methods are `async` and the "Async" suffix has been dropped.
   - All API methods have an optional `CancellationToken` rather than forcing users to provide one.
   - At initialization, S3-compatible stores will no longer create the bucket if the specified bucket does not exist.
   - `WriteAsync()` is renamed to `SetObject()` and will auto compute the object's MIME type (`Content-Type`) if it is not supplied.
   - `GetPresignedUrl()` and its variations will auto compute the object's MIME type (`Content-Type`).
   - `CreateDirectory()` will no longer create a "dummy file" in a cloud storage bucket.
   - `RenameAsync()` is renamed to `MoveObject()` and will no longer perform a recursive copy and delete, instead it will efficiently move using native operations.
   - `ExistsAsync()` is renamed to `ObjectExists()` and will consistently return true/false if the file or folder exists on the bucket/server.
   - `DeleteAsync()` is renamed to `DeleteObject()` and will consistently delete a single file or folder from the bucket/server.
   - `DeleteObjects()` will consistently delete multiple files or folders from the bucket/server.
   - `GetClient()` will consistently return the internal cloud SDK client for all types of cloud storage. (Replaces `NativeBlobClient`)
   - Collections returned by APIs will always be `List` instead of `IReadOnlyCollection`. 
   - Exceptions are moved into the `FluentStorage.Exceptions` namespace
   - Enums are moved into the `FluentStorage.Enums` namespace
   - Bucket stores now support [streaming/seeking, presigned URL and more object manipulation API](https://github.com/robinrodricks/FluentStorage/wiki/Migration-Guide#new-bucket-api)
   - We no longer support [Databricks, ServiceFabric, EventHub](https://github.com/robinrodricks/FluentStorage/wiki/Migration-Guide#deleted-libraries), [Virtual storage and ZIP archives](https://github.com/robinrodricks/FluentStorage/wiki/Migration-Guide#deleted-api)
 - **FluentStorage.AWS**
   - Breaking changes to [entire API surface](https://github.com/robinrodricks/FluentStorage/wiki/Migration-Guide#renamed-api) to improve productivity and ease of use.
   - New API: `GetClient`, `MoveObject`, `DownloadObject`, `UploadObject`, `OpenRead`, `OpenWrite`, `GetUploadUrl`, `GetDownloadUrl`, `GetPresignedUrl`, `GetObjectSas`, `OpenRange`, `OpenSeekable`, `GetObjectLength`.
 - **FluentStorage.GCP**
   - Breaking changes to [entire API surface](https://github.com/robinrodricks/FluentStorage/wiki/Migration-Guide#renamed-api) to improve productivity and ease of use.
   - New API: `GetPresignedUrl`, `GetObjectSas`, `MoveObject`, `DeleteDirectory`, `OpenRange`, `OpenSeekable`, `GetObjectLength`.
 - **FluentStorage.Azure.Blobs**
   - Breaking changes to [entire API surface](https://github.com/robinrodricks/FluentStorage/wiki/Migration-Guide#renamed-api) to improve productivity and ease of use.
   - New API: `GetClient`, `MoveObject`, `DownloadObject`, `UploadObject`, `OpenRead`, `OpenWrite`, `GetUploadUrl`, `GetDownloadUrl`, `GetPresignedUrl`, `GetObjectSas`, `OpenRange`, `OpenSeekable`, `GetObjectLength`.
 - **FluentStorage.Azure.Files**
   - Breaking changes to [entire API surface](https://github.com/robinrodricks/FluentStorage/wiki/Migration-Guide#renamed-api) to improve productivity and ease of use.
   - New API: `GetClient`, `OpenRange`, `OpenSeekable`, `GetObjectLength`.
 - **FluentStorage.Azure.ServiceBus**
   - Breaking changes to [entire API surface](https://github.com/robinrodricks/FluentStorage/wiki/Migration-Guide#renamed-api) to improve productivity and ease of use.
 - **FluentStorage.Azure.KeyVault**
   - Breaking changes to [entire API surface](https://github.com/robinrodricks/FluentStorage/wiki/Migration-Guide#renamed-api) to improve productivity and ease of use.
 - **FluentStorage.Azure.Queues**
   - Breaking changes to [entire API surface](https://github.com/robinrodricks/FluentStorage/wiki/Migration-Guide#renamed-api) to improve productivity and ease of use.
 - **FluentStorage.FTP**
   - Breaking changes to [entire API surface](https://github.com/robinrodricks/FluentStorage/wiki/Migration-Guide#renamed-api) to improve productivity and ease of use.
   - FTP has new [directory and server API](https://github.com/robinrodricks/FluentStorage/wiki/Migration-Guide#new-ftpsftp-api) for file systems
   - New API: `GetServer`, `CreateDirectory`, `DeleteDirectory`, `DirectoryExists`, `MoveDirectory`, `GetFilePermissions`, `SetFilePermissions`, `MoveObject`, `DownloadObject`, `UploadObject`, `GetBytes`, `SetBytes`, `OpenRange`, `OpenSeekable`, `GetObjectLength`.
 - **FluentStorage.SFTP**
   - Breaking changes to [entire API surface](https://github.com/robinrodricks/FluentStorage/wiki/Migration-Guide#renamed-api) to improve productivity and ease of use.
   - SFTP has new [directory and server API](https://github.com/robinrodricks/FluentStorage/wiki/Migration-Guide#new-ftpsftp-api) for file systems
   - New API: `GetServer`, `CreateDirectory`, `DeleteDirectory`, `DirectoryExists`, `MoveDirectory`, `GetFilePermissions`, `SetFilePermissions`, `MoveObject`, `DownloadObject`, `UploadObject`, `GetBytes`, `SetBytes`, `OpenRange`, `OpenSeekable`, `GetObjectLength`.
 - **FluentStorage.Alibaba**
   - New: Alibaba OSS provider using native Aliyun SDK. Introduces the factory API `AlibabaStorage` and store class `AlibabaStore`.
 - **FluentStorage.Minio**
   - New: MinIO storage provider using native Minio SDK. Introduces the factory API `MinioStorage` and store class `MinioStore`.
 - **FluentStorage.Databricks**
   - Deprecated and unlisted from Nuget since it is out of scope.
 - **FluentStorage.Azure.EventHub**
   - Deprecated and unlisted from Nuget since it has very low community usage.
 - **FluentStorage.Azure.DataLake**
   - Deprecated and unlisted from Nuget since DataLake Gen 1 is obsolete.
 - **FluentStorage.Azure.ServiceFabric**
   - Deprecated and unlisted from Nuget since it is out of scope and has very low community usage.

#### FluentStorage 7.1.1
 - **FluentStorage**
   - Add connection string support for Wasabi, DigitalOcean Spaces, Cloudflare R2 
 - **FluentStorage.AWS**
   - Add connection string support for Wasabi, DigitalOcean Spaces, Cloudflare R2 

#### FluentStorage 7.1.0
 - **FluentStorage**
   - New: All providers and sinks updated to support `WriteAsync` with content type
 - **FluentStorage.AWS**
   - New: Content type will automatically be computed for all uploaded files in S3 or S3-compatible storage (if not supplied by user)
   - New: Add support for Cloudflare R2 with new factory API `StorageFactory.Blob.CloudflareR2`
   - New: Add support for uploading files to S3 or S3-compatible storage with MIME Type using `WriteAsync`  API
   - New: Add dependency on `MimeMapping` 4.0.0 for content type computation
   - New: All providers updated to support `WriteAsync` with content type
   - Fix: Allow null content types in `GetPresignedUrlAsync` API
 - **FluentStorage.Azure.Blobs**
   - Update `MimeMapping` to version 4.0.0
 - **All providers updated to support `WriteAsync` with content type**
   - FluentStorage.Azure.Blobs
   - FluentStorage.Azure.Files
   - FluentStorage.Azure.DataLake
   - FluentStorage.Azure.KeyVault
   - FluentStorage.Azure.ServiceFabric
   - FluentStorage.FTP
     - Updated FluentFTP to 54.2.0
   - FluentStorage.SFTP
   - FluentStorage.Databricks

#### FluentStorage 7.0.0
 - **FluentStorage**
   - Fix: Make hash helpers thread-safe using new `MD5.HashData` and `SHA256.HashData` .NET APIs
 - **FluentStorage.AWS**
   - Fix: Update AWS SDK packages to 4.0.100.2
 - **FluentStorage.Azure**
   - New: Add Azure identity authentication core framework
   - New: Add dependency on `Azure.Identity` SDK package
 - **FluentStorage.Azure.Blobs**
   - New: Add dependency on `FluentStorage.Azure` package
   - Change: Migrate from legacy API to `AzureStorageIdentity` based authentication
 - **FluentStorage.Azure.Files**
   - New: Add Azure Identity authentication supporting Shared Key, Azure AD, Service Principal, Managed Identity and Token Credential based authentication
   - New: Add dependency on `Azure.Storage.Files.Shares` package
   - New: Add dependency on `FluentStorage.Azure` package
   - New: Add factory API for `AzureFiles`, `AzureFilesWithSharedKey`, `AzureFilesWithAzureAd`, `AzureFilesWithTokenCredential`, `AzureFilesWithMsi`
   - Change: Migrate from legacy API to `AzureStorageIdentity` based authentication
   - Change: Rewrite all file management APIs to use new Azure SDK API (`CreateFromAccountNameAndKey`, `ListAt`, `Write`, `OpenRead`, `GetBlob`, `SetBlob`, `SetBlobs`, `DeleteSingle`, `DeleteDirectory`, `GetFileReference`, `GetDirectoryReference`, `GetShareReference`)
   - Change: Remove dependency on outdated `Microsoft.Azure.Storage.File` SDK package
  
#### FluentStorage 6.0.4
 - **FluentStorage**
   - Fix: Potential fix for unseekable streams at `StorageSourceStream.Seek`
 - **FluentStorage.Azure.Blobs**
   - New: Sets `Content-Type` header on Azure blob uploads based on file extension to ensure proper browser rendering of files
   - New: Detects MIME type from blob path using `MimeUtility` from the `MimeMapping` Nuget package
   - New: Automatically defaults to `application/octet-stream` for unknown extensions
   - New: Sets `Content-Type` of the blob via `BlobUploadOptions` with `BlobHttpHeaders` during upload

#### FluentStorage 6.0.3
 - **FluentStorage.FTP**
   - Fix: Update FluentFTP to latest version 54.1.0
 - **FluentStorage.SFTP**
   - Fix: Update SSH.NET to latest version 2025.1.0
   - New: `SetLengthOnNewStream` setting to enable/disable calling `SetLength()` on `SftpStream` when writing new blobs
   - Fix:`OpenReadAsync` for fix issues when creating remote files where the user lacks permissions to set file attributes
 - **FluentStorage.AWS**
   - Use the `DeleteObjects` S3 API to optimize recursive deletes by using a single request
 - **FluentStorage.Azure.Blobs**
   - New: Add `AzureBlobStorage` factory method accepting `BlobServiceClient`
 - **FluentStorage.GCP**
   - Fix: Make `cred` param optional in `google.storage://` connection strings, falling back to Application Default Credentials

#### FluentStorage 6.0.2
 - **FluentStorage.FTP**
   - Fixed logic in `Dispose` method that prevented the FTP client from being disposed
   - Update FluentFTP to 53.0.2
 - **FluentStorage.AWS**
   - Added null check for `CommonPrefixes` in the `ToBlobs` method
   - Handle case where no S3 Objects exists in the response
 - **FluentStorage.Azure.Blobs**
   - Added multi-region support for Azure Blob and DataLake 

#### FluentStorage 6.0.1
 - **FluentStorage.AWS**
   - Fix: Null Exception at `ListAsync`

#### FluentStorage 6.0.0

 - **FluentStorage**
   - Allow setting text content when writing dummy files
   - Update `TestableIO` packages due to binary incompatibility 
   - Respect `createIfNotExists` in `GetFilePath` and prevent unintended directory creation
 - **FluentStorage.Azure.Blobs**
   - Allow passing `BlobClientOptions`
   - Update `OpenReadAsync` to use `OpenReadAsync` from Azure SDK
   - `OpenReadAsync` for efficient streaming over `DownloadAsync`
   - Minimise platform dependencies  
   - Update `Azure.Identity` Nuget package to latest versions
 - **FluentStorage.Azure.DataLake.Store**
   - Update `Microsoft` Nuget package to latest versions
 - **FluentStorage.AWS**
   - Allow passing the Protocol property to the `PresignedRequest`
   - Use `ContentType` when setting AWS Metadata 
   - Handle response with null S3 objects  
   - Update AWS Nuget packages from v3.7 to v4.0
 - **FluentStorage.Tests**
   - Upgrade projects from NET 6 to NET 9
 - **FluentStorage.FTP**
   - Upgrade FluentFTP to the latest version 53.0.1
 - **FluentStorage.SFTP**
   - Upgrade SSH.NET to the latest version 2025.0.0
 - **All projects**
   - Reduce .NET platform Nuget dependencies
   - Drop support for `net50`,`net60`
   - Add support for `net70`,`net80`,`net90`
   - Cleanup folder structure and add a common `build` folder for all projects

#### FluentStorage 5.6.0
 - Fix: Update to latest `Microsoft.IO.RecyclableMemoryStream` package (thanks @dammitjanet)
 - New: Use `IFileSystem` package to improve testability of `DiskDirectoryBlobStorage` (thanks @gerrewsb)

#### FluentStorage.Azure.Blobs 5.3.0
 - New: `OpenWriteAsync` API to append to existing blobs (thanks @gentledepp)
 - Fix: Update to latest Azure packages (thanks @dammitjanet)
 - Change: Swap from `MemoryStream` to `RecyclableMemoryStream` (thanks @dammitjanet)

#### FluentStorage.AWS 5.5.0
 - New: Return additional headers in `GetObjectMetadataAsync` API for access to `Content-Type` (thanks @pkdigital)

#### FluentStorage 5.5.1
 - Fix: Paths prefixed with `\` will be correctly handled in `ZipFileBlobStorage` (thanks @gerrewsb)
 - New: Adds new connection string prefix `minio.s3` to allow MinIO connections to be created using a connection string (thanks @NickHarmer)
 - Change: Default value of `ListOptions.RecursionMode` from `Local` to `Remote`, so that S3/MinIO connections will recurse remotely by default
 - New: Add support for `ListOptions.PageSize` which allows users to customize the number of objects returned per call
 - New: Added reflection utilities that can be used by other FluentStorage modules

#### FluentStorage.Azure.Blobs 5.2.5
 - Fix: `ExtendedSdk.GetHttpPipeline` needs to be manually set otherwise connection to DataLake Gen2 fails

#### FluentStorage.AWS 5.4.0
 - New: `BucketName` API in `AwsS3BlobStorage` to obtain the bucket name (thanks @Pchol)
 - New: `SetAcl` API in `AwsS3BlobStorage` to set S3 object permissions (thanks @Pchol)
 - New: Adds support for connection string prefix `minio.s3` (thanks @NickHarmer)
 - New: Add support for `ListOptions.PageSize` which allows users to customize the number of objects returned per call
 - New: Add support for `ListOptions.NumberOfRecursionThreads` to configure the number of threads

#### FluentStorage.GCP 5.3.0
 - New: Add support for `ListOptions.PageSize` which allows users to customize the number of objects returned per call
 
#### FluentStorage.Azure.Blobs 5.2.4
 - New: Add support for `ListOptions.NumberOfRecursionThreads` to configure the number of threads

#### FluentStorage.FTP 5.4.0
 - Fix: Add missing dependency to `Polly` Nuget package

#### FluentStorage.AWS 5.3.1
 - Fix: Incorrect extension method `MinIO` in AWS Storage Factory

#### FluentStorage 5.4.3
 - Support for DigitalOceanSpaces, MinIO, Wasabi

#### FluentStorage.AWS 5.3.0
 - New: `StorageFactory.Blobs.DigitalOceanSpaces` API for connecting to DigitalOcean Spaces
 - New: `StorageFactory.Blobs.MinIO ` API for connecting to MinIO storage servers
 - New: `StorageFactory.Blobs.Wasabi` API for connecting to Wasabi storage
 - New: Adds support for `serviceUrl` parameter in the `Aws.S3` connection string
 - New: Check to ensure `region` and `serviceUrl` are not specified together

#### FluentStorage.GCP 5.2.2

- Fix: `DeleteAsync` will delete all files in bucket if target object is not found
- GCP packages have been updated to their latest versions

#### Dependency Updates May 2024

The following packages have had their dependencies updated. No other changes were done.

- FluentStorage.Azure.Blobs 5.2.3
- FluentStorage.Azure.DataLake 5.2.2
- FluentStorage.Azure.EventHub 5.2.2
- FluentStorage.Azure.Files 5.2.2
- FluentStorage.Azure.KeyVault 5.2.2
- FluentStorage.Azure.Queues 5.2.2
- FluentStorage.Azure.ServiceBus 6.0.1
- FluentStorage.Azure.ServiceFabric 5.2.2
- FluentStorage.Databricks 5.2.2
- FluentStorage.FTP 5.3.1
- FluentStorage.SFTP 5.3.1

#### FluentStorage 5.4.2
(thanks timbze)
 - Fix: Disk blob storage `WriteAsync` uses `CopyToAsync` rather than sync

#### FluentStorage.FTP 5.3.0
(thanks beeradmoore)
 - New: `ListAsync` now supports recursion for listing directories using  `FtpListOption.Recursive`

#### FluentStorage.SFTP 5.3.0
(thanks beeradmoore)
 - New: `ListAsync` now supports recursion for listing directories using manual async recursion
 - Fix: `WriteAsync` now sets the length of destination stream manually to avoid resulting data being left

#### FluentStorage.Azure.ServiceBus 6.0.0
(thanks GiampaoloGabba)
 - Fix: Completely rewrite package for the new SDK `Azure.Messaging.ServiceBus`
 - New: New API for construction `AzureServiceBusTopicReceiver` and `AzureServiceBusQueueReceiver`
 - New: `IMessenger` interface uses structured string format for referencing queues/topics
 - New: `IAzureServiceBusMessenger` interface with API to send/create/delete/count a queue/topic/subscription

#### FluentStorage 5.4.1
 - Fix: Remove unused dependency package `Newtonsoft.Json` from main project

#### FluentStorage 5.4.0
(thanks dammitjanet)
 - New: Constructor for `SymmetricEncryptionSink` and `AesSymmetricEncryptionSink` to pass IV and key 
 - New: Constructor for `EncryptedSink` abstract base class to pass in IV and key
 - New: Additional tests for encryption/decryption repeatability when the decrpytion IV is known
 - Fix: Resolved Xunit errors and issue with with Xunit `FileData` attribute only finding a single file per test
 - Fix: Package updates and consolidation to latest Xunit

#### FluentStorage 5.3.0
(thanks dammitjanet)
 - New: Addition of `AesSymmetricEncryptionSink` and `WithAesSymmetricEncryption` extension
 - Fix: Obsolesence of `SymmetricEncryptionSink` and `WithSymmetricEncryption` extension
 - New: Updated Tests for `AesSymmetricEncryptionSink`
 - New: Additional Blob/Stream file tests and XUnit `FileDataAttribute` to support tests

#### FluentStorage.AWS 5.2.2
 - Fix: `AwsS3BlobStorage` checks if a bucket exists before trying to create one (thanks AntMaster7)

#### FluentStorage.Azure.Blobs 5.2.2
 - Fix: Upgrade `System.Text.Json` package from v4 to v7

#### FluentStorage 5.2.2
 - Fix: Upgrade `System.Text.Json` package from v4 to v7
 - Fix: Local storage: Handling of `LastModificationTime` and `CreatedTime`
 - Fix: Local storage: `LastAccessTimeUtc` is saved as a Universal sortable string in the Blob properties

#### FluentStorage.SFTP 5.2.3
 - Fix: Various fixes to `ListAsync` path handling
 - Fix: Upgrade `SSH.NET` package from v2016 to v2020

#### FluentStorage.SFTP 5.2.2
 - New: Added support for a root path in the SFTP connection string
 - Fix: `GetBlobsAsync` should return an array with a single null if the file does not exist
 - Fix: `WriteAsync` will create the directory if it does not exist

#### FluentStorage 5.2.1
 - New: Implement `LocalDiskMessenger.StartProcessorAsync`
 - Package: Add Nuget reference to `Newtonsoft.Json 13.0.3`
 - Package: Remove Nuget reference to `Newtonsoft.Json 12.x.x`
 - Package: Remove Nuget reference to `NetBox` and add the required utilities within this library

#### FluentStorage.AWS 5.2.1
 - New: Implement server-side filtering in `AwsS3DirectoryBrowser.ListAsync` by supplying a `FilePrefix` (thanks SRJames)

#### FluentStorage.FTP 5.2.1
 - Fix: Support for the append parameter in FluentFtpBlobStorage (thanks candoumbe)
 - Fix: `IBlobStorage.WriteAsync` will create the directory hierarchy if required (thanks candoumbe)

#### FluentStorage 5.1.1
 - Fix: Implementation of `LocalDiskMessenger.StartProcessorAsync` (issue [#14](https://github.com/robinrodricks/FluentStorage/issues/14))(`netstandard2.1` / `net6.0` and above

#### FluentStorage 5.0.0
 - New: Introducing the FluentStorage set of libraries created from Storage.NET
 - New: Added SFTP provider using [SSH.NET](https://github.com/sshnet/SSH.NET)
 - Fix: FTP provider [FluentFTP](https://github.com/robinrodricks/FluentFTP) updated to v44
 - Fix: AWS Nugets bumped to latest versions as of Jan 2023
 - Fix: All nuget packages now target `netstandard2.0`,`netstandard2.1`,`net50`,`net60`
 - Change: Refactored package structure to simplify naming scheme
 - Change: Refactored entire codebase to simplify code organization
 - New: Documentation wiki created with one page per provider
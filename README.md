<p align="center">
<img src="https://github.com/robinrodricks/FluentStorage/raw/develop/.github/logo.png" alt="FluentStorage" />
</p>

<p align="center">
<a href="https://www.nuget.org/packages/FluentStorage"><img src="https://img.shields.io/nuget/vpre/FluentStorage.svg" alt="Version" /></a>
<a href="https://www.nuget.org/packages/FluentStorage"><img src="https://img.shields.io/nuget/dt/FluentStorage.svg" alt="Downloads" /></a>
<a href="https://github.com/robinrodricks/FluentStorage/graphs/contributors"><img src="https://img.shields.io/github/contributors/robinrodricks/FluentStorage.svg" alt="GitHub contributors" /></a>
<a href="https://github.com/robinrodricks/FluentStorage/blob/develop/LICENSE"><img src="https://img.shields.io/github/license/robinrodricks/FluentStorage.svg" alt="License" /></a>
</p>

<p align="center">
    <b>FluentStorage is free, but powered by</b> <a href="https://github.com/sponsors/robinrodricks"><b>your donations</b></a>
</p>

### One Interface To Rule Them All

FluentStorage is a fully managed polycloud .NET cloud storage library, optimized for speed. It helps you interface with multiple cloud providers from a [single unified API](#polycloud-api), allowing you to switch cloud providers or support multiple cloud providers without any logic changes.

FluentStorage is released under the permissive MIT License, so it can be used in both proprietary and free/open source applications. FluentStorage is written entirely in C# with no configuration files required.

## Features

* **Common features**
  * [Unified polycloud API](#polycloud-api) for [Object storage](https://github.com/robinrodricks/FluentStorage/wiki/Object-Storage) and [Queue messaging](https://github.com/robinrodricks/FluentStorage/wiki/Message-Storage) across all [cloud storage providers](#storage-providers)
  * Each provider has its own [Nuget package](#packages) with zero configuration required.
  * Fully asynchronous API with identical behavior across all providers
  * [Unified path system](https://github.com/robinrodricks/FluentStorage/wiki/Unified-Path-System) that works across all providers
  * Object manipulation commands like create/get/set/move/delete
  * High-level API to easily upload/download files
  * High-level API to easily upload/download directories or virtual directories with [rule-based filtering](https://github.com/robinrodricks/FluentStorage/wiki/Rule-Engine), progress callbacks, ability to only re-transfer changed files, and a detailed transfer report provided at the end
  * Easily work with any object using read streams, write streams and seekable streams
  * File streaming support to provide video playback on top of object stores
  * Fast server-side checksum computation for Azure, GCP, FTP, SFTP, with a fallback to client-side hashing for other providers
  * Object metadata, including reading and writing object info, length, timestamps, etc
  * [Memory and local disk providers](https://github.com/robinrodricks/FluentStorage/wiki/Standard-Storage), so you can test on a local machine or access attached NAS/EBS drives
  * Large [suite of integration tests](https://github.com/robinrodricks/FluentStorage/wiki/Automated-Tests) and some [performance tests](https://github.com/robinrodricks/FluentStorage/wiki/Performance-Tests) that work across all providers, which are easily configured to run against your own clouds
* **Specially for object storage**
  * Presigned URL generation, with AWS-friendly and Azure-friendly API signatures
  * Virtual directory manipulation
  * Object versions, including fetching older versions, deleting a version, etc
  * Object tagging, including adding and removing tags
  * Object storage tier or storage class modification
* **Specially for SFTP and FTP**
  * Directory listing & Directory manipulation
  * Fast server-side file checksums using native FTP and SSH commands
  * SSH command engine for SFTP provider, to execute native SSH commands with automatic OS and utility detection
  * File permissions/CHMOD modification
  * Get detailed server metadata and OS information
* **Specially for Git**
  * Read and write files in a git repository working tree, with automatic or explicit commit and push
  * Object versioning mapped to git commit history
  * Restrict the store to a sub-folder of the repository using a root path


## Storage Providers

FluentStorage supports the following cloud storage providers:

|       		| Documentation Link                                               | Factory class | Store class |  `GetClient()` returns
|---------------| --------------------------------------------------------------------------- | ---------------- | ---------------- | ---------------- |
| <img src="https://raw.githubusercontent.com/robinrodricks/FluentStorage/develop/.github/providers/aws.png" width="32"></img>| [AWS S3](https://github.com/robinrodricks/FluentStorage/wiki/AWS-S3-Storage#connect-to-aws-s3)   	| `AwsS3Storage` | `S3Store` |`AmazonS3Client`  |
| <img src="https://raw.githubusercontent.com/robinrodricks/FluentStorage/develop/.github/providers/azure.png" width="32"></img>|  [Azure Blobs](https://github.com/robinrodricks/FluentStorage/wiki/Azure-Blob-Storage) 			| `AzureBlobStore` | `AzureBlobStore` | `BlobServiceClient` |
| <img src="https://raw.githubusercontent.com/robinrodricks/FluentStorage/develop/.github/providers/azure.png" width="32"></img>| [Azure Files](https://github.com/robinrodricks/FluentStorage/wiki/Azure-Files-Storage) 			| `AzureFilesStorage` | `AzureFilesStore` | `ShareServiceClient` |
| <img src="https://raw.githubusercontent.com/robinrodricks/FluentStorage/develop/.github/providers/azure-data-lake.png" width="32"></img>| [Azure DataLake](https://github.com/robinrodricks/FluentStorage/wiki/Azure-Data-Lake) 	| `AzureDataLakeStorage` | `AzureDataLakeStore` | `ExtendedSdk` |
| <img src="https://raw.githubusercontent.com/robinrodricks/FluentStorage/develop/.github/providers/gcp.png" width="32"></img>| [GCP](https://github.com/robinrodricks/FluentStorage/wiki/Google-Cloud-Storage)         			| `GoogleCloudStorage` | `GoogleCloudStore` | `StorageClient` |
| <img src="https://raw.githubusercontent.com/robinrodricks/FluentStorage/develop/.github/providers/minio.png" width="32"></img>| [MinIO (native)](https://github.com/robinrodricks/FluentStorage/wiki/MinIO-Storage)         		| `MinioStorage` | `MinioStore` |`MinioClient`  |
| <img src="https://raw.githubusercontent.com/robinrodricks/FluentStorage/develop/.github/providers/minio.png" width="32"></img>| [MinIO (S3)](https://github.com/robinrodricks/FluentStorage/wiki/MinIO-Storage)       			| `MinioS3Storage` | `S3Store` |`AmazonS3Client`  |
| <img src="https://raw.githubusercontent.com/robinrodricks/FluentStorage/develop/.github/providers/r2.png" width="32"></img>| [Cloudflare R2](https://github.com/robinrodricks/FluentStorage/wiki/Cloudflare-R2-Storage)  			| `CloudflareR2Storage` | `S3Store` |`AmazonS3Client`  |
| <img src="https://raw.githubusercontent.com/robinrodricks/FluentStorage/develop/.github/providers/digitalocean.png" width="32"></img>|[DigitalOcean Spaces](https://github.com/robinrodricks/FluentStorage/wiki/DigitalOcean-Spaces-Storage)  |`DigitalOceanSpacesStorage` | `S3Store` |`AmazonS3Client`  |
| <img src="https://raw.githubusercontent.com/robinrodricks/FluentStorage/develop/.github/providers/wasabi.png" width="32"></img>| [Wasabi](https://github.com/robinrodricks/FluentStorage/wiki/Wasabi-Storage)         			| `WasabiStorage` | `S3Store` |`AmazonS3Client`  |
| <img src="https://raw.githubusercontent.com/robinrodricks/FluentStorage/develop/.github/providers/backblaze.png" width="32"></img>|  [Backblaze B2](https://github.com/robinrodricks/FluentStorage/wiki/Backblaze-B2-Storage)  	| `BackblazeB2Storage` | `S3Store` |`AmazonS3Client`  |
| <img src="https://raw.githubusercontent.com/robinrodricks/FluentStorage/develop/.github/providers/hetzner.png" width="32"></img>|  [Hetzner](https://github.com/robinrodricks/FluentStorage/wiki/Hetzner-Storage)  				| `HetznerStorage` | `S3Store` |`AmazonS3Client`  |
| <img src="https://raw.githubusercontent.com/robinrodricks/FluentStorage/develop/.github/providers/vultr.png" width="32"></img>|  [Vultr](https://github.com/robinrodricks/FluentStorage/wiki/Vultr-Storage)        				| `VultrStorage` | `S3Store` |`AmazonS3Client`  |
| <img src="https://raw.githubusercontent.com/robinrodricks/FluentStorage/develop/.github/providers/mongo.png" width="32"></img>| [MongoDB GridFS](https://github.com/robinrodricks/FluentStorage/wiki/MongoDB-GridFS-Storage)      | `MongoGridStorage` | `MongoGridStore` |`MongoClient`  |
| <img src="https://raw.githubusercontent.com/robinrodricks/FluentStorage/develop/.github/providers/alibaba.png" width="32"></img>|  [Alibaba OSS](https://github.com/robinrodricks/FluentStorage/wiki/Alibaba-OSS-Storage)         | `AlibabaStorage` | `AlibabaStore` |`OssClient`  |
| <img src="https://raw.githubusercontent.com/robinrodricks/FluentStorage/develop/.github/providers/ftp.png" width="32"></img>|  [FTP](https://github.com/robinrodricks/FluentStorage/wiki/FTP-Storage)        						| `FtpStorage` | `FtpStore` |  `AsyncFtpClient` |
| <img src="https://raw.githubusercontent.com/robinrodricks/FluentStorage/develop/.github/providers/sftp.png" width="32"></img>|  [SFTP](https://github.com/robinrodricks/FluentStorage/wiki/SFTP-Storage)        					| `SftpStorage` | `SftpStore` | `SftpClient` |
| <img src="https://raw.githubusercontent.com/robinrodricks/FluentStorage/develop/.github/providers/git.png" width="32"></img>|  [Git](https://github.com/robinrodricks/FluentStorage/wiki/Git-Repository-Storage)        						| `GitStorage` | `GitStore` | `Repository` |
| <img src="https://raw.githubusercontent.com/robinrodricks/FluentStorage/develop/.github/providers/local.png" width="32"></img>|  [Local Disk](https://github.com/robinrodricks/FluentStorage/wiki/Standard-Storage)        		| `StorageFactory` | `DiskStore` | `IFileSystem` |

To add support for a new S3-compatible provider, search for `[ADD STORAGE PROVIDER]` across all code files.



## Polycloud API

This table shows the API supported by `IStore` across various cloud and server providers.

<table>
<thead>

<tr>
<th>API</th>
<th> <img src="https://raw.githubusercontent.com/robinrodricks/FluentStorage/develop/.github/providers/azure.png" width="32"></img> <br><b>Azure<br>Blobs</b></th>
<th> <img src="https://raw.githubusercontent.com/robinrodricks/FluentStorage/develop/.github/providers/azure.png" width="32"></img> <br><b>Azure<br>Files</b></th>
<th> <img src="https://raw.githubusercontent.com/robinrodricks/FluentStorage/develop/.github/providers/aws.png" width="32"></img> <br><b>AWS S3</b></th>
<th> <img src="https://raw.githubusercontent.com/robinrodricks/FluentStorage/develop/.github/providers/gcp.png" width="32"></img> <br><b>GCP</b></th>
<th> <img src="https://raw.githubusercontent.com/robinrodricks/FluentStorage/develop/.github/providers/minio.png" width="32"></img> <br><b>MinIO</b></th>
<th> <img src="https://raw.githubusercontent.com/robinrodricks/FluentStorage/develop/.github/providers/mongo.png" width="32"></img> <br><b>Mongo</b></th>
<th> <img src="https://raw.githubusercontent.com/robinrodricks/FluentStorage/develop/.github/providers/alibaba.png" width="32"></img> <br><b>Alibaba</b></th>
<th> <img src="https://raw.githubusercontent.com/robinrodricks/FluentStorage/develop/.github/providers/ftp.png" width="32"></img> <br><b>FTP</b></th>
<th> <img src="https://raw.githubusercontent.com/robinrodricks/FluentStorage/develop/.github/providers/sftp.png" width="32"></img> <br><b>SFTP</b></th>
<th> <img src="https://raw.githubusercontent.com/robinrodricks/FluentStorage/develop/.github/providers/git.png" width="32"></img> <br><b>Git</b></th>
<th> <img src="https://raw.githubusercontent.com/robinrodricks/FluentStorage/develop/.github/providers/local.png" width="32"></img> <br><b>Disk</b></th>
</tr>

</thead>
<tbody>

<tr>
<td colspan="12"><b>System information</b></td>
</tr>
<tr>
<td>&nbsp; GetClient</td>
<td>✔️</td><td>✔️</td><td>✔️</td><td>✔️</td><td>✔️</td><td>✔️</td><td>✔️</td><td>✔️</td><td>✔️</td><td>✔️</td><td>✔️</td>
</tr>
<tr>
<td>&nbsp; GetServer</td>
<td>❌</td><td>❌</td><td>❌</td><td>❌</td><td>❌</td><td>✔️</td><td>❌</td><td>✔️</td><td>✔️</td><td>✔️</td><td>✔️</td>
</tr>

<tr>
<td colspan="12"><b>File listing</b></td>
</tr>
<tr>
<td>&nbsp; ListDirectory</td>
<td>✔️</td><td>✔️</td><td>✔️</td><td>✔️</td><td>✔️</td><td>✔️</td><td>✔️</td><td>✔️</td><td>✔️</td><td>✔️</td><td>✔️</td>
</tr>
<tr>
<td>&nbsp; ListObjects</td>
<td>✔️</td><td>✔️</td><td>✔️</td><td>✔️</td><td>✔️</td><td>✔️</td><td>✔️</td><td>✔️</td><td>✔️</td><td>✔️</td><td>✔️</td>
</tr>

<tr>
<td colspan="12"><b>File upload &#x2F; download</b></td>
</tr>
<tr>
<td>&nbsp; GetObject</td>
<td>✔️</td><td>✔️</td><td>✔️</td><td>✔️</td><td>✔️</td><td>✔️</td><td>✔️</td><td>✔️</td><td>✔️</td><td>✔️</td><td>✔️</td>
</tr>
<tr>
<td>&nbsp; SetObject</td>
<td>✔️</td><td>✔️</td><td>✔️</td><td>✔️</td><td>✔️</td><td>✔️</td><td>✔️</td><td>✔️</td><td>✔️</td><td>✔️</td><td>✔️</td>
</tr>
<tr>
<td>&nbsp; GetBytes</td>
<td>✔️</td><td>✔️</td><td>✔️</td><td>✔️</td><td>✔️</td><td>✔️</td><td>✔️</td><td>✔️</td><td>✔️</td><td>✔️</td><td>✔️</td>
</tr>
<tr>
<td>&nbsp; SetBytes</td>
<td>✔️</td><td>✔️</td><td>✔️</td><td>✔️</td><td>✔️</td><td>✔️</td><td>✔️</td><td>✔️</td><td>✔️</td><td>✔️</td><td>✔️</td>
</tr>
<tr>
<td>&nbsp; DownloadObject</td>
<td>✔️</td><td>✔️</td><td>✔️</td><td>✔️</td><td>✔️</td><td>✔️</td><td>✔️</td><td>✔️</td><td>✔️</td><td>✔️</td><td>✔️</td>
</tr>
<tr>
<td>&nbsp; UploadObject</td>
<td>✔️</td><td>✔️</td><td>✔️</td><td>✔️</td><td>✔️</td><td>✔️</td><td>✔️</td><td>✔️</td><td>✔️</td><td>✔️</td><td>✔️</td>
</tr>
<tr>
<td>&nbsp; OpenRead</td>
<td>✔️</td><td>✔️</td><td>✔️</td><td>✔️</td><td>✔️</td><td>✔️</td><td>✔️</td><td>✔️</td><td>✔️</td><td>✔️</td><td>✔️</td>
</tr>
<tr>
<td>&nbsp; OpenWrite</td>
<td>✔️</td><td>✔️</td><td>✔️</td><td>✔️</td><td>✔️</td><td>✔️</td><td>✔️</td><td>✔️</td><td>✔️</td><td>✔️</td><td>✔️</td>
</tr>


<tr>
<th>API</th>
<th><b>Azure<br>Blobs</b></th>
<th><b>Azure<br>Files</b></th>
<th><b>AWS S3</b></th>
<th><b>GCP</b></th>
<th><b>MinIO</b></th>
<th><b>Mongo</b></th>
<th><b>Alibaba</b></th>
<th><b>FTP</b></th>
<th><b>SFTP</b></th>
<th><b>Git</b></th>
<th><b>Disk</b></th>
</tr>

<tr>
<td colspan="12"><b>Directory upload &#x2F; download</b></td>
</tr>
<tr>
<td>&nbsp; DownloadDirectory</td>
<td>✔️</td><td>✔️</td><td>✔️</td><td>✔️</td><td>✔️</td><td>✔️</td><td>✔️</td><td>✔️</td><td>✔️</td><td>✔️</td><td>✔️</td>
</tr>
<tr>
<td>&nbsp; UploadDirectory</td>
<td>✔️</td><td>✔️</td><td>✔️</td><td>✔️</td><td>✔️</td><td>✔️</td><td>✔️</td><td>✔️</td><td>✔️</td><td>✔️</td><td>✔️</td>
</tr>

<tr>
<td colspan="12"><b>Directory manipulation</b></td>
</tr>
<tr>
<td>&nbsp; IsFileSystem</td>
<td>✔️</td><td>✔️</td><td>✔️</td><td>✔️</td><td>✔️</td><td>✔️</td><td>✔️</td><td>✔️</td><td>✔️</td><td>✔️</td><td>✔️</td>
</tr>
<tr>
<td>&nbsp; DirectoryExists</td>
<td>✔️</td><td>❌</td><td>✔️</td><td>✔️</td><td>✔️</td><td>✔️</td><td>✔️</td><td>✔️</td><td>✔️</td><td>✔️</td><td>✔️</td>
</tr>
<tr>
<td>&nbsp; CreateDirectory</td>
<td>✔️</td><td>❌</td><td>✔️</td><td>✔️</td><td>✔️</td><td>✔️</td><td>✔️</td><td>✔️</td><td>✔️</td><td>✔️</td><td>✔️</td>
</tr>
<tr>
<td>&nbsp; DeleteDirectory</td>
<td>✔️</td><td>❌</td><td>✔️</td><td>✔️</td><td>✔️</td><td>✔️</td><td>✔️</td><td>✔️</td><td>✔️</td><td>✔️</td><td>✔️</td>
</tr>
<tr>
<td>&nbsp; MoveDirectory</td>
<td>❌</td><td>❌</td><td>❌</td><td>❌</td><td>❌</td><td>❌</td><td>❌</td><td>✔️</td><td>✔️</td><td>✔️</td><td>✔️</td>
</tr>


<tr>
<th>API</th>
<th><b>Azure<br>Blobs</b></th>
<th><b>Azure<br>Files</b></th>
<th><b>AWS S3</b></th>
<th><b>GCP</b></th>
<th><b>MinIO</b></th>
<th><b>Mongo</b></th>
<th><b>Alibaba</b></th>
<th><b>FTP</b></th>
<th><b>SFTP</b></th>
<th><b>Git</b></th>
<th><b>Disk</b></th>
</tr>

<tr>
<td colspan="12"><b>File streaming &#x2F; seeking</b></td>
</tr>
<tr>
<td>&nbsp; IsSeekable</td>
<td>✔️</td><td>✔️</td><td>✔️</td><td>✔️</td><td>✔️</td><td>✔️</td><td>✔️</td><td>✔️</td><td>✔️</td><td>✔️</td><td>✔️</td>
</tr>
<tr>
<td>&nbsp; OpenRange</td>
<td>✔️</td><td>✔️</td><td>✔️</td><td>✔️</td><td>✔️</td><td>✔️</td><td>✔️</td><td>✔️</td><td>✔️</td><td>✔️</td><td>✔️</td>
</tr>
<tr>
<td>&nbsp; OpenSeekable</td>
<td>✔️</td><td>✔️</td><td>✔️</td><td>✔️</td><td>✔️</td><td>✔️</td><td>✔️</td><td>✔️</td><td>✔️</td><td>✔️</td><td>✔️</td>
</tr>

<tr>
<td colspan="12"><b>File manipulation</b></td>
</tr>
<tr>
<td>&nbsp; ObjectExists</td>
<td>✔️</td><td>✔️</td><td>✔️</td><td>✔️</td><td>✔️</td><td>✔️</td><td>✔️</td><td>✔️</td><td>✔️</td><td>✔️</td><td>✔️</td>
</tr>
<tr>
<td>&nbsp; MoveObject</td>
<td>✔️</td><td>❌</td><td>✔️</td><td>✔️</td><td>✔️</td><td>✔️</td><td>✔️</td><td>✔️</td><td>✔️</td><td>✔️</td><td>✔️</td>
</tr>
<tr>
<td>&nbsp; DeleteObject</td>
<td>✔️</td><td>✔️</td><td>✔️</td><td>✔️</td><td>✔️</td><td>✔️</td><td>✔️</td><td>✔️</td><td>✔️</td><td>✔️</td><td>✔️</td>
</tr>
<tr>
<td>&nbsp; DeleteObjects</td>
<td>✔️</td><td>✔️</td><td>✔️</td><td>✔️</td><td>✔️</td><td>✔️</td><td>✔️</td><td>✔️</td><td>✔️</td><td>✔️</td><td>✔️</td>
</tr>

<tr>
<td colspan="12"><b>File metadata</b></td>
</tr>
<tr>
<td>&nbsp; GetObjectChecksum</td>
<td>✔️</td><td>✔️</td><td>✔️</td><td>✔️</td><td>✔️</td><td>✔️</td><td>✔️</td><td>✔️</td><td>✔️</td><td>✔️</td><td>✔️</td>
</tr>
<tr>
<td>&nbsp; GetObjectLength</td>
<td>✔️</td><td>✔️</td><td>✔️</td><td>✔️</td><td>✔️</td><td>✔️</td><td>✔️</td><td>✔️</td><td>✔️</td><td>✔️</td><td>✔️</td>
</tr>
<tr>
<td>&nbsp; GetObjectInfo</td>
<td>✔️</td><td>✔️</td><td>✔️</td><td>✔️</td><td>✔️</td><td>✔️</td><td>✔️</td><td>✔️</td><td>✔️</td><td>✔️</td><td>✔️</td>
</tr>
<tr>
<td>&nbsp; GetObjectsInfo</td>
<td>✔️</td><td>✔️</td><td>✔️</td><td>✔️</td><td>✔️</td><td>✔️</td><td>✔️</td><td>✔️</td><td>✔️</td><td>✔️</td><td>✔️</td>
</tr>
<tr>
<td>&nbsp; SetObjectInfo</td>
<td>✔️</td><td>✔️</td><td>✔️</td><td>✔️</td><td>✔️</td><td>✔️</td><td>✔️</td><td>❌</td><td>❌</td><td>✔️</td><td>✔️</td>
</tr>
<tr>
<td>&nbsp; SetObjectsInfo</td>
<td>✔️</td><td>✔️</td><td>✔️</td><td>✔️</td><td>✔️</td><td>✔️</td><td>✔️</td><td>❌</td><td>❌</td><td>✔️</td><td>✔️</td>
</tr>


<tr>
<th>API</th>
<th><b>Azure<br>Blobs</b></th>
<th><b>Azure<br>Files</b></th>
<th><b>AWS S3</b></th>
<th><b>GCP</b></th>
<th><b>MinIO</b></th>
<th><b>Mongo</b></th>
<th><b>Alibaba</b></th>
<th><b>FTP</b></th>
<th><b>SFTP</b></th>
<th><b>Git</b></th>
<th><b>Disk</b></th>
</tr>

<tr>
<td colspan="12"><b>File versioning</b></td>
</tr>
<tr>
<td>&nbsp; IsVersioned</td>
<td>✔️</td><td>✔️</td><td>✔️</td><td>✔️</td><td>✔️</td><td>✔️</td><td>✔️</td><td>✔️</td><td>✔️</td><td>✔️</td><td>✔️</td>
</tr>
<tr>
<td>&nbsp; ListObjectVersions</td>
<td>✔️</td><td>🚫</td><td>✔️</td><td>✔️</td><td>🚫</td><td>🚫</td><td>🚫</td><td>🚫</td><td>🚫</td><td>✔️</td><td>🚫</td>
</tr>
<tr>
<td>&nbsp; GetObjectVersion</td>
<td>✔️</td><td>🚫</td><td>✔️</td><td>✔️</td><td>🚫</td><td>🚫</td><td>🚫</td><td>🚫</td><td>🚫</td><td>✔️</td><td>🚫</td>
</tr>
<tr>
<td>&nbsp; RestoreObjectVersion</td>
<td>✔️</td><td>🚫</td><td>✔️</td><td>✔️</td><td>🚫</td><td>🚫</td><td>🚫</td><td>🚫</td><td>🚫</td><td>✔️</td><td>🚫</td>
</tr>
<tr>
<td>&nbsp; DeleteObjectVersion</td>
<td>✔️</td><td>🚫</td><td>✔️</td><td>✔️</td><td>🚫</td><td>🚫</td><td>🚫</td><td>🚫</td><td>🚫</td><td>🚫</td><td>🚫</td>
</tr>

<tr>
<td colspan="12"><b>File tagging</b></td>
</tr>
<tr>
<td>&nbsp; IsTagged</td>
<td>✔️</td><td>✔️</td><td>✔️</td><td>✔️</td><td>✔️</td><td>✔️</td><td>✔️</td><td>✔️</td><td>✔️</td><td>✔️</td><td>✔️</td>
</tr>
<tr>
<td>&nbsp; GetObjectTags</td>
<td>✔️</td><td>🚫</td><td>✔️</td><td>✔️</td><td>✔️</td><td>🚫</td><td>✔️</td><td>🚫</td><td>🚫</td><td>🚫</td><td>🚫</td>
</tr>
<tr>
<td>&nbsp; SetObjectTags</td>
<td>✔️</td><td>🚫</td><td>✔️</td><td>✔️</td><td>✔️</td><td>🚫</td><td>✔️</td><td>🚫</td><td>🚫</td><td>🚫</td><td>🚫</td>
</tr>
<tr>
<td>&nbsp; DeleteObjectTags</td>
<td>✔️</td><td>🚫</td><td>✔️</td><td>✔️</td><td>✔️</td><td>🚫</td><td>✔️</td><td>🚫</td><td>🚫</td><td>🚫</td><td>🚫</td>
</tr>

<tr>
<td colspan="12"><b>File storage tier</b></td>
</tr>
<tr>
<td>&nbsp; IsTiered</td>
<td>✔️</td><td>✔️</td><td>✔️</td><td>✔️</td><td>✔️</td><td>✔️</td><td>✔️</td><td>✔️</td><td>✔️</td><td>✔️</td><td>✔️</td>
</tr>
<tr>
<td>&nbsp; GetObjectTier</td>
<td>✔️</td><td>🚫</td><td>✔️</td><td>✔️</td><td>✔️</td><td>🚫</td><td>🚫</td><td>🚫</td><td>🚫</td><td>🚫</td><td>🚫</td>
</tr>
<tr>
<td>&nbsp; SetObjectTier</td>
<td>✔️</td><td>🚫</td><td>✔️</td><td>✔️</td><td>✔️</td><td>🚫</td><td>🚫</td><td>🚫</td><td>🚫</td><td>🚫</td><td>🚫</td>
</tr>


<tr>
<th>API</th>
<th><b>Azure<br>Blobs</b></th>
<th><b>Azure<br>Files</b></th>
<th><b>AWS S3</b></th>
<th><b>GCP</b></th>
<th><b>MinIO</b></th>
<th><b>Mongo</b></th>
<th><b>Alibaba</b></th>
<th><b>FTP</b></th>
<th><b>SFTP</b></th>
<th><b>Git</b></th>
<th><b>Disk</b></th>
</tr>


<tr>
<td colspan="12"><b>Presigned URL generation</b></td>
</tr>
<tr>
<td>&nbsp; GetUploadUrl</td>
<td>✔️</td><td>🚫</td><td>✔️</td><td>✔️</td><td>✔️</td><td>🚫</td><td>✔️</td><td>🚫</td><td>🚫</td><td>🚫</td><td>🚫</td>
</tr>
<tr>
<td>&nbsp; GetDownloadUrl</td>
<td>✔️</td><td>🚫</td><td>✔️</td><td>✔️</td><td>✔️</td><td>🚫</td><td>✔️</td><td>🚫</td><td>🚫</td><td>🚫</td><td>🚫</td>
</tr>
<tr>
<td>&nbsp; GetPresignedUrl</td>
<td>✔️</td><td>🚫</td><td>✔️</td><td>✔️</td><td>✔️</td><td>🚫</td><td>✔️</td><td>🚫</td><td>🚫</td><td>🚫</td><td>🚫</td>
</tr>
<tr>
<td>&nbsp; GetObjectSas</td>
<td>✔️</td><td>🚫</td><td>✔️</td><td>✔️</td><td>✔️</td><td>🚫</td><td>✔️</td><td>🚫</td><td>🚫</td><td>🚫</td><td>🚫</td>
</tr>

<tr>
<td colspan="12"><b>File permissions</b></td>
</tr>
<tr>
<td>&nbsp; GetFilePermissions</td>
<td>❌</td><td>❌</td><td>❌</td><td>❌</td><td>❌</td><td>❌</td><td>❌</td><td>✔️</td><td>✔️</td><td>❌</td><td>❌</td>
</tr>
<tr>
<td>&nbsp; SetFilePermissions</td>
<td>❌</td><td>❌</td><td>❌</td><td>❌</td><td>❌</td><td>❌</td><td>❌</td><td>✔️</td><td>✔️</td><td>❌</td><td>❌</td>
</tr>

</tbody>
</table>


## Provider API

This table shows the API supported by specific cloud providers:


<table>
<tbody>

<tr>
<th> <img src="https://raw.githubusercontent.com/robinrodricks/FluentStorage/develop/.github/providers/azure.png" width="32"></img> <br><b>Azure Blobs</b> </th>
<th> <img src="https://raw.githubusercontent.com/robinrodricks/FluentStorage/develop/.github/providers/azure.png" width="32"></img> <br><b>Azure Data Lake</b> </th>
<th> <img src="https://raw.githubusercontent.com/robinrodricks/FluentStorage/develop/.github/providers/aws.png" width="32"></img> <br><b>AWS S3</b> </th>
</tr>

<tr>
<th> <code>IAzureBlobStore</code> </th>
<th> <code>IAzureDataLakeStore</code> </th>
<th> <code>IS3Storage</code> </th>
</tr>

<tr>
<td>
	<ul>
		<li>AcquireLease</li>
		<li>BreakLease</li>
		<li>GetContainerPublicAccess</li>
		<li>SetContainerPublicAccess</li>
		<li>GetStorageSas</li>
		<li>GetContainerSas</li>
	</ul>
</td>
<td>
	<ul>
		<li>ListFilesystems</li>
		<li>CreateFilesystem</li>
		<li>DeleteFilesystem</li>
		<li>GetAccessControl</li>
		<li>SetAccessControl</li>
	</ul>
</td>
<td>
	<ul>
		<li>SetAcl</li>
	</ul>
</td>
</tr>

</tbody>
</table>


## Queue Providers

FluentStorage supports the following queue/messaging providers:

|       		| Documentation Link                                               | Factory class |
|---------------| --------------------------------------------------------------------------- | ---------------- |
| <img src="https://raw.githubusercontent.com/robinrodricks/FluentStorage/develop/.github/providers/aws.png" width="32"></img>| [AWS SQS](https://github.com/robinrodricks/FluentStorage/wiki/AWS-SQS)   						| `AwsSqsStorage` |
| <img src="https://raw.githubusercontent.com/robinrodricks/FluentStorage/develop/.github/providers/azure.png" width="32"></img>|  [Azure Queue](https://github.com/robinrodricks/FluentStorage/wiki/Azure-Queue-Storage) 		| `AzureQueueStorage` |
| <img src="https://raw.githubusercontent.com/robinrodricks/FluentStorage/develop/.github/providers/azure.png" width="32"></img>|  [Azure Service Bus](https://github.com/robinrodricks/FluentStorage/wiki/Azure-Service-Bus) 	| `AzureServiceBus` |




## Packages

Stable binaries are released on NuGet, and contain everything you need to use Cloud Storage in your .NET app.


|       		| Package      		| Latest Version	|  Downloads	|  Documentation	|
|---------------|---------------		|-----------	|-----------		|-----------		|
| <img src="https://raw.githubusercontent.com/robinrodricks/FluentStorage/develop/.github/providers/local.png" width="32"></img>| **[FluentStorage](https://www.nuget.org/packages/FluentStorage)**      	|     [![Version](https://img.shields.io/nuget/vpre/FluentStorage.svg)](https://www.nuget.org/packages/FluentStorage) 		|  [![Downloads](https://img.shields.io/nuget/dt/FluentStorage.svg)](https://www.nuget.org/packages/FluentStorage) | [Standard](https://github.com/robinrodricks/FluentStorage/wiki/Standard-Storage) |
| <img src="https://raw.githubusercontent.com/robinrodricks/FluentStorage/develop/.github/providers/aws.png" width="32"></img>| **[FluentStorage.AWS](https://www.nuget.org/packages/FluentStorage.AWS)**      	|     [![Version](https://img.shields.io/nuget/vpre/FluentStorage.AWS.svg)](https://www.nuget.org/packages/FluentStorage.AWS) 		|  [![Downloads](https://img.shields.io/nuget/dt/FluentStorage.AWS.svg)](https://www.nuget.org/packages/FluentStorage.AWS) | [S3](https://github.com/robinrodricks/FluentStorage/wiki/AWS-S3-Storage), [SQS](https://github.com/robinrodricks/FluentStorage/wiki/AWS-SQS) |
| <img src="https://raw.githubusercontent.com/robinrodricks/FluentStorage/develop/.github/providers/gcp.png" width="32"></img>| **[FluentStorage.GCP](https://www.nuget.org/packages/FluentStorage.GCP)**      	|     [![Version](https://img.shields.io/nuget/vpre/FluentStorage.GCP.svg)](https://www.nuget.org/packages/FluentStorage.GCP) 		|  [![Downloads](https://img.shields.io/nuget/dt/FluentStorage.GCP.svg)](https://www.nuget.org/packages/FluentStorage.GCP) | [GCP](https://github.com/robinrodricks/FluentStorage/wiki/Google-Cloud-Storage) |
| <img src="https://raw.githubusercontent.com/robinrodricks/FluentStorage/develop/.github/providers/azure.png" width="32"></img>| **[FluentStorage.Azure](https://www.nuget.org/packages/FluentStorage.Azure)**      	|     [![Version](https://img.shields.io/nuget/vpre/FluentStorage.Azure.svg)](https://www.nuget.org/packages/FluentStorage.Azure) 		|  [![Downloads](https://img.shields.io/nuget/dt/FluentStorage.Azure.svg)](https://www.nuget.org/packages/FluentStorage.Azure) | --- |
| <img src="https://raw.githubusercontent.com/robinrodricks/FluentStorage/develop/.github/providers/azure-blob-block.png" width="32"></img>| **[FluentStorage.Azure.Blobs](https://www.nuget.org/packages/FluentStorage.Azure.Blobs)**      	|     [![Version](https://img.shields.io/nuget/vpre/FluentStorage.Azure.Blobs.svg)](https://www.nuget.org/packages/FluentStorage.Azure.Blobs) 		|  [![Downloads](https://img.shields.io/nuget/dt/FluentStorage.Azure.Blobs.svg)](https://www.nuget.org/packages/FluentStorage.Azure.Blobs) | [Blob](https://github.com/robinrodricks/FluentStorage/wiki/Azure-Blob-Storage) |
| <img src="https://raw.githubusercontent.com/robinrodricks/FluentStorage/develop/.github/providers/azure-blob-file.png" width="32"></img>| **[FluentStorage.Azure.Files](https://www.nuget.org/packages/FluentStorage.Azure.Files)**      	|     [![Version](https://img.shields.io/nuget/vpre/FluentStorage.Azure.Files.svg)](https://www.nuget.org/packages/FluentStorage.Azure.Files) 		|  [![Downloads](https://img.shields.io/nuget/dt/FluentStorage.Azure.Files.svg)](https://www.nuget.org/packages/FluentStorage.Azure.Files) | [File](https://github.com/robinrodricks/FluentStorage/wiki/Azure-Files-Storage) |
| <img src="https://raw.githubusercontent.com/robinrodricks/FluentStorage/develop/.github/providers/azure-service-bus.png" width="32"></img>| **[FluentStorage.Azure.ServiceBus](https://www.nuget.org/packages/FluentStorage.Azure.ServiceBus)**      	|     [![Version](https://img.shields.io/nuget/vpre/FluentStorage.Azure.ServiceBus.svg)](https://www.nuget.org/packages/FluentStorage.Azure.ServiceBus) 		|  [![Downloads](https://img.shields.io/nuget/dt/FluentStorage.Azure.ServiceBus.svg)](https://www.nuget.org/packages/FluentStorage.Azure.ServiceBus) | [ServiceBus](https://github.com/robinrodricks/FluentStorage/wiki/Azure-Service-Bus) |
| <img src="https://raw.githubusercontent.com/robinrodricks/FluentStorage/develop/.github/providers/azure-key-vault.png" width="32"></img>| **[FluentStorage.Azure.KeyVault](https://www.nuget.org/packages/FluentStorage.Azure.KeyVault)**      	|     [![Version](https://img.shields.io/nuget/vpre/FluentStorage.Azure.KeyVault.svg)](https://www.nuget.org/packages/FluentStorage.Azure.KeyVault) 		|  [![Downloads](https://img.shields.io/nuget/dt/FluentStorage.Azure.KeyVault.svg)](https://www.nuget.org/packages/FluentStorage.Azure.KeyVault) | [KeyVault](https://github.com/robinrodricks/FluentStorage/wiki/Azure-Key-Vault) |
| <img src="https://raw.githubusercontent.com/robinrodricks/FluentStorage/develop/.github/providers/azure-queue-storage.png" width="32"></img>| **[FluentStorage.Azure.Queues](https://www.nuget.org/packages/FluentStorage.Azure.Queues)**      	|     [![Version](https://img.shields.io/nuget/vpre/FluentStorage.Azure.Queues.svg)](https://www.nuget.org/packages/FluentStorage.Azure.Queues) 		|  [![Downloads](https://img.shields.io/nuget/dt/FluentStorage.Azure.Queues.svg)](https://www.nuget.org/packages/FluentStorage.Azure.Queues) | [Queue](https://github.com/robinrodricks/FluentStorage/wiki/Azure-Queue-Storage) |
| <img src="https://raw.githubusercontent.com/robinrodricks/FluentStorage/develop/.github/providers/minio.png" width="32"></img>| **[FluentStorage.Minio](https://www.nuget.org/packages/FluentStorage.Minio)**      	|     [![Version](https://img.shields.io/nuget/vpre/FluentStorage.Minio.svg)](https://www.nuget.org/packages/FluentStorage.Minio) 		|  [![Downloads](https://img.shields.io/nuget/dt/FluentStorage.Minio.svg)](https://www.nuget.org/packages/FluentStorage.Minio) | [Minio](https://github.com/robinrodricks/FluentStorage/wiki/MinIO-Storage) |
| <img src="https://raw.githubusercontent.com/robinrodricks/FluentStorage/develop/.github/providers/mongo.png" width="32"></img>| **[FluentStorage.Mongo](https://www.nuget.org/packages/FluentStorage.Mongo)**      	|     [![Version](https://img.shields.io/nuget/vpre/FluentStorage.Mongo.svg)](https://www.nuget.org/packages/FluentStorage.Mongo) 		|  [![Downloads](https://img.shields.io/nuget/dt/FluentStorage.Mongo.svg)](https://www.nuget.org/packages/FluentStorage.Mongo) | [Mongo](https://github.com/robinrodricks/FluentStorage/wiki/MongoDB-GridFS-Storage) |
| <img src="https://raw.githubusercontent.com/robinrodricks/FluentStorage/develop/.github/providers/alibaba.png" width="32"></img>| **[FluentStorage.Alibaba](https://www.nuget.org/packages/FluentStorage.Alibaba)**      	|     [![Version](https://img.shields.io/nuget/vpre/FluentStorage.Alibaba.svg)](https://www.nuget.org/packages/FluentStorage.Alibaba) 		|  [![Downloads](https://img.shields.io/nuget/dt/FluentStorage.Alibaba.svg)](https://www.nuget.org/packages/FluentStorage.Alibaba) | [Alibaba](https://github.com/robinrodricks/FluentStorage/wiki/Alibaba-OSS-Storage) |
| <img src="https://raw.githubusercontent.com/robinrodricks/FluentStorage/develop/.github/providers/ftp.png" width="32"></img>| **[FluentStorage.FTP](https://www.nuget.org/packages/FluentStorage.FTP)**      	|     [![Version](https://img.shields.io/nuget/vpre/FluentStorage.FTP.svg)](https://www.nuget.org/packages/FluentStorage.FTP) 		|  [![Downloads](https://img.shields.io/nuget/dt/FluentStorage.FTP.svg)](https://www.nuget.org/packages/FluentStorage.FTP) | [FTP](https://github.com/robinrodricks/FluentStorage/wiki/FTP-Storage) |
| <img src="https://raw.githubusercontent.com/robinrodricks/FluentStorage/develop/.github/providers/sftp.png" width="32"></img>| **[FluentStorage.SFTP](https://www.nuget.org/packages/FluentStorage.SFTP)**      	|     [![Version](https://img.shields.io/nuget/vpre/FluentStorage.SFTP.svg)](https://www.nuget.org/packages/FluentStorage.SFTP) 		|  [![Downloads](https://img.shields.io/nuget/dt/FluentStorage.SFTP.svg)](https://www.nuget.org/packages/FluentStorage.SFTP) | [SFTP](https://github.com/robinrodricks/FluentStorage/wiki/SFTP-Storage) |
| <img src="https://raw.githubusercontent.com/robinrodricks/FluentStorage/develop/.github/providers/git.png" width="32"></img>| **[FluentStorage.Git](https://www.nuget.org/packages/FluentStorage.Git)**      	|     [![Version](https://img.shields.io/nuget/vpre/FluentStorage.Git.svg)](https://www.nuget.org/packages/FluentStorage.Git) 		|  [![Downloads](https://img.shields.io/nuget/dt/FluentStorage.Git.svg)](https://www.nuget.org/packages/FluentStorage.Git) | [Git](https://github.com/robinrodricks/FluentStorage/wiki/Git-Repository-Storage) |




## Concept Mapping

This table shows the API and the provider-specific concept it maps to:

| Concept              | AWS S3                    | Azure Blob                       | GCP                |
| -------------------- | -------------------------------------- | -------------------------------- | ------------------ |
| **Versioning API**   | Object Versions                        | Blob Versions                    | Object Generations |
| **Tagging API**      | Object Tags                            | Blob Index Tags <br> / Blob Tags  | Custom Metadata    |
| **Metadata API**     | Object Metadata                        | Blob Metadata                    | Object Metadata    |
| **Storage Tier**     | Storage Class                           | Access Tier                     | Storage Class      |
| **Retention API** (planned)    | Object Lock Retention                  | Immutability Policy              | Retention Policy   |
| **Locking API** (planned)      | Object Lock Configuration <br> / Legal Hold | Legal Hold <br> + Immutability Policy | Object Holds       |

## Storage Tier Mapping

This table shows the FluentStorage `StorageTier` enum and the provider-specific tier it maps to:


| Concept                   | AWS S3              | Azure Blob        |    GCP           |
| ------------------------- | ---------------------- | -------------- | ------------------ |
| `StorageTier.Standard`    | `STANDARD`            | `Hot`          | `STANDARD`         |
| `StorageTier.Intelligent` | `INTELLIGENT_TIERING` | `AutoTiering`  | `AUTOCLASS`        |
| `StorageTier.Nearline`    | `STANDARD_IA`         | `Cool`         | `NEARLINE`         |
| `StorageTier.Cold`        | `GLACIER_IR`          | `Cold`         | `COLDLINE`         |
| `StorageTier.Archive`     | `GLACIER`             | `Archive`      | `ARCHIVE`          |
| `StorageTier.DeepArchive` | `DEEP_ARCHIVE`        | `Archive`      | `ARCHIVE`          |




## Platform Support

FluentStorage works on .NET and .NET Standard/.NET Core.

| Platform      		| Binaries Folder	|
|---------------		|-----------		|
| **.NET 7.0**      	| net70     		|
| **.NET 8.0**      	| net80     		|
| **.NET 9.0**      	| net90     		|
| **.NET Standard 2.0** | netstandard2.0	|
| **.NET Standard 2.1** | netstandard2.1	|

FluentStorage is also supported on these platforms: (via .NET Standard)

  - **Mono** 4.6
  - **Xamarin.iOS** 10.0
  - **Xamarin.Android** 10.0
  - **Universal Windows Platform** 10.0

Binaries for all platforms are built from a single Visual Studio Project. You will need the latest [Visual Studio](https://visualstudio.microsoft.com/downloads/) to build or contribute to FluentStorage.




## Architecture

### Without FluentStorage

Today, most cloud applications and services are developed against vendor-specific APIs like Amazon S3 API or Azure Blob API for cloud storage capabilities.

Using multiple vendor-specific APIs can increase your vendor lock-in, and makes your application more complex and harder to maintain. And sometimes these APIs may not offer all the functionality you need for your application. Polycloud also becomes very hard to implement.

![Arch](https://raw.githubusercontent.com/robinrodricks/FluentStorage/develop/.github/arch-without.png)

### With FluentStorage

What if we had a single, consistent API to deal with all types of cloud storage? That would solve these issues and bring more flexibility in switching cloud providers or cloud services.

Thus was born the idea for FluentStorage.

You can use a single, consistent API to interact with multiple cloud providers, where each provider is supported through its own special Nuget package.

![Arch](https://raw.githubusercontent.com/robinrodricks/FluentStorage/develop/.github/arch-with.png)



## Documentation

Check the [Wiki](https://github.com/robinrodricks/FluentStorage/wiki).



## What's New

In 2026, we added:

* **Version 8** with a massive redesign of the [entire API and behaviour](https://github.com/robinrodricks/FluentStorage/wiki/Migration-Guide)
* **MongoDB GridFS** provider using native MongoDB Driver
* **MinIO** provider using native Minio SDK
* **Alibaba OSS** provider using native Aliyun SDK
* **Cloudflare R2** provider using S3-compatible SDK
* **Backblaze B2** provider using S3-compatible SDK
* **Hetzner** provider using S3-compatible SDK
* **Vultr** provider using S3-compatible SDK
* **Git** provider using native LibGit2Sharp
* **Azure.Identity support for Azure Files** with token credential, client secret, and managed identity authentication.
* **Wiki** pages per provider
* **Removed** unused packages: Databricks, EventHub, DataLake Gen 1, ServiceFabric

In 2025, we added:

* **Azure.Identity support for Azure Blobs** with token credential, client secret, and managed identity authentication.

In 2024, we added:

* **DigitalOcean Spaces** provider
* **MinIO** provider
* **Wasabi** provider

In 2023, we added:

* **SFTP** provider [SSH.NET](https://github.com/sshnet/SSH.NET) added
* **FTP** provider [FluentFTP](https://github.com/robinrodricks/FluentFTP) updated to v44
* **AWS** Nuget bumped to latest versions
* **Wiki** created for documentation
* **Platform** support updated to `netstandard2.0`,`netstandard2.1`,`net50`,`net60`



## Similar Libraries

- [Foundatio](https://github.com/FoundatioFx/Foundatio) - Caching, Messaging, Queues and Storage library with AWS, Azure and many other providers
- [SlimMessageBus](https://github.com/zarusz/SlimMessageBus) - Messaging library with providers like RabbitMQ, Kafka, Azure EventHub, MQTT, Redis
- [ManagedCode.Storage](https://github.com/managedcode/Storage) - Storage library with AWS, Azure, GCP and other providers



## Sponsorship

FluentStorage has received major sponsorship from these generous organizations:

<table>
<tr>
	<td width="200px">
		<a href="https://www.microsoft.com/">
		<img src="https://raw.githubusercontent.com/robinrodricks/FluentStorage/refs/heads/develop/.github/microsoft-logo.png" /> Microsoft Corporation
		</a>
	</td>
</tr>
</table>

Has FluentStorage made a difference for you or your organization? If so, consider [becoming a sponsor](https://github.com/sponsors/robinrodricks) to help keep the project thriving. Even a small monthly contribution, like $20, can make a meaningful impact.


## Contributors

Special thanks to these awesome people who helped create FluentStorage!


<a href="https://github.com/robinrodricks/FluentStorage/graphs/contributors">
	<!---
	<img src="https://contributors-img.web.app/image?repo=robinrodricks/FluentStorage" />
	-->
	<img src="https://github.com/robinrodricks/FluentStorage/raw/develop/.github/contributors.png" />
</a>

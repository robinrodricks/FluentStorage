using System;
using System.Collections.Generic;
using System.Linq;
using Azure;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Azure.Storage.Sas;
using FluentStorage.Azure.Blobs.DataLake.Model;
using FluentStorage.Enums;
using FluentStorage.Model;
using FluentStorage.Utils.Extensions;
using MimeMapping;

namespace FluentStorage.Azure.Blobs.Utils;

static class AzConvert {
	private static readonly char[] MetadataPairsSeparator = new[] { ',' };
	private static readonly char[] MetadataPairSeparator = new[] { '=' };

	public static StoreObject ToBlob(Filesystem fs) {
		var blob = new StoreObject(fs.Name, StorageObjectType.Folder);
		blob.DateModified = fs.LastModified;
		blob.TryAddProperties(
			"ETag", fs.Etag,
			"IsFilesystem", true);
		return blob;
	}

	public static StoreObject ToBlob(BlobContainerItem item) {
		var blob = new StoreObject(item.Name, StorageObjectType.Folder);
		blob.TryAddProperties(
			"IsContainer", true);

		return blob;
	}

	public static StoreObject ToBlob(BlobContainerClient client) {
		var blob = new StoreObject(client.Name, StorageObjectType.Folder);
		blob.Properties["IsContainer"] = true;
		if (client.Name == "$logs") {
			blob.Properties["IsLogsContainer"] = true;
		}
		return blob;
	}

	public static StoreObject ToBlob(string name, Response<BlobContainerProperties> properties) {
		return ToBlob(name, properties.Value);
	}

	public static StoreObject ToBlob(string name, BlobContainerProperties properties) {
		var blob = new StoreObject(name, StorageObjectType.Folder);
		blob.DateModified = properties.LastModified;

		blob.TryAddProperties(
			"IsContainer", true,
			"ETag", properties.ETag,
			"HasImmutabilityPolicy", properties.HasImmutabilityPolicy,
			"HasLegalHold", properties.HasLegalHold,
			"LastModified", properties.LastModified,
			"LeaseDuration", properties.LeaseDuration,
			"LeaseState", properties.LeaseState,
			"LeaseStatus", properties.LeaseStatus,
			"PublicAccess", properties.PublicAccess);

		blob.Metadata.MergeRange(properties.Metadata);

		return blob;
	}

	public static StoreObject ToBlob(string containerName, BlobHierarchyItem bhi) {
		string GetFullName(string name) => containerName == null
			? name
			: StoragePath.Combine(containerName, name);

		if (bhi.IsBlob) {
			var blob = new StoreObject(GetFullName(bhi.Blob.Name), StorageObjectType.File);
			blob.MD5 = bhi.Blob.Properties.ContentHash.ToHexString();
			blob.Size = bhi.Blob.Properties.ContentLength;
			blob.DateModified = bhi.Blob.Properties.LastModified;

			AddProperties(blob, bhi.Blob.Properties);
			blob.Metadata.MergeRange(bhi.Blob.Metadata);

			return blob;
		}

		if (bhi.IsPrefix) {
			var blob = new StoreObject(GetFullName(bhi.Prefix), StorageObjectType.Folder);
			//nothing else we know about prefix
			return blob;
		}

		throw new NotImplementedException();
	}

	public static StoreObject ToBlob(string containerName, string path, Response<BlobProperties> properties) {
		return ToBlob(containerName, path, properties.Value);
	}

	public static StoreObject ToBlob(string containerName, string path, BlobProperties properties) {
		string GetFullName(string name) => containerName == null
			? name
			: StoragePath.Combine(containerName, name);

		var blob = new StoreObject(GetFullName(path), StorageObjectType.File);
		blob.MD5 = properties.ContentHash.ToHexString();
		blob.Size = properties.ContentLength;
		blob.DateModified = properties.LastModified;

		AddProperties(blob, properties);

		blob.Metadata.MergeRange(properties.Metadata);

		return blob;
	}

	private static void AddProperties(StoreObject blob, BlobItemProperties properties) {
		blob.TryAddProperties(
			"CustomerProvidedKeySha256", properties.CustomerProvidedKeySha256,
			"IncrementalCopy", properties.IncrementalCopy,
			"ServerEncrypted", properties.ServerEncrypted,
			"DeletedOn", properties.DeletedOn,
			"RemainingRetentionDays", properties.RemainingRetentionDays,
			"AccessTier", properties.AccessTier,
			"AccessTierChangedOn", properties.AccessTierChangedOn,
			"AccessTierInferred", properties.AccessTierInferred,
			"ArchiveStatus", properties.ArchiveStatus,
			"BlobSequenceNumber", properties.BlobSequenceNumber,
			"BlobType", properties.BlobType,
			"CacheControl", properties.CacheControl,
			"ContentDisposition", properties.ContentDisposition,
			"ContentEncoding", properties.ContentEncoding,
			"ContentHash", properties.ContentHash.ToHexString(),
			"ContentLanguage", properties.ContentLanguage,
			"ContentLength", properties.ContentLength,
			"ContentType", properties.ContentType,
			"CopyCompletedOn", properties.CopyCompletedOn,
			"CopyId", properties.CopyId,
			"CopyProgress", properties.CopyProgress,
			"CopySource", properties.CopySource,
			"CopyStatus", properties.CopyStatus,
			"CopyStatusDescription", properties.CopyStatusDescription,
			"CreatedOn", properties.CreatedOn,
			"DestinationSnapshot", properties.DestinationSnapshot,
			"ETag", properties.ETag,
			"LastModified", properties.LastModified,
			"LeaseDuration", properties.LeaseDuration,
			"LeaseState", properties.LeaseState,
			"LeaseStatus", properties.LeaseStatus);
	}

	private static void AddProperties(StoreObject blob, BlobProperties properties) {
		blob.TryAddProperties(
			"AcceptRanges", properties.AcceptRanges,
			"AccessTier", properties.AccessTier,
			"AccessTierChangedOn", properties.AccessTierChangedOn,
			"AccessTierInferred", properties.AccessTierInferred,
			"ArchiveStatus", properties.ArchiveStatus,
			"BlobCommittedBlockCount", properties.BlobCommittedBlockCount,
			"BlobSequenceNumber", properties.BlobSequenceNumber,
			"BlobType", properties.BlobType,
			"CacheControl", properties.CacheControl,
			"ContentDisposition", properties.ContentDisposition,
			"ContentEncoding", properties.ContentEncoding,
			"ContentHash", properties.ContentHash,
			"ContentLanguage", properties.ContentLanguage,
			"ContentLength", properties.ContentLength,
			"ContentType", properties.ContentType,
			"CopyCompletedOn", properties.CopyCompletedOn,
			"CopyId", properties.CopyId,
			"CopyProgress", properties.CopyProgress,
			"CopySource", properties.CopySource,
			"CopyStatus", properties.CopyStatus,
			"CopyStatusDescription", properties.CopyStatusDescription,
			"CreatedOn", properties.CreatedOn,
			"DestinationSnapshot", properties.DestinationSnapshot,
			"EncryptionKeySha256", properties.EncryptionKeySha256,
			"ETag", properties.ETag,
			"IsIncrementalCopy", properties.IsIncrementalCopy,
			"IsServerEncrypted", properties.IsServerEncrypted,
			"LastModified", properties.LastModified,
			"LeaseDuration", properties.LeaseDuration,
			"LeaseState", properties.LeaseState,
			"LeaseStatus", properties.LeaseStatus);
	}

	public static StoreObject ToBlob(string filesystemName, Gen2Path path) {
		var blob = new StoreObject(StoragePath.Combine(filesystemName, path.Name), path.IsDirectory ? StorageObjectType.Folder : StorageObjectType.File) {
			Size = path.ContentLength,
			DateModified = path.LastModified
		};

		blob.TryAddProperties(
			"ETag", path.ETag,
			"Owner", path.Owner,
			"Group", path.Group,
			"Permissions", path.Permissions);

		return blob;
	}

	public static StoreObject ToBlob(string fullPath, IDictionary<string, string> pathHeaders, bool isFilesystem) {
		var blob = new StoreObject(fullPath);

		if (pathHeaders.TryGetValue("Content-MD5", out string md5)) {
			blob.MD5 = md5.Base64DecodeAsBytes().ToHexString();
		}

		if (pathHeaders.TryGetValue("Content-Length", out string contentLength)) {
			blob.Size = long.Parse(contentLength);
		}

		blob.DateModified = DateTimeOffset.Parse(pathHeaders["Last-Modified"]);

		blob.TryAddPropertiesFromDictionary(pathHeaders,
			"Content-Type",
			"ETag",
			"x-ms-owner",
			"x-ms-group",
			"x-ms-permissions",
			"x-ms-resource-type",
			"x-ms-lease-state",
			"x-ms-lease-status",
			"x-ms-server-encrypted",
			"x-ms-request-id",
			"x-ms-client-request-id");

		if (isFilesystem) {
			blob.Properties["IsFilesystem"] = isFilesystem;
		}

		if (pathHeaders.TryGetValue("x-ms-properties", out string um)) {
			Dictionary<string, string> umd = um
				.Split(MetadataPairsSeparator, StringSplitOptions.RemoveEmptyEntries)
				.Select(pair => pair.Split(MetadataPairSeparator, 2))
				.ToDictionary(a => a[0], a => a[1].Base64Decode());

			blob.Metadata.MergeRange(umd);
		}

		return blob;
	}

	public static BlobSasBuilder OptionsToSas(StorageUrlOptions options,string containerName,string objectPath) {

		if (options == null)
			throw new ArgumentNullException(nameof(options));

		if (string.IsNullOrWhiteSpace(containerName))
			throw new ArgumentNullException(nameof(containerName));

		if (string.IsNullOrWhiteSpace(objectPath))
			throw new ArgumentNullException(nameof(objectPath));

		DateTimeOffset startsOn = options.StartsOn ?? DateTimeOffset.UtcNow;

		BlobSasBuilder sas = new() {
			BlobContainerName = containerName,
			BlobName = objectPath,
			Resource = "b",
			StartsOn = startsOn,
			ExpiresOn = startsOn + options.ExpiresIn,
			Protocol = options.Protocol == StorageUrlProtocol.HttpAndHttps
				? SasProtocol.HttpsAndHttp
				: SasProtocol.Https
		};

		sas.SetPermissions((BlobSasPermissions)options.Permissions);

		if (!string.IsNullOrWhiteSpace(options.ContentType))
			sas.ContentType = options.ContentType;

		else if (options.Permissions.HasFlag(StorageUrlPermissions.Read))
			sas.ContentType = MimeUtility.GetMimeMapping(objectPath);

		if (!string.IsNullOrWhiteSpace(options.ContentDisposition))
			sas.ContentDisposition = options.ContentDisposition;

		if (!string.IsNullOrWhiteSpace(options.CacheControl))
			sas.CacheControl = options.CacheControl;

		if (!string.IsNullOrWhiteSpace(options.ContentEncoding))
			sas.ContentEncoding = options.ContentEncoding;

		if (!string.IsNullOrWhiteSpace(options.ContentLanguage))
			sas.ContentLanguage = options.ContentLanguage;

		if (!string.IsNullOrWhiteSpace(options.StoredAccessPolicy))
			sas.Identifier = options.StoredAccessPolicy;

		if (!string.IsNullOrWhiteSpace(options.EncryptionScope))
			sas.EncryptionScope = options.EncryptionScope;

		if (!string.IsNullOrWhiteSpace(options.CorrelationId))
			sas.CorrelationId = options.CorrelationId;

		if (!string.IsNullOrWhiteSpace(options.SignedVersion))
			sas.Version = options.SignedVersion;

		if (!string.IsNullOrWhiteSpace(options.IpRange))
			sas.IPRange = SasIPRange.Parse(options.IpRange);

		return sas;
	}

}
using System;
using Azure.Storage.Files.Shares.Models;
using FluentStorage.Enums;
using FluentStorage.Model;
using FluentStorage.Utils.Extensions;

namespace FluentStorage.Azure.Files.Utils;

static class AzConvert {
	public static StoreObject ToBlob(ShareItem share) {
		var blob = new StoreObject(share.Name, StorageObjectType.Folder);
		blob.TryAddProperties(
			"ETag", share.Properties.ETag?.ToString(),
			"LastModified", share.Properties.LastModified?.ToString(),
			"QuotaInGB", share.Properties.QuotaInGB?.ToString(),
			"IsShare", "True");
		return blob;
	}

	public static StoreObject ToBlob(string path, ShareFileItem item) {
		if (item.IsDirectory) {
			var blob = new StoreObject(path, item.Name, StorageObjectType.Folder);
			blob.TryAddProperties(
				"ETag", item.Properties.ETag?.ToString(),
				"LastModified", item.Properties.LastModified?.ToString());
			return blob;
		}

		return ToFileBlob(path, item);
	}

	public static StoreObject ToBlob(string path, string name, ShareFileProperties properties) {
		return ToBlob(path, name, properties, properties.Metadata);
	}

	private static StoreObject ToFileBlob(string path, ShareFileItem item) {
		ShareFileItemProperties properties = item.Properties;
		var blob = new StoreObject(path, item.Name, StorageObjectType.File) {
			DateModified = properties.LastModified,
			Size = item.FileSize
		};
		blob.TryAddProperties(
			"ETag", properties.ETag?.ToString(),
			"LastModified", properties.LastModified?.ToString());
		return blob;
	}

	private static StoreObject ToBlob(string path, string name, ShareFileProperties properties, System.Collections.Generic.IDictionary<string, string> metadata) {
		var blob = new StoreObject(path, name, StorageObjectType.File) {
			DateModified = properties.LastModified,
			Size = properties.ContentLength,
			MD5 = properties.ContentHash == null ? null : Convert.ToBase64String(properties.ContentHash)
		};
		blob.TryAddProperties(
			"CopyStatus", properties.CopyStatus.ToString(),
			"ContentType", properties.ContentType,
			"ETag", properties.ETag.ToString(),
			"IsServerEncrypted", properties.IsServerEncrypted.ToString(),
			"LastModified", properties.LastModified.ToString());
		blob.Metadata.MergeRange(metadata);
		return blob;
	}
}
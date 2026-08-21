using System;
using System.Collections.Generic;
using System.Linq;
using FluentStorage.Enums;
using FluentStorage.Model;
using FluentStorage.Utils.Extensions;
using FluentStorage.Utils.Validation;

namespace FluentStorage.Storage;

/// <summary>
/// Options for listing storage content
/// </summary>
public class StorageListOptions {

	public const int MAX_THREADS = 10;
	public const int PAGE_SIZE = 1000;
		
	private string _prefix;
	private string _folderPath = "";

	/// <summary>
	/// Folder path to start browsing from. When not set scanning starts from the root folder.
	/// </summary>
	public string FolderPath {
		get => _folderPath;
		set {
			_folderPath = StoragePath.Normalize(value);
		}
	}

	/// <summary>
	/// Gets or sets a browsing filter used by some implementations which can filter out results before returning it to you.
	/// This is useful to minimise amount of RAM taken when returning the results and then filtering them on client side.
	/// Note that filtering will be happening on the client side, therefore this is the least efficient filter and should
	/// only be used when you're concerned about RAM usage.
	/// </summary>
	public Func<StoreObject, bool> BrowseFilter { get; set; }

	/// <summary>
	/// Prefix to filter file name by. Folders are not affected by this filter. If you list files recursively
	/// the prefix is applied in every folder.
	/// </summary>
	public string FilePrefix {
		get => _prefix;
		set {
			ArgValidator.AssertPrefix(value);
			_prefix = value;
		}
	}

	/// <summary>
	/// When true, operation will recursively navigate down the folders.
	/// </summary>
	public bool Recurse { get; set; }

	/// <summary>
	/// Recursion mode to use if recursion is enabled.  Remote recursion is the default for services which support it.
	///
	///  * AWS/MinIO     : Allows remote or local recursion
	///  * Azure/GCP/FTP : recursion always occurs remotely regardless of this setting
	///  * SFTP/Disk/ZIP : recursion always occurs locally regardless of this setting
	/// </summary>
	public StorageRecursion RecursionMode { get; set; } = StorageRecursion.Remote;

	/// <summary>
	/// Specify the number of parallel tasks to use when querying (default 10)
	/// This option is only relevant for S3/MinIO and Azure
	/// </summary>
	public int? NumberOfRecursionThreads { get; set; }

	/// <summary>
	/// When recursing, specify the number of items returned per page from the remote service (default 1000)
	/// This option is only relevant for S3/MinIO and GCP
	/// </summary>
	public int? PageSize { get; set; }
		
	/// <summary>
	/// When set, limits the maximum amount of results. The count affects all object counts, including files and folders.
	/// </summary>
	public int? MaxResults { get; set; }

	/// <summary>
	/// When set, includes blob attributes in the response if the provider supports it. False by default
	/// only because metadata consumes more memory in response object, although most of the providers do not
	/// have any overhead in creating this metadata.
	/// </summary>
	public bool IncludeAttributes { get; set; } = false;

	/// <summary>
	/// Helper method that returns true if a <see cref="StoreObject"/> matches these list options.
	/// </summary>
	public bool IsMatch(StoreObject blob) {
		return _prefix == null || blob.Type != StorageObjectType.File || blob.Name.StartsWith(_prefix);
	}

	/// <summary>
	/// Only for internal use
	/// </summary>
	public bool Add(ICollection<StoreObject> dest, ICollection<StoreObject> src) {
		if (MaxResults == null || (dest.Count + src.Count < MaxResults.Value)) {
			dest.AddRange(src);
			return false;
		}

		dest.AddRange(src.Take(MaxResults.Value - dest.Count));
		return true;
	}

	/// <summary>
	/// Clones list options
	/// </summary>
	/// <returns></returns>
	public StorageListOptions Clone() {
		return (StorageListOptions)(MemberwiseClone());
	}
}
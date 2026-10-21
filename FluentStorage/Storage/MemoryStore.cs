using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentStorage.Enums;
using FluentStorage.Model;
using FluentStorage.Streaming;
using FluentStorage.Utils.Extensions;
using FluentStorage.Utils.Validation;

namespace FluentStorage.Storage;

class MemoryStore : StoreBase {
	struct Tag {
		public StoreObject blob;
		public byte[] data;
	}
	// Lock object guarding _files and _directories. Never hold the lock while running caller code
	// (streams, delegates, lazy enumerables), and never let a stored StoreObject out of it: callers get
	// clones, as the disk and cloud stores return new objects on every call.
	private readonly object _lock = new object();
	private readonly Dictionary<string, Tag> _files = new Dictionary<string, Tag>();
	private readonly HashSet<string> _directories = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "" };

	public override async Task<List<StoreObject>> ListObjects(StorageListOptions options, CancellationToken cancellationToken = default) {
		if (options == null) options = new StorageListOptions();

		List<KeyValuePair<string, Tag>> files;
		List<string> directories;
		lock (_lock) {
			files = _files.Select(p => new KeyValuePair<string, Tag>(p.Key, Snapshot(p.Value))).ToList();
			directories = _directories.ToList();
		}

		IEnumerable<KeyValuePair<string, Tag>> query = files;

		//limit by folder path
		if (options.Recurse) {
			if (!StoragePath.IsRootPath(options.FolderPath)) {
				string prefix = options.FolderPath + StoragePath.PathSeparatorString;

				query = query.Where(p => p.Key.StartsWith(prefix));
			}
		}
		else {
			var fPath = StoragePath.Normalize(options.FolderPath);

			// immediate subfolders, which the disk and cloud stores also return (DiskStore and Azure Blob
			// put them before the files); FilePrefix applies to them as it does on disk and S3
			IEnumerable<KeyValuePair<string, Tag>> subfolders = directories
				.Where(d => StoragePath.GetParent(d) == fPath)
				.Select(d => new KeyValuePair<string, Tag>(d, new Tag { blob = new StoreObject(d, StorageObjectType.Folder) }))
				.Where(p => options.FilePrefix == null || p.Value.blob.Name.StartsWith(options.FilePrefix));

			query = subfolders.Concat(query.Where(p => p.Value.blob.FolderPath == fPath));
		}

		//prefix
		query = query.Where(p => options.IsMatch(p.Value.blob));

		//browser filter
		query = query.Where(p => options.BrowseFilter == null || options.BrowseFilter(p.Value.blob));

		//limit
		if (options.MaxResults != null) {
			query = query.Take(options.MaxResults.Value);
		}

		List<StoreObject> matches = query.Select(p => p.Value.blob).ToList();

		return matches;
	}

	public override async Task SetObject(string fullPath, Stream sourceStream, bool append, CancellationToken cancellationToken = default) {
		await SetObject(fullPath, sourceStream, null, append, cancellationToken).ConfigureAwait(false);
	}

	public override async Task SetObject(string fullPath, Stream sourceStream, string contentType, bool append, CancellationToken cancellationToken = default) {
		if (fullPath == null) throw new ArgumentNullException(nameof(fullPath));
		fullPath = StoragePath.Normalize(fullPath);

		if (sourceStream is null)
			throw new ArgumentNullException(nameof(sourceStream));

		// Read the caller's stream before taking the lock. As before, a MemoryStream is read from the
		// start, except when appending to an existing object, which reads from its current position.
		long appendFrom = 0;
		if (sourceStream is MemoryStream ms) {
			appendFrom = ms.Position;
			ms.Position = 0;
		}
		byte[] data = sourceStream.ToByteArray();

		lock (_lock) {
			if (append && _files.TryGetValue(fullPath, out Tag tag))
				data = tag.data.Concat(data.Skip((int)appendFrom)).ToArray();
			Write(fullPath, data);
		}
	}

	public override async Task<Stream> OpenRead(string fullPath, CancellationToken cancellationToken = default) {
		if (fullPath == null) throw new ArgumentNullException(nameof(fullPath));
		fullPath = StoragePath.Normalize(fullPath);

		Tag tag;
		lock (_lock) {
			// return null if the object is not found
			if (!_files.TryGetValue(fullPath, out tag) || tag.data == null) return null;
		}

		// read-only, so callers cannot change the stored bytes, which are shared and never copied
		return new NonCloseableStream(new MemoryStream(tag.data, false));
	}
	public override async Task<Stream> OpenWrite(string fullPath, bool overwrite, CancellationToken cancellationToken = default) {
		if (fullPath == null) throw new ArgumentNullException(nameof(fullPath));
		fullPath = StoragePath.Normalize(fullPath);

		// return null if the object is exists and overwriting is not wanted
		if (!overwrite && await ObjectExists(fullPath, cancellationToken).ConfigureAwait(false))
			return null;

		MemoryStream stream = new MemoryStream();

		return new FixedStream(stream, null, async s => {
			s.Position = 0;
			await SetObject(fullPath, s, append: false, cancellationToken: cancellationToken).ConfigureAwait(false);
		});
	}
	public override async Task<Stream> OpenRange(string fullPath, long offset, long length, CancellationToken cancellationToken = default) {
		if (fullPath == null) throw new ArgumentNullException(nameof(fullPath));
		if (offset < 0) throw new ArgumentOutOfRangeException(nameof(offset));
		if (length < 0) throw new ArgumentOutOfRangeException(nameof(length));

		fullPath = StoragePath.Normalize(fullPath);

		Tag tag;
		lock (_lock) {
			// return null if the object is not found
			if (!_files.TryGetValue(fullPath, out tag) || tag.data == null)
				return null;
		}

		var stream = new MemoryStream(tag.data, false);
		stream.Position = offset;

		return stream;
	}

	public override async Task<bool> IsSeekable() {
		return true;
	}

	/// <summary>
	/// Deletes multiple objects by its full path.
	/// </summary>
	public override async Task DeleteObjects(IEnumerable<string> fullPaths, CancellationToken cancellationToken = default) {
		if (fullPaths == null) return;

		foreach (string fullPath in fullPaths) {
			await DeleteObject(fullPath);
		}
	}

	/// <summary>
	/// Deletes an object by its full path.
	/// </summary>
	/// <param name="fullPath">The full path.</param>
	/// <param name="cancellationToken">The cancellation token.</param>
	/// <returns></returns>
	public override async Task DeleteObject(string fullPath, CancellationToken cancellationToken = default) {
		if (fullPath == null) return;
		StoreObject pb = fullPath;
		lock (_lock) {
			if (_files.ContainsKey(pb)) {
				_files.Remove(pb);
			}
		}
	}

	public override async Task<List<bool>> ObjectsExists(IEnumerable<string> fullPaths, CancellationToken cancellationToken = default) {
		List<string> paths = fullPaths.ToList();
		var result = new List<bool>();

		lock (_lock) {
			foreach (string fullPath in paths) {
				result.Add(_files.ContainsKey(StoragePath.Normalize(fullPath)));
			}
		}

		return result;
	}

	public override async Task<StoreObject> GetObjectInfo(string path, CancellationToken cancellationToken = default) {
		return (await GetObjectsInfo(new List<string> { path }, cancellationToken).ConfigureAwait(false)).FirstOrDefault();
	}
	public override async Task<List<StoreObject>> GetObjectsInfo(IEnumerable<string> fullPaths, CancellationToken cancellationToken = default) {
		List<string> paths = fullPaths?.ToList();
		ArgValidator.AssertFullPaths(paths);

		var result = new List<StoreObject>();

		lock (_lock) {
			foreach (string fullPath in paths) {
				if (!_files.TryGetValue(StoragePath.Normalize(fullPath), out Tag tag)) {
					result.Add(null);
				}
				else {
					result.Add(Snapshot(tag).blob);
				}
			}
		}

		return result;
	}

	public override async Task SetObjectInfo(StoreObject obj, CancellationToken cancellationToken = default) {
		await SetObjectsInfo(new List<StoreObject> { obj }, cancellationToken).ConfigureAwait(false);
	}

	public override async Task SetObjectsInfo(IEnumerable<StoreObject> blobs, CancellationToken cancellationToken = default) {
		if (blobs == null)
			return;

		List<StoreObject> blobList = blobs.ToList();

		lock (_lock) {
			foreach (StoreObject blob in blobList) {
				if (_files.TryGetValue(blob, out Tag tag)) {
					tag.blob.Metadata.Clear();
					tag.blob.Metadata.AddRange(blob.Metadata);
				}
			}
		}
	}

	// A copy of a stored entry that callers may keep and change. The bytes are shared, as they are never
	// changed in place: a write stores a new array.
	private static Tag Snapshot(Tag tag) {
		return new Tag { blob = (StoreObject)tag.blob.Clone(), data = tag.data };
	}

	// Caller must hold _lock.
	private void Write(string fullPath, byte[] data) {
		if (!_files.TryGetValue(fullPath, out Tag tag)) {
			tag = new Tag {
				data = data,
				blob = new StoreObject(fullPath) {
					Size = data.Length,
					DateModified = DateTime.UtcNow,
					MD5 = data.MD5().ToHexString()
				}
			};
		}
		else {
			tag.data = data;
			tag.blob.Size = data.Length;
			tag.blob.DateModified = DateTime.UtcNow;
			tag.blob.MD5 = data.MD5().ToHexString();
		}
		_files[fullPath] = tag;

		AddVirtualFolderHierarchy(tag.blob.FolderPath);
	}

	// Caller must hold _lock.
	private void AddVirtualFolderHierarchy(string folderPath) {
		string path = StoragePath.Normalize(folderPath);

		while (true) {
			_directories.Add(path);

			if (StoragePath.IsRootPath(path))
				break;

			path = StoragePath.GetParent(path);
		}
	}

	public override async Task<bool> ObjectExists(string fullPath, CancellationToken cancellationToken = default) {
		if (fullPath == null) throw new ArgumentNullException(nameof(fullPath));

		lock (_lock) {
			return _files.ContainsKey(StoragePath.Normalize(fullPath));
		}
	}


	public override async Task<long> GetObjectLength(string fullPath, long defaultValue = -1, CancellationToken cancellationToken = default) {
		if (fullPath == null)
			return defaultValue;

		fullPath = StoragePath.Normalize(fullPath);

		lock (_lock) {
			return _files.TryGetValue(fullPath, out Tag tag) && tag.data != null
				? tag.data.LongLength
				: defaultValue;
		}
	}

	public override async Task CreateDirectory(string folderPath, bool force, CancellationToken cancellationToken = default) {
		if (folderPath == null) throw new ArgumentNullException(nameof(folderPath));

		folderPath = StoragePath.Normalize(folderPath);

		lock (_lock) {
			while (true) {
				_directories.Add(folderPath);

				if (StoragePath.IsRootPath(folderPath))
					break;

				folderPath = StoragePath.GetParent(folderPath);
			}
		}
	}

	public override async Task<bool> DirectoryExists(string folderPath, CancellationToken cancellationToken = default) {
		if (folderPath == null) throw new ArgumentNullException(nameof(folderPath));

		lock (_lock) {
			return _directories.Contains(StoragePath.Normalize(folderPath));
		}
	}

	public override async Task DeleteDirectory(string folderPath, bool recursive, CancellationToken cancellationToken = default) {
		if (folderPath == null) throw new ArgumentNullException(nameof(folderPath));

		folderPath = StoragePath.Normalize(folderPath);

		lock (_lock) {
			if (!_directories.Contains(folderPath))
				return;

			string prefix = folderPath.Length == 0 ? "" : folderPath + "/";

			if (recursive) {

				foreach (string file in _files.Keys.Where(x => x.StartsWith(prefix, StringComparison.OrdinalIgnoreCase)).ToList())
					_files.Remove(file);

				foreach (string dir in _directories.Where(x => x.StartsWith(prefix, StringComparison.OrdinalIgnoreCase)).ToList())
					_directories.Remove(dir);
			}
			else {

				if (_files.Keys.Any(x => StoragePath.GetParent(x) == folderPath))
					return;

				if (_directories.Any(x => StoragePath.GetParent(x) == folderPath))
					return;
			}

			_directories.Remove(folderPath);
		}
	}

	public override async Task MoveDirectory(string sourceFolderPath, string destinationFolderPath, CancellationToken cancellationToken = default) {
		if (sourceFolderPath == null) throw new ArgumentNullException(nameof(sourceFolderPath));
		if (destinationFolderPath == null) throw new ArgumentNullException(nameof(destinationFolderPath));

		sourceFolderPath = StoragePath.Normalize(sourceFolderPath);
		destinationFolderPath = StoragePath.Normalize(destinationFolderPath);

		string prefix = sourceFolderPath.Length == 0 ? "" : sourceFolderPath + "/";

		lock (_lock) {
			List<string> filesToMove = _files.Keys.Where(x => x.StartsWith(prefix, StringComparison.OrdinalIgnoreCase)).ToList();
			List<string> foldersToMove = _directories.Where(x => x.StartsWith(prefix, StringComparison.OrdinalIgnoreCase)).ToList();

			// nothing to move: not even the destination is created
			if (!_directories.Contains(sourceFolderPath) && filesToMove.Count == 0 && foldersToMove.Count == 0)
				return;

			foreach (string oldPath in filesToMove) {

				string relative = oldPath.Substring(prefix.Length);
				string newPath = StoragePath.Combine(destinationFolderPath, relative);

				Tag tag = _files[oldPath];
				tag.blob.SetFullPath(newPath);

				_files.Remove(oldPath);
				_files[newPath] = tag;
			}

			// remove every old folder record before adding the new ones, as a moved folder may land on
			// the old path of the source or of another moved folder
			if (!StoragePath.IsRootPath(sourceFolderPath))
				_directories.Remove(sourceFolderPath);
			foreach (string dir in foldersToMove)
				_directories.Remove(dir);

			foreach (string dir in foldersToMove) {
				string relative = dir.Substring(prefix.Length);
				_directories.Add(StoragePath.Combine(destinationFolderPath, relative));
			}

			AddVirtualFolderHierarchy(destinationFolderPath);
		}
	}

	public override async Task<bool> MoveObject(string oldPath, string newPath, bool overwrite, CancellationToken cancellationToken = default) {
		if (oldPath == null) throw new ArgumentNullException(nameof(oldPath));
		if (newPath == null) throw new ArgumentNullException(nameof(newPath));

		oldPath = StoragePath.Normalize(oldPath);
		newPath = StoragePath.Normalize(newPath);

		lock (_lock) {
			// source must exist
			if (!_files.TryGetValue(oldPath, out Tag tag) || tag.data == null)
				return false;

			// destination exists and overwrite disabled
			if (!overwrite && _files.ContainsKey(newPath))
				return false;

			// remove destination if overwriting
			if (overwrite && _files.ContainsKey(newPath))
				_files.Remove(newPath);

			// move the object
			tag.blob.SetFullPath(newPath);
			tag.blob.DateModified = DateTime.UtcNow;

			_files.Remove(oldPath);
			_files[newPath] = tag;

			// ensure destination folder hierarchy exists
			AddVirtualFolderHierarchy(tag.blob.FolderPath);

			return true;
		}
	}

}
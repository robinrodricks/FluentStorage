using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using FluentStorage.Enums;
using FluentStorage.Model;
using FluentStorage.Storage;
using FluentStorage.Streaming;
using MimeMapping;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.GridFS;

namespace FluentStorage.Mongo.Storage;

/// <summary>
/// Manages a single MongoDB GridFS bucket using the native MongoDB Driver.
/// </summary>
public class MongoGridStore : StoreBase {
	private MongoClient _client;
	private IMongoDatabase _database;
	private string _bucketName;

	private GridFSBucket _bucket;

	// ------------------------------------------------------------------
	// Constructors
	// ------------------------------------------------------------------

	public MongoGridStore(MongoClient client, IMongoDatabase database, string bucketName = "fs") {
		if (client == null) throw new ArgumentNullException(nameof(client));
		if (database == null) throw new ArgumentNullException(nameof(database));
		if (string.IsNullOrWhiteSpace(bucketName)) throw new ArgumentNullException(nameof(bucketName));

		_client = client;
		_database = database;
		Construct(null, bucketName);

	}
	public MongoGridStore(string connectionString, string databaseName, string bucketName = "fs") {
		if (string.IsNullOrWhiteSpace(connectionString)) throw new ArgumentNullException(nameof(connectionString));
		if (string.IsNullOrWhiteSpace(databaseName)) throw new ArgumentNullException(nameof(databaseName));
		if (string.IsNullOrWhiteSpace(bucketName)) throw new ArgumentNullException(nameof(bucketName));

		_client = new MongoClient(connectionString);
		Construct(databaseName, bucketName);
	}

	public MongoGridStore(string host, int port, string username, string password, string databaseName,
		string bucketName = "fs", string authDatabase = null, bool useSsl = false) {
		if (string.IsNullOrWhiteSpace(host)) throw new ArgumentNullException(nameof(host));
		if (string.IsNullOrWhiteSpace(username)) throw new ArgumentNullException(nameof(username));
		if (string.IsNullOrWhiteSpace(password)) throw new ArgumentNullException(nameof(password));
		if (string.IsNullOrWhiteSpace(databaseName)) throw new ArgumentNullException(nameof(databaseName));
		if (string.IsNullOrWhiteSpace(bucketName)) throw new ArgumentNullException(nameof(bucketName));

		var credential = new MongoCredential(
			"SCRAM-SHA-256",
			new MongoInternalIdentity(authDatabase ?? databaseName, username),
			new PasswordEvidence(password));

		var settings = new MongoClientSettings {
			Server = new MongoServerAddress(host, port),
			Credential = credential,
			UseTls = useSsl,
				
		};

		_client = new MongoClient(settings);
		Construct(databaseName, bucketName);
	}

	public MongoGridStore(string host, int port, X509Certificate2 clientCertificate, string databaseName,
		string bucketName = "fs") {

		if (string.IsNullOrWhiteSpace(host)) throw new ArgumentNullException(nameof(host));
		if (clientCertificate == null) throw new ArgumentNullException(nameof(clientCertificate));
		if (string.IsNullOrWhiteSpace(databaseName)) throw new ArgumentNullException(nameof(databaseName));
		if (string.IsNullOrWhiteSpace(bucketName)) throw new ArgumentNullException(nameof(bucketName));

		var settings = new MongoClientSettings {
			Server = new MongoServerAddress(host, port),
			Credential = MongoCredential.CreateMongoX509Credential(),
			UseTls = true,
			SslSettings = new SslSettings {
				ClientCertificates = new[] { clientCertificate }
			}
		};

		_client = new MongoClient(settings);
		Construct(databaseName, bucketName);
	}

	public MongoGridStore(MongoClientSettings clientSettings, string databaseName, string bucketName = "fs") {
		if (clientSettings == null) throw new ArgumentNullException(nameof(clientSettings));
		if (string.IsNullOrWhiteSpace(databaseName)) throw new ArgumentNullException(nameof(databaseName));
		if (string.IsNullOrWhiteSpace(bucketName)) throw new ArgumentNullException(nameof(bucketName));

		_client = new MongoClient(clientSettings);
		Construct(databaseName, bucketName);
	}

	private void Construct(string databaseName, string bucketName) {
		if (databaseName != null) {
			_database = _client.GetDatabase(databaseName);
		}
		_bucketName = bucketName;
		_bucket = new GridFSBucket(_database, new GridFSBucketOptions {
			BucketName = _bucketName
		});
	}

	// ------------------------------------------------------------------
	// Client / server info
	// ------------------------------------------------------------------

	public override async Task<object> GetClient() {
		return _client;
	}

	/// <summary>
	/// Gets information about the connected MongoDB server.
	///
	/// Returns a dictionary with server capabilities, build information and
	/// protocol limits.
	/// </summary>
	public override async Task<Dictionary<string, object>> GetServer(CancellationToken cancellationToken = default) {

		var database = _client.GetDatabase("admin");

		BsonDocument buildInfo = await database.RunCommandAsync<BsonDocument>(new BsonDocument("buildInfo", 1), null, cancellationToken).ConfigureAwait(false);

		BsonDocument hello = await database.RunCommandAsync<BsonDocument>(new BsonDocument("hello", 1), null, cancellationToken).ConfigureAwait(false);

		static object Get(BsonDocument doc, string name) {
			if (!doc.TryGetValue(name, out BsonValue value) || value.IsBsonNull)
				return null;

			return value.BsonType switch {
				BsonType.Array => value.AsBsonArray.Select(v => BsonTypeMapper.MapToDotNetValue(v)).ToArray(),
				BsonType.Document => value.AsBsonDocument,
				_ => BsonTypeMapper.MapToDotNetValue(value)
			};
		}

		string serverType = hello.Contains("msg") && hello["msg"] == "isdbgrid" ? "Mongos" :
			hello.Contains("setName") ? "ReplicaSet" : "Standalone";

		return new Dictionary<string, object> {

			// Protocol
			["ProtocolVersion"] = Get(buildInfo, "maxWireVersion"),
			["MinProtocolVersion"] = Get(buildInfo, "minWireVersion"),

			// Server
			["ServerVersion"] = Get(buildInfo, "version"),
			["GitVersion"] = Get(buildInfo, "gitVersion"),
			["ClientVersion"] = typeof(MongoClient).Assembly.GetName().Version?.ToString(),

			// Build
			["Bits"] = Get(buildInfo, "bits"),
			["Debug"] = Get(buildInfo, "debug"),
			["Allocator"] = Get(buildInfo, "allocator"),
			["JavascriptEngine"] = Get(buildInfo, "javascriptEngine"),
			["Modules"] = Get(buildInfo, "modules"),
			["StorageEngines"] = Get(buildInfo, "storageEngines"),
			["OpenSSLVersion"] = buildInfo.TryGetValue("openssl", out var openssl) && openssl.IsBsonDocument
				? Get(openssl.AsBsonDocument, "running")
				: null,
			["BuildEnvironment"] = Get(buildInfo, "buildEnvironment"),
			["CompilerFlags"] = Get(buildInfo, "compilerFlags"),
			["TargetArchitecture"] = Get(buildInfo, "target_arch"),

			// Limits
			["MaxBsonObjectSize"] = Get(hello, "maxBsonObjectSize"),
			["MaxMessageSizeBytes"] = Get(hello, "maxMessageSizeBytes"),
			["MaxWriteBatchSize"] = Get(hello, "maxWriteBatchSize"),
			["LogicalSessionTimeout"] = Get(hello, "logicalSessionTimeoutMinutes"),
			["ReadOnly"] = Get(hello, "readOnly"),

			// Topology
			["ServerType"] = serverType,
			["IsWritablePrimary"] = Get(hello, "isWritablePrimary"),
			["ReplicaSetName"] = Get(hello, "setName"),
			["Hosts"] = Get(hello, "hosts"),
			["Passives"] = Get(hello, "passives"),
			["Arbiters"] = Get(hello, "arbiters"),
			["CompressionAlgorithms"] = Get(hello, "compression"),
		};
	}

	// ------------------------------------------------------------------
	// Write
	// ------------------------------------------------------------------

	public override async Task SetObject(string fullPath, Stream dataStream, bool append = false, CancellationToken cancellationToken = default) {
		await SetObject(fullPath, dataStream, null, append, cancellationToken).ConfigureAwait(false);
	}

	public override async Task SetObject(string fullPath, Stream dataStream, string contentType, bool append = false, CancellationToken cancellationToken = default) {
		if (string.IsNullOrWhiteSpace(fullPath)) throw new ArgumentNullException(nameof(fullPath));
		if (dataStream == null) throw new ArgumentNullException(nameof(dataStream));

		string path = StoragePath.Normalize(fullPath);
		contentType ??= MimeUtility.GetMimeMapping(fullPath);


		Stream uploadStream = dataStream;
		MemoryStream combinedStream = null;

		try {
			if (append) {
				// GridFS files are immutable: emulate append by combining existing
				// content (if any) with the new content and re-uploading as one file.
				GridFSFileInfo existing = await FindLatestAsync(_bucket, path, cancellationToken).ConfigureAwait(false);
				if (existing != null) {
					combinedStream = new MemoryStream();
					using (Stream existingStream = await _bucket.OpenDownloadStreamAsync(existing.Id, cancellationToken: cancellationToken).ConfigureAwait(false)) {
						await existingStream.CopyToAsync(combinedStream, 81920, cancellationToken).ConfigureAwait(false);
					}
					await dataStream.CopyToAsync(combinedStream, 81920, cancellationToken).ConfigureAwait(false);
					combinedStream.Position = 0;
					uploadStream = combinedStream;
				}
			}

			// Remove any previous revision(s) so SetObject behaves like an overwrite/upsert.
			await DeleteAllRevisionsAsync(_bucket, path, cancellationToken).ConfigureAwait(false);

			var options = new GridFSUploadOptions {
				Metadata = new BsonDocument
				{
					{ "contentType", contentType ?? "application/octet-stream" }
				}
			};

			await _bucket.UploadFromStreamAsync(path, uploadStream, options, cancellationToken).ConfigureAwait(false);
		}
		finally {
			combinedStream?.Dispose();
		}
	}

	// ------------------------------------------------------------------
	// Read
	// ------------------------------------------------------------------

	public override async Task<Stream> OpenRead(string fullPath, CancellationToken cancellationToken = default) {
		if (string.IsNullOrWhiteSpace(fullPath)) throw new ArgumentNullException(nameof(fullPath));

		string path = StoragePath.Normalize(fullPath);

		try {

			return await _bucket.OpenDownloadStreamByNameAsync(path, cancellationToken: cancellationToken).ConfigureAwait(false);
		}
		catch (GridFSFileNotFoundException) {

			// return null if the object does not exist
			return null;
		}
	}

	public override async Task<Stream> OpenWrite(string fullPath, bool overwrite, CancellationToken cancellationToken = default) {
		if (string.IsNullOrWhiteSpace(fullPath)) throw new ArgumentNullException(nameof(fullPath));

		string path = StoragePath.Normalize(fullPath);

		if (!overwrite && await ObjectExists(path, cancellationToken).ConfigureAwait(false)){
			//throw new StorageException($"Object '{path}' already exists and overwrite is disabled.");
			return null;
		}

		var stream = new MemoryStream();

		return new FixedStream(stream, null, async s => {
			s.Position = 0;
			await SetObject(path, s, append: false, cancellationToken: cancellationToken).ConfigureAwait(false);
		});
	}

	// ------------------------------------------------------------------
	// Seeking/Streaming
	// ------------------------------------------------------------------

	public override async Task<Stream> OpenRange(string path, long offset, long length, CancellationToken cancellationToken = default) {
		if (string.IsNullOrWhiteSpace(path)) throw new ArgumentNullException(nameof(path));
		if (offset < 0) throw new ArgumentOutOfRangeException(nameof(offset));
		if (length < 0) throw new ArgumentOutOfRangeException(nameof(length));

		string normalized = StoragePath.Normalize(path);

		try {

			var opt = new GridFSDownloadByNameOptions();
			opt.Seekable = true;
			var downloadStream = await _bucket.OpenDownloadStreamByNameAsync(normalized, opt, cancellationToken).ConfigureAwait(false);

			// we hope this stream is seekable, else the entire seeking/streaming functionality will be broken,
			// more testing and feedback is required
			downloadStream.Seek(offset, SeekOrigin.Begin);
			return downloadStream;
		}
		catch (GridFSFileNotFoundException) {

			// return null if the object does not exist
			return null;
		}
	}

	public override async Task<bool> IsSeekable() {
		return true;
	}

	public override async Task<long> GetObjectLength(string path, long defaultValue = -1, CancellationToken cancellationToken = default) {
		if (string.IsNullOrWhiteSpace(path)) return defaultValue;

		try {
			GridFSFileInfo info = await FindLatestAsync(_bucket, StoragePath.Normalize(path), cancellationToken).ConfigureAwait(false);
			return info?.Length ?? defaultValue;
		}
		catch {
			// Per spec: absorb all exceptions here and fall back to defaultValue.
			return defaultValue;
		}
	}

	// ------------------------------------------------------------------
	// Delete
	// ------------------------------------------------------------------

	public override async Task DeleteObjects(IEnumerable<string> fullPaths, CancellationToken cancellationToken = default) {
		if (fullPaths == null) throw new ArgumentNullException(nameof(fullPaths));


		foreach (string path in fullPaths) {
			if (string.IsNullOrWhiteSpace(path)) continue;
			await DeleteAllRevisionsAsync(_bucket, StoragePath.Normalize(path), cancellationToken).ConfigureAwait(false);
		}
	}

	public override async Task DeleteObject(string fullPath, CancellationToken cancellationToken = default) {
		if (string.IsNullOrWhiteSpace(fullPath)) throw new ArgumentNullException(nameof(fullPath));

		await DeleteAllRevisionsAsync(_bucket, StoragePath.Normalize(fullPath), cancellationToken).ConfigureAwait(false);
	}

	private static async Task DeleteAllRevisionsAsync(GridFSBucket gridBucket, string path, CancellationToken cancellationToken) {
		FilterDefinition<GridFSFileInfo> filter = Builders<GridFSFileInfo>.Filter.Eq(f => f.Filename, path);

		List<GridFSFileInfo> matches;
		using (IAsyncCursor<GridFSFileInfo> cursor = await gridBucket.FindAsync(filter, cancellationToken: cancellationToken).ConfigureAwait(false)) {
			matches = await cursor.ToListAsync(cancellationToken).ConfigureAwait(false);
		}

		foreach (GridFSFileInfo file in matches) {
			try {
				await gridBucket.DeleteAsync(file.Id, cancellationToken).ConfigureAwait(false);
			}
			catch (GridFSFileNotFoundException) {
				// Harmless race - another caller already deleted this revision. Suppress.
			}
		}
	}

	public override async Task DeleteDirectory(string folderPath, bool recursive, CancellationToken cancellationToken = default) {
		if (string.IsNullOrWhiteSpace(folderPath)) throw new ArgumentNullException(nameof(folderPath));

		List<StoreObject> items = await ListDirectory(folderPath, recursive, cancellationToken).ConfigureAwait(false);
		if (items == null || items.Count == 0) return;


		foreach (StoreObject item in items.Where(i => i.Type == StorageObjectType.File)) {
			string fullPath = string.IsNullOrEmpty(item.FolderPath) ? item.Name : $"{item.FolderPath}/{item.Name}";
			await DeleteAllRevisionsAsync(_bucket, StoragePath.Normalize(fullPath), cancellationToken).ConfigureAwait(false);
		}
	}

	// ------------------------------------------------------------------
	// Existence / info
	// ------------------------------------------------------------------

	public override async Task<List<bool>> ObjectsExists(IEnumerable<string> fullPaths, CancellationToken cancellationToken = default) {
		if (fullPaths == null) throw new ArgumentNullException(nameof(fullPaths));

		var results = new List<bool>();
		foreach (string path in fullPaths) {
			results.Add(await ObjectExists(path, cancellationToken).ConfigureAwait(false));
		}
		return results;
	}

	public override async Task<bool> ObjectExists(string fullPath, CancellationToken cancellationToken = default) {
		if (string.IsNullOrWhiteSpace(fullPath)) throw new ArgumentNullException(nameof(fullPath));

		GridFSFileInfo info = await FindLatestAsync(_bucket, StoragePath.Normalize(fullPath), cancellationToken).ConfigureAwait(false);
		return info != null;
	}

	public override async Task<List<StoreObject>> GetObjectsInfo(IEnumerable<string> fullPaths, CancellationToken cancellationToken = default) {
		if (fullPaths == null) throw new ArgumentNullException(nameof(fullPaths));

		var results = new List<StoreObject>();
		foreach (string path in fullPaths) {
			StoreObject info = await GetObjectInfo(path, cancellationToken).ConfigureAwait(false);
			if (info != null) results.Add(info);
		}
		return results;
	}

	public override async Task<StoreObject> GetObjectInfo(string fullPath, CancellationToken cancellationToken = default) {
		if (string.IsNullOrWhiteSpace(fullPath)) throw new ArgumentNullException(nameof(fullPath));

		string path = StoragePath.Normalize(fullPath);

		GridFSFileInfo info = await FindLatestAsync(_bucket, path, cancellationToken).ConfigureAwait(false);
		if (info == null) return null;

		return ToStoreObject(info, includeAttributes: true);
	}

	public override async Task SetObjectInfo(StoreObject obj, CancellationToken cancellationToken = default) {
		if (obj == null) throw new ArgumentNullException(nameof(obj));

		await SetObjectsInfo(new[] { obj }, cancellationToken).ConfigureAwait(false);
	}

	public override async Task SetObjectsInfo(IEnumerable<StoreObject> objs, CancellationToken cancellationToken = default) {
		if (objs == null) throw new ArgumentNullException(nameof(objs));

		// GridFS metadata lives on the "{_bucket}.files" collection. Renaming isn't handled
		// here (use MoveObject for that) - this only patches the free-form metadata bag.

		IMongoCollection<BsonDocument> filesCollection = _database.GetCollection<BsonDocument>($"{_bucketName}.files");

		foreach (StoreObject obj in objs) {
			if (obj == null) continue;

			string fullPath = string.IsNullOrEmpty(obj.FolderPath) ? obj.Name : $"{obj.FolderPath}/{obj.Name}";
			string path = StoragePath.Normalize(fullPath);

			var metadataDoc = new BsonDocument();
			foreach (KeyValuePair<string, string> kv in obj.Metadata) {
				metadataDoc[kv.Key] = kv.Value != null ? kv.Value : BsonNull.Value;
			}

			FilterDefinition<BsonDocument> filter = Builders<BsonDocument>.Filter.Eq("filename", path);
			UpdateDefinition<BsonDocument> update = Builders<BsonDocument>.Update.Set("metadata", metadataDoc);

			await filesCollection.UpdateManyAsync(filter, update, cancellationToken: cancellationToken).ConfigureAwait(false);
		}
	}

	private async Task<GridFSFileInfo> FindLatestAsync(GridFSBucket gridBucket, string path, CancellationToken cancellationToken) {
		FilterDefinition<GridFSFileInfo> filter = Builders<GridFSFileInfo>.Filter.Eq(f => f.Filename, path);
		SortDefinition<GridFSFileInfo> sort = Builders<GridFSFileInfo>.Sort.Descending(f => f.UploadDateTime);

		var options = new GridFSFindOptions { Sort = sort, Limit = 1 };

		using IAsyncCursor<GridFSFileInfo> cursor = await gridBucket.FindAsync(filter, options, cancellationToken).ConfigureAwait(false);
		List<GridFSFileInfo> matches = await cursor.ToListAsync(cancellationToken).ConfigureAwait(false);
		return matches.FirstOrDefault();
	}

	private static StoreObject ToStoreObject(GridFSFileInfo info, bool includeAttributes) {
		var (folderPath, name) = SplitPath(info.Filename);

		var so = new StoreObject(folderPath, name, StorageObjectType.File) {
			Size = info.Length,
			DateCreated = info.UploadDateTime,
			DateModified = info.UploadDateTime
		};

		if (includeAttributes) {
			so.TryAddProperties(
				"Id", info.Id.ToString(),
				"ChunkSizeBytes", info.ChunkSizeBytes);

			if (info.Metadata != null) {
				var metaDict = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
				foreach (BsonElement el in info.Metadata.Elements) {
					metaDict[el.Name] = el.Value.IsBsonNull ? null : el.Value.ToString();
				}

				so.TryAddPropertiesFromDictionary(metaDict, metaDict.Keys.ToArray());
			}
		}

		return so;
	}

	// ------------------------------------------------------------------
	// Move
	// ------------------------------------------------------------------

	public override async Task<bool> MoveObject(string oldPath, string newPath, bool overwrite, CancellationToken cancellationToken = default) {
		if (string.IsNullOrWhiteSpace(oldPath)) throw new ArgumentNullException(nameof(oldPath));
		if (string.IsNullOrWhiteSpace(newPath)) throw new ArgumentNullException(nameof(newPath));

		string from = StoragePath.Normalize(oldPath);
		string to = StoragePath.Normalize(newPath);


		GridFSFileInfo source = await FindLatestAsync(_bucket, from, cancellationToken).ConfigureAwait(false);
		if (source == null) return false;

		GridFSFileInfo destination = await FindLatestAsync(_bucket, to, cancellationToken).ConfigureAwait(false);
		if (destination != null) {
			if (!overwrite) return false;
			await DeleteAllRevisionsAsync(_bucket, to, cancellationToken).ConfigureAwait(false);
		}

		// RenameAsync is a metadata-only operation server-side - no chunk data is re-written.
		await _bucket.RenameAsync(source.Id, to, cancellationToken).ConfigureAwait(false);
		return true;
	}

	// ------------------------------------------------------------------
	// Listing
	// ------------------------------------------------------------------

	public override async Task<List<StoreObject>> ListObjects(StorageListOptions options = null, CancellationToken cancellationToken = default) {
		options ??= new StorageListOptions();


		string prefix = StoragePath.Normalize(options.FolderPath);
		string prefixWithSlash = string.IsNullOrEmpty(prefix) ? string.Empty : prefix + "/";

		// build the search criteria
		FilterDefinition<GridFSFileInfo> filter = Builders<GridFSFileInfo>.Filter.Empty;
		if (!string.IsNullOrEmpty(prefixWithSlash)) {
			filter = Builders<GridFSFileInfo>.Filter.Regex(
				f => f.Filename,
				new BsonRegularExpression("^" + Regex.Escape(prefixWithSlash)));
		}

		var findOptions = new GridFSFindOptions();
		if (options.PageSize.HasValue) findOptions.BatchSize = options.PageSize;

		// collect all files that meet the search criteria
		List<GridFSFileInfo> allFiles;
		using (IAsyncCursor<GridFSFileInfo> cursor = await _bucket.FindAsync(filter, findOptions, cancellationToken).ConfigureAwait(false)) {
			allFiles = await cursor.ToListAsync(cancellationToken).ConfigureAwait(false);
		}

		// Only keep the newest revision per filename (GridFS allows duplicate filenames).
		List<GridFSFileInfo> latestPerFile = allFiles
			.GroupBy(f => f.Filename, StringComparer.OrdinalIgnoreCase)
			.Select(g => g.OrderByDescending(f => f.UploadDateTime).First())
			.OrderBy(f => f.Filename, StringComparer.OrdinalIgnoreCase)
			.ToList();

		var result = new List<StoreObject>();
		var seenFolders = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

		// per latest file
		foreach (GridFSFileInfo info in latestPerFile) {

			string filename = info.Filename;
			if (!string.IsNullOrEmpty(prefixWithSlash) &&
			    !filename.StartsWith(prefixWithSlash, StringComparison.OrdinalIgnoreCase)) {
				continue;
			}

			string relative = string.IsNullOrEmpty(prefixWithSlash)
				? filename
				: filename.Substring(prefixWithSlash.Length);

			if (string.IsNullOrEmpty(relative)) continue;

			int slashIdx = relative.IndexOf('/');

			// skip this if it is inside a "subfolder" and recursion is disabled
			if (!options.Recurse && slashIdx >= 0) {

				// Emit a synthetic folder entry, once, for this immediate subfolder.
				/*string folderName = relative.Substring(0, slashIdx);
				string folderFullPath = string.IsNullOrEmpty(prefix) ? folderName : $"{prefix}/{folderName}";

				if (seenFolders.Add(folderFullPath)) {
					result.Add(new StoreObject(prefix, folderName, StorageObjectType.Folder));
				}*/

				continue;
			}

			string fileName = slashIdx >= 0 ? relative.Substring(relative.LastIndexOf('/') + 1) : relative;

			if (!string.IsNullOrEmpty(options.FilePrefix) &&
			    !fileName.StartsWith(options.FilePrefix, StringComparison.OrdinalIgnoreCase)) {
				continue;
			}

			result.Add(ToStoreObject(info, options.IncludeAttributes));

			if (options.MaxResults.HasValue && result.Count >= options.MaxResults.Value) {
				break;
			}
		}

		return result;
	}

	// ------------------------------------------------------------------
	// Path helpers
	// ------------------------------------------------------------------

	private static (string folderPath, string name) SplitPath(string fullPath) {
		string normalized = StoragePath.Normalize(fullPath);
		int idx = normalized.LastIndexOf('/');
		if (idx < 0) return (string.Empty, normalized);
		return (normalized.Substring(0, idx), normalized.Substring(idx + 1));
	}

	// ------------------------------------------------------------------
	// Dir exists
	// ------------------------------------------------------------------

	/// <summary>
	/// Fastest possible implemention to check if a virtual directory exists in a Mongo GridFS store.
	/// Query for a single file whose filename begins with the directory prefix.
	/// </summary>
	public override async Task<bool> DirectoryExists(string fullPath, CancellationToken cancellationToken = default) {
		if (fullPath == null) throw new ArgumentNullException(nameof(fullPath));
		fullPath = StoragePath.Normalize(fullPath);

		// do not check root directory
		if (fullPath.Length == 0) return true;

		if (!fullPath.EndsWith("/"))
			fullPath += "/";

		// craete filter for the virtual dir path
		FilterDefinition<GridFSFileInfo> filter = Builders<GridFSFileInfo>.Filter
			.Regex(x => x.Filename, new BsonRegularExpression("^" + Regex.Escape(fullPath)));

		// only 1 result wanted
		GridFSFindOptions options = new GridFSFindOptions {Limit = 1};

		// run find query
		using IAsyncCursor<GridFSFileInfo> cursor = await _bucket
			.FindAsync(filter, options, cancellationToken).ConfigureAwait(false);
		return await cursor.MoveNextAsync(cancellationToken).ConfigureAwait(false) && cursor.Current.Any();
	}

}
using FluentStorage.Utils.Hashing;

namespace FluentStorage.Tests.Integration.Storage.TestSuite;

public partial class IStoreTest {


	// ---------------------------------------------------------------------
	// Old tests (2021-2025)
	// ---------------------------------------------------------------------


	[Fact]
	public async Task List_All_DoesntCrash() {
		await _storage.ListObjects();
	}

	[Fact]
	public async Task List_RootFolder_HasAtLeastOne() {
		string targetId = RandomBlobPath();

		await _storage.SetText(targetId, "test");

		List<StoreObject> rootContent = await _storage.ListObjects();

		Assert.NotEmpty(rootContent);
	}

	[Fact]
	public async Task List_ByFilePrefix_Filtered() {
		string prefix = RandomGenerator.RandomString;

		int countBefore = (await _storage.ListObjects(new StorageListOptions { FolderPath = _blobPrefix, FilePrefix = prefix })).Count;

		string id1 = RandomBlobPath(prefix);
		string id2 = RandomBlobPath(prefix);
		string id3 = RandomBlobPath();

		await _storage.SetText(id1, RandomGenerator.RandomString);
		await _storage.SetText(id2, RandomGenerator.RandomString);
		await _storage.SetText(id3, RandomGenerator.RandomString);

		List<StoreObject> items = (await _storage.ListObjects(new StorageListOptions { FolderPath = _blobPrefix, FilePrefix = prefix }));
		Assert.Equal(2 + countBefore, items.Count); //2 files + containing folder
	}

	[Fact]
	public async Task List_FilesInFolder_NonRecursive() {
		string id = RandomBlobPath();

		await _storage.SetText(id, RandomGenerator.RandomString);

		List<StoreObject> items = (await _storage.ListObjects(new StorageListOptions { FolderPath = _blobPrefix, Recurse = false })).ToList();

		Assert.True(items.Count > 0);

		StoreObject tid = items.FirstOrDefault(i => i.FullPath == id);
		Assert.NotNull(tid);
	}

	[Fact]
	public async Task List_FilesInFolder_Recursive() {
		string folderPath = RandomBlobPath();
		string id1 = StoragePath.Combine(folderPath, "1.txt");
		string id2 = StoragePath.Combine(folderPath, "sub", "2.txt");
		string id3 = StoragePath.Combine(folderPath, "sub", "3.txt");

		try {
			await _storage.SetText(id1, RandomGenerator.RandomString);
			await _storage.SetText(id2, RandomGenerator.RandomString);
			await _storage.SetText(id3, RandomGenerator.RandomString);

			List<StoreObject> items = (await _storage.ListDirectory(folderPath, true))
				.Where(f => f.Type == StorageObjectType.File).ToList();

			Assert.Equal(3, items.Count); //1.txt + sub (folder) + 2.txt + 3.txt

		}
		catch (NotSupportedException) {
			//it ok for providers not to support hierarchy
		}
	}

	[Fact]
	public async Task List_InNonExistingFolder_EmptyCollection() {
		IEnumerable<StoreObject> objects = await _storage.ListObjects(new StorageListOptions { FolderPath = RandomBlobPath() });

		Assert.NotNull(objects);
		Assert.True(objects.Count() == 0);
	}

	[Fact]
	public async Task List_FilesInNonExistingFolder_EmptyCollection() {
		IEnumerable<StoreObject> objects = await (_storage as StoreBase).ListFileObjects(new StorageListOptions { FolderPath = RandomBlobPath() });

		Assert.NotNull(objects);
		Assert.True(objects.Count() == 0);
	}

	[Fact]
	public async Task List_VeryLongPrefix_NoResultsNoCrash() {
		await Assert.ThrowsAsync<ArgumentException>(async () => await _storage.ListObjects(new StorageListOptions { FilePrefix = RandomGenerator.GetRandomString(100000, false) }));
	}

	[Fact]
	public async Task List_limited_number_of_results() {
		string prefix = RandomGenerator.RandomString;
		string id1 = RandomBlobPath(prefix);
		string id2 = RandomBlobPath(prefix);
		await _storage.SetText(id1, RandomGenerator.RandomString);
		await _storage.SetText(id2, RandomGenerator.RandomString);

		int countAll = (await (_storage as StoreBase).ListFileObjects(new StorageListOptions { FolderPath = _blobPrefix, FilePrefix = prefix })).Count;
		int countOne = (await _storage.ListObjects(new StorageListOptions { FolderPath = _blobPrefix, FilePrefix = prefix, MaxResults = 1 })).Count;

		Assert.Equal(2, countAll);
		Assert.Equal(1, countOne);
	}

	[Fact]
	public async Task List_with_browsefilter_calls_filter() {

		// not for FS
		if (await _storage.IsFileSystem()) return;


		string id1 = RandomBlobPath();
		string id2 = RandomBlobPath();
		await _storage.SetText(id1, RandomGenerator.RandomString);
		await _storage.SetText(id2, RandomGenerator.RandomString);

		//dump compare
		List<StoreObject> files = await (_storage as StoreBase).ListFileObjects(new StorageListOptions {
			FolderPath = _blobPrefix,
			Recurse = true
		});
		Assert.Contains(files, f => f.FullPath == id1 && f.Type == StorageObjectType.File);

		//server-side filtering
		files = await (_storage as StoreBase).ListFileObjects(new StorageListOptions {
			FolderPath = _blobPrefix,
			Recurse = true,
			BrowseFilter = id => (id.Type != StorageObjectType.File || id.FullPath == id1)
		});


		Assert.Single(files);
		Assert.Equal(id1, files.First().FullPath);
	}

	//[Fact]
	public async Task List_large_number_of_results() {
		const int count = 500;
		//arrange

		//something like FTP doesn't support multiple connections, however this should be implemented in FTP provider itself

		for (int it = 0; it < 50; it++) {
			await Task.WhenAll(Enumerable.Range(0, 10).Select(i => _storage.SetText(RandomBlobPath(), "123")));
		}

		//act
		List<StoreObject> blobs = await _storage.ListDirectory(folderPath: _blobPrefix);

		//assert
		Assert.True(blobs.Count >= count, $"expected over {count}, but received only {blobs.Count}");
	}

	[Fact]
	public async Task List_folder_nonrecursively_no_children() {
		try {
			string sub = RandomBlobPath() + "/";

			await _storage.SetText(sub + "one.txt", "test");
			await _storage.SetText(sub + "sub/two.txt", "test");

			List<StoreObject> subItems = await _storage.ListDirectory(sub, false);
			Assert.Equal(2, subItems.Count);


			Assert.Contains(new StoreObject(sub + "one.txt"), subItems);
			Assert.Contains(new StoreObject(sub + "sub", StorageObjectType.Folder), subItems);
		}
		catch (NotSupportedException) {
			//hierarchy not supported
		}
	}

	[Fact]
	public async Task GetBlob_for_one_file_succeeds() {
		string content = RandomGenerator.GetRandomString(1000, false);
		string id = RandomBlobPath();

		await _storage.SetText(id, content);

		StoreObject meta = await _storage.GetObjectInfo(id);

		var bytes = Encoding.UTF8.GetBytes(content);
		long size = bytes.Length;
		string md5 = HashUtility.HashBytes(bytes, StorageHash.MD5);

		if (meta.Size != null)
			Assert.Equal(size, meta.Size);
		if (meta.MD5 != null)
			Assert.Equal(md5, meta.MD5);
		if (meta.DateModified != null)
			Assert.Equal(DateTime.UtcNow.RoundToDay(), meta.DateModified.Value.DateTime.RoundToDay());
	}

	[Fact]
	public async Task GetBlob_doesnt_exist_returns_null() {
		string id = RandomBlobPath();

		StoreObject meta = (await _storage.GetObjectsInfo(new[] { id })).First();

		Assert.Null(meta);
	}

	[Fact]
	public async Task GetBlob_Root_doesnt_exist_returns_null() {
		string id = "/" + Guid.NewGuid().ToString();
		//string id = "test";

		Assert.Null(await _storage.GetObjectInfo(id));
	}

	[Fact]
	public async Task GetBlob_Root_valid_returns_some() {
		string id = RandomBlobPath();

		string root = StoragePath.Split(id)[0];

		try {
			StoreObject rb = await _storage.GetObjectInfo(root);
		}
		catch (NotSupportedException) {

		}
	}


	[Fact]
	public async Task Open_doesnt_exist_returns_null() {
		string id = RandomBlobPath();

		Assert.Null(await _storage.OpenRead(id));
	}

	[Fact]
	public async Task Open_blob_exists_returns_stream() {
		string existingBlobPath = $"{Guid.NewGuid()}/existing-blob.txt";

		await _storage.SetText(existingBlobPath, "Hello, Blob!");

		var result = await _storage.OpenRead(existingBlobPath);
		Assert.NotNull(result);

		using var reader = new StreamReader(result);
		string content = await reader.ReadToEndAsync();
		Assert.Equal("Hello, Blob!", content);
	}


	[Fact]
	public async Task Open_empty_blob_returns_empty_stream() {
		string emptyBlobPath = $"{Guid.NewGuid()}/empty-blob.txt";

		await _storage.SetObject(emptyBlobPath, new MemoryStream(new byte[0]));
		Stream result = await _storage.OpenRead(emptyBlobPath);

		Assert.NotNull(result);
		Assert.Equal(0, result.Length);
	}

	[Fact]
	public async Task Open_copy_to_memory_stream_succeeds() {
		string id = await GetRandomStreamIdAsync();
		IStore ms = StorageFactory.InMemory();

		//if this doesn't crash it means the returned stream is compatible with usual .net streaming
		await _storage.CopyObjectTo(id, ms, id);
	}

	[Fact]
	public async Task Write_with_writeasync_succeeds() {
		string id = RandomBlobPath();
		byte[] data = Encoding.UTF8.GetBytes("oh my");

		await _storage.SetObject(id, new MemoryStream(data));

		//read and check
		string result = await _storage.GetText(id);
		Assert.Equal("oh my", result);

		// length
		var len = await _storage.GetObjectLength(id);
		Assert.Equal(data.Length, len);
	}

	[Fact]
	public async Task Write_nullDataStream_argumentnullexception() {
		await Assert.ThrowsAsync<ArgumentNullException>(() => _storage.SetObject(RandomBlobPath(), (Stream)null, false));
	}

	[Fact]
	public async Task Write_non_seekable_stream_succeeds() {
		string s = "test content";
		string id = RandomBlobPath();
		var data = Encoding.UTF8.GetBytes(s);

		var nonSeekable = new NonSeekableStream(new MemoryStream(data));

		await _storage.SetObject(id, nonSeekable);

		// check content
		Assert.Equal(s, await _storage.GetText(id));

		// check length
		var len = await _storage.GetObjectLength(id);
		Assert.Equal(data.Length, len);

	}

	[Fact]
	public async Task ObjectExists_non_existing_blob_returns_false() {
		Assert.False(await _storage.ObjectExists(RandomBlobPath()));
	}

	[Fact]
	public async Task ObjectExists_existing_blob_returns_true() {
		string id = RandomBlobPath();
		await _storage.SetText(id, "test");

		Assert.True(await _storage.ObjectExists(id));
	}

	[Fact]
	public async Task Delete_create_and_delete_doesnt_exist() {
		string path = RandomBlobPath();
		await _storage.SetText(path, "test");
		await _storage.DeleteObject(path);

		Assert.False(await _storage.ObjectExists(path));
	}

	[Fact]
	public async Task Delete_non_existing_file_ignores() {
		string path = RandomBlobPath();
		await _storage.DeleteObject(path);
	}

	[Fact]
	public async Task Delete_folder_removes_everything() {
		//setup
		string prefix = RandomBlobPath();
		string file1 = StoragePath.Combine(prefix, "1.txt");
		string file2 = StoragePath.Combine(prefix, "sub", "2.txt");

		//setup
		await _storage.SetText(file1, "1");
		await _storage.SetText(file2, "2");

		//act
		await _storage.DeleteDirectory(prefix, true);

		//assert
		List<StoreObject> files = await _storage.ListDirectory(prefix, true);
		Assert.True(files.Count == 0);
	}

	[Fact]
	public async Task MoveObject_File_Renames() {
		string prefix = RandomBlobPath();
		string file = StoragePath.Combine(prefix, "1");

		try {
			await _storage.SetText(file, "test");
			await _storage.MoveObject(file, StoragePath.Combine(prefix, "2"), true);
			List<StoreObject> list = await _storage.ListDirectory(prefix);

			Assert.Single(list);
			Assert.True(list.First().Name == "2");
		}
		catch (NotSupportedException) {

		}
	}

	[Fact]
	public async Task MoveObject_OldPathNull_ThowsArgumentNull() {
		await Assert.ThrowsAsync<ArgumentNullException>(() => _storage.MoveObject(null, "test/1", true));
	}

	[Fact]
	public async Task MoveObject_NewPathNull_ThowsArgumentNull() {
		await Assert.ThrowsAsync<ArgumentNullException>(() => _storage.MoveObject("test/1", null, true));
	}


	[Fact]
	public async Task MoveObject_Folder_Renames() {
		string prefix = RandomBlobPath();
		string file1 = StoragePath.Combine(prefix, "old", "1.txt");
		string file11 = StoragePath.Combine(prefix, "old", "1", "1.txt");
		string file111 = StoragePath.Combine(prefix, "old", "1", "1", "1.txt");

		try {
			await _storage.SetText(file1, string.Empty);
		}
		catch (NotSupportedException) {
			return;
		}

		await _storage.SetText(file11, string.Empty);
		await _storage.SetText(file111, string.Empty);

		await _storage.MoveObject(StoragePath.Combine(prefix, "old"), StoragePath.Combine(prefix, "new"), true);

		List<StoreObject> list = await _storage.ListDirectory(prefix);
	}

	[Fact]
	public async Task Read_larger_file() {
		string text = RandomGenerator.GetRandomString(1024 * 1024, false);

		try {
			await _storage.SetText("test/test", text);

			string text2 = await _storage.GetText("test/test");

			Assert.Equal(text, text2);
		}
		catch (NotSupportedException) {

		}
	}

	[Fact]
	public async Task UserMetadata_write_readsback() {
		var blob = new StoreObject(RandomBlobPath());
		blob.Metadata["user"] = "ivan";
		blob.Metadata["fun"] = "no";

		await _storage.SetText(blob, "test");
		StoreObject blob2 = await _storage.GetObjectInfo(blob);

		try {
			await _storage.SetObjectInfo(blob);
			blob2 = await _storage.GetObjectInfo(blob);
			Assert.True(blob2.Size > 0);
		}
		catch (NotSupportedException) {
			return;
		}

		//test
		blob2 = await _storage.GetObjectInfo(blob);
		Assert.NotNull(blob2.Metadata);
		Assert.Equal("ivan", blob2.Metadata["user"]);
		Assert.Equal("no", blob2.Metadata["fun"]);
		Assert.Equal(2, blob2.Metadata.Count);
	}

	[Fact]
	public async Task UserMetadata_OverwriteWithLess_RemovesOld() {
		//setup
		var blob = new StoreObject(RandomBlobPath());
		blob.Metadata["user"] = "ivan";
		blob.Metadata["fun"] = "no";
		await _storage.SetText(blob, "test");
		try {
			await _storage.SetObjectInfo(blob);
		}
		catch (NotSupportedException) {
			return;
		}
		blob.Metadata.Clear();
		blob.Metadata["user"] = "ivan2";
		await _storage.SetText(blob, "test2");
		await _storage.SetObjectInfo(blob);

		//test
		StoreObject blob2 = await _storage.GetObjectInfo(blob);
		Assert.NotNull(blob2.Metadata);
		Assert.Single(blob2.Metadata);
		Assert.Equal("ivan2", blob2.Metadata["user"]);
	}

	[Fact]
	public async Task UserMetadata_openwrite_readsback() {
		var blob = new StoreObject(RandomBlobPath());
		blob.Metadata["user"] = "ivan";
		blob.Metadata["fun"] = "no";

		await _storage.SetObject(blob, new MemoryStream(RandomGenerator.GetRandomBytes(10, 15)));

		try {
			await _storage.SetObjectInfo(blob);
		}
		catch (NotSupportedException) {
			return;
		}

		//test
		StoreObject blob2 = await _storage.GetObjectInfo(blob);
		Assert.NotNull(blob2.Metadata);
		Assert.Equal("ivan", blob2.Metadata["user"]);
		Assert.Equal("no", blob2.Metadata["fun"]);
		Assert.Equal(2, blob2.Metadata.Count);
	}

	[Fact]
	public async Task UserMetadata_List_AlsoReturnsMetadata() {
		var blob = new StoreObject(RandomBlobPath());
		blob.Metadata["user"] = "ivan";
		blob.Metadata["fun"] = "no";
		await _storage.SetText(blob, "test2");

		try {
			await _storage.SetObjectInfo(blob);
		}
		catch (NotSupportedException) {
			return;
		}

		List<StoreObject> all = await _storage.ListDirectory(folderPath: blob.FolderPath, includeAttributes: true);

		//test
		StoreObject blob2 = all.First(b => b.FullPath == blob.FullPath);
		Assert.NotNull(blob2.Metadata);
		Assert.Equal("ivan", blob2.Metadata["user"]);
		Assert.Equal("no", blob2.Metadata["fun"]);
		Assert.Equal(2, blob2.Metadata.Count);
	}

	[Fact]
	public async Task Hierarchy_CreateFolder_Exists() {
		string folderPath = RandomBlobPath();

		try {
			await _storage.CreateDirectory(folderPath, false);

			Assert.True(await _storage.DirectoryExists(folderPath));
		}
		catch (NotSupportedException) {

		}
	}

	private string RandomBlobPath(string prefix = null, string subfolder = null, string extension = "") {
		return StoragePath.Combine(
			_blobPrefix,
			subfolder,
			(prefix ?? "") + Guid.NewGuid().ToString() + extension);
	}

	class TestDocument {
		public string M { get; set; }
	}


}
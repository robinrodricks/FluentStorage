namespace FluentStorage.Tests.Integration.Storage.TestSuite;

public partial class IStoreTest {


	// ---------------------------------------------------------------------
	// ListObjects
	// ---------------------------------------------------------------------

	[Fact]
	public async Task ListObjects_ReturnsUploadedObject() {
		string file = RandomFile();

		await CreateText(file);

		var list = await _storage.ListObjects();

		Assert.Contains(list, x => x.FullPath == file);
	}

	[Fact]
	public async Task ListObjects_ReturnsMultipleUploadedObjects() {

		// not for FS
		if (await _storage.IsFileSystem()) return;


		string f1 = RandomFile();
		string f2 = RandomFile();
		string f3 = RandomFile();

		await CreateText(f1);
		await CreateText(f2);
		await CreateText(f3);

		var list = await _storage.ListObjects(new StorageListOptions { Recurse = true });

		Assert.Contains(list, x => x.FullPath == f1);
		Assert.Contains(list, x => x.FullPath == f2);
		Assert.Contains(list, x => x.FullPath == f3);
	}

	[Fact]
	public async Task ListObjects_AfterDelete_DoesNotContainDeletedObject() {
		string file = RandomFile();

		await CreateText(file);

		await _storage.DeleteObject(file);

		var list = await _storage.ListObjects();

		Assert.DoesNotContain(list, x => x.FullPath == file);
	}

	// ---------------------------------------------------------------------
	// ListDirectory(folder, recurse)
	// ---------------------------------------------------------------------

	[Fact]
	public async Task ListDirectory_EmptyFolder_ReturnsEmpty() {
		string folder = RandomFolder();

		var list = await _storage.ListDirectory(folder, false);

		Assert.NotNull(list);
		Assert.Empty(list);
	}

	[Fact]
	public async Task ListDirectory_ReturnsDirectChildren() {
		string folder = RandomFolder();

		await CreateText($"{folder}/a.txt");
		await CreateText($"{folder}/b.txt");

		var list = await _storage.ListDirectory(folder, false);

		Assert.Equal(2, list.Count);

		Assert.Contains(list, x => x.Name == "a.txt");
		Assert.Contains(list, x => x.Name == "b.txt");
	}

	[Fact]
	public async Task ListDirectory_NonRecursive_DoesNotReturnNestedFiles() {
		string folder = RandomFolder();

		await CreateText($"{folder}/root.txt");
		await CreateText($"{folder}/child/file.txt");

		var list = await _storage.ListDirectory(folder, false);

		Assert.Contains(list, x => x.Name == "root.txt");
		Assert.DoesNotContain(list, x => x.Name == "file.txt");
	}

	[Fact]
	public async Task ListDirectory_Recursive_ReturnsNestedFiles() {
		string folder = RandomFolder();

		await CreateText($"{folder}/root.txt");
		await CreateText($"{folder}/child/file.txt");
		await CreateText($"{folder}/child/sub/file2.txt");

		var list = (await _storage.ListDirectory(folder, true)).Where(f => f.Type == StorageObjectType.File).ToList();

		Assert.Equal(3, list.Count);

		Assert.Contains(list, x => x.Name == "root.txt");
		Assert.Contains(list, x => x.Name == "file.txt");
		Assert.Contains(list, x => x.Name == "file2.txt");
	}

	// ---------------------------------------------------------------------
	// ListDirectory(options)
	// ---------------------------------------------------------------------

	[Fact]
	public async Task ListDirectory_FilePrefix_ReturnsOnlyMatchingFiles() {
		string folder = RandomFolder();

		await CreateText($"{folder}/apple.txt");
		await CreateText($"{folder}/apricot.txt");
		await CreateText($"{folder}/banana.txt");

		var list = await _storage.ListDirectory(
			folderPath: folder,
			filePrefix: "ap",
			recurse: false);

		Assert.Equal(2, list.Count);

		Assert.All(list, x => Assert.StartsWith("ap", x.Name));
	}

	[Fact]
	public async Task ListDirectory_BrowseFilter_IsApplied() {
		string folder = RandomFolder();

		await CreateText($"{folder}/one.txt");
		await CreateText($"{folder}/two.txt");
		await CreateText($"{folder}/three.bin");

		var list = (await _storage.ListDirectory(folderPath: folder,
				recurse: false,
				browseFilter: x => x.Name.EndsWith(".txt")))
			.Where(f => f.Type == StorageObjectType.File).ToList();

		Assert.Equal(2, list.Count);

		Assert.All(list, x => Assert.EndsWith(".txt", x.Name));
	}

	[Fact]
	public async Task ListDirectory_MaxResults_IsRespected() {
		string folder = RandomFolder();

		for (int i = 0; i < 20; i++)
			await CreateText($"{folder}/{i}.txt");

		var list = await _storage.ListDirectory(
			folderPath: folder,
			recurse: true,
			maxResults: 5);

		Assert.True(list.Count <= 5);
	}

	[Fact]
	public async Task ListDirectory_FolderThatDoesNotExist_ReturnsEmpty() {
		var list = await _storage.ListDirectory(
			$"tests/{Guid.NewGuid():N}",
			true);

		Assert.NotNull(list);
		Assert.Empty(list);
	}


}
namespace FluentStorage.Tests.Integration.Storage.TestSuite;

public partial class IStoreTest {



	// ---------------------------------------------------------------------
	// UploadDirectory
	// ---------------------------------------------------------------------

	[Fact]
	public async Task UploadDirectory_EmptyFolder_UploadsNothing() {
		string local = RandomLocalFolder();
		string remote = RandomRemoteFolder();

		await UploadTree(Array.Empty<LocalFile>(), local, remote);

		await AssertRemoteContainsExactly(remote, 0);
	}

	[Fact]
	public async Task UploadDirectory_SingleFile() {
		string local = RandomLocalFolder();
		string remote = RandomRemoteFolder();

		var tree = new[] { new LocalFile("hello.txt", 123) };

		await UploadTree(tree, local, remote);

		await AssertRemoteTree(remote, tree);
	}

	[Fact]
	public async Task UploadDirectory_MultipleFiles() {
		string local = RandomLocalFolder();
		string remote = RandomRemoteFolder();

		await UploadTree(SmallTree, local, remote);

		await AssertRemoteTree(remote, SmallTree);
	}

	[Fact]
	public async Task UploadDirectory_RecursiveDirectories() {
		string local = RandomLocalFolder();
		string remote = RandomRemoteFolder();

		await UploadTree(DeepTree, local, remote);

		await AssertRemoteDirectoriesExist(remote, DeepTree);
		await AssertRemoteTree(remote, DeepTree);
	}

	[Fact]
	public async Task UploadDirectory_PreservesRelativePaths() {
		string local = RandomLocalFolder();
		string remote = RandomRemoteFolder();

		await UploadTree(SmallTree, local, remote);

		foreach (var file in SmallTree)
			Assert.True(await _storage.ObjectExists($"{remote}/{file.RelativePath.Replace('\\', '/')}"));
	}

	[Fact]
	public async Task UploadDirectory_UploadsUnicodeNames() {
		string local = RandomLocalFolder();
		string remote = RandomRemoteFolder();

		await UploadTree(UnicodeTree, local, remote);

		await AssertRemoteTree(remote, UnicodeTree);
	}

	[Fact]
	public async Task UploadDirectory_UploadsEmptyFiles() {
		string local = RandomLocalFolder();
		string remote = RandomRemoteFolder();

		await UploadTree(EmptyFilesTree, local, remote);

		await AssertRemoteTree(remote, EmptyFilesTree);
	}

	[Fact]
	public async Task UploadDirectory_UploadsLargeFiles() {
		string local = RandomLocalFolder();
		string remote = RandomRemoteFolder();

		await UploadTree(LargeTree, local, remote);

		await AssertRemoteTree(remote, LargeTree);
	}

	[Fact]
	public async Task UploadDirectory_ReportsProgressPerFile() {
		string local = RandomLocalFolder();
		string remote = RandomRemoteFolder();

		var recorder = new ProgressRecorder();

		await UploadTree(SmallTree, local, remote, StorageExists.Skip, recorder);

		Assert.Equal(SmallTree.Length, recorder.Count);
		Assert.Equal(SmallTree.Length, recorder.SuccessCount);
		Assert.Equal(0, recorder.FailureCount);
	}

	[Fact]
	public async Task UploadDirectory_DoesNotCreateExtraFiles() {
		string local = RandomLocalFolder();
		string remote = RandomRemoteFolder();

		await UploadTree(SmallTree, local, remote);

		await AssertRemoteContainsExactly(remote, SmallTree.Length);
	}

	[Fact]
	public async Task UploadDirectory_SkipExisting_DoesNotOverwrite() {
		string local = RandomLocalFolder();
		string remote = RandomRemoteFolder();

		CreateLocalTree(local, new[] { new LocalFile("a.txt", 100) });

		await _storage.UploadDirectory(local, remote, StorageExists.Skip);

		long originalLength = await _storage.GetObjectLength($"{remote}/a.txt");

		File.WriteAllBytes(Path.Combine(local, "a.txt"), new byte[500]);

		await _storage.UploadDirectory(local, remote, StorageExists.Skip);

		Assert.Equal(originalLength, await _storage.GetObjectLength($"{remote}/a.txt"));
	}

	[Fact]
	public async Task UploadDirectory_Overwrite_ReplacesExistingFiles() {
		string local = RandomLocalFolder();
		string remote = RandomRemoteFolder();

		CreateLocalTree(local, new[] { new LocalFile("a.txt", 100) });

		await _storage.UploadDirectory(local, remote, StorageExists.Skip);

		File.WriteAllBytes(Path.Combine(local, "a.txt"), new byte[700]);

		await _storage.UploadDirectory(local, remote, StorageExists.Overwrite);

		Assert.Equal(700, await _storage.GetObjectLength($"{remote}/a.txt"));
	}

	[Fact]
	public async Task UploadDirectory_CreatesRemoteDirectories() {
		string local = RandomLocalFolder();
		string remote = RandomRemoteFolder();

		await UploadTree(DeepTree, local, remote);

		await AssertRemoteDirectoriesExist(remote, DeepTree);
	}

	[Fact]
	public async Task UploadDirectory_AllUploadedFilesExist() {
		string local = RandomLocalFolder();
		string remote = RandomRemoteFolder();

		await UploadTree(SmallTree, local, remote);

		foreach (var file in SmallTree)
			Assert.True(await _storage.ObjectExists($"{remote}/{file.RelativePath.Replace('\\', '/')}"));
	}

	[Fact]
	public async Task UploadDirectory_AllUploadedFilesHaveCorrectLength() {
		string local = RandomLocalFolder();
		string remote = RandomRemoteFolder();

		await UploadTree(SmallTree, local, remote);

		foreach (var file in SmallTree)
			Assert.Equal(file.Size, await _storage.GetObjectLength($"{remote}/{file.RelativePath.Replace('\\', '/')}"));
	}

	[Fact]
	public async Task UploadDirectory_TwiceWithSkip_DoesNotDuplicateFiles() {
		string local = RandomLocalFolder();
		string remote = RandomRemoteFolder();

		await UploadTree(SmallTree, local, remote);
		await _storage.UploadDirectory(local, remote, StorageExists.Skip);

		await AssertRemoteContainsExactly(remote, SmallTree.Length);
	}

	[Fact]
	public async Task UploadDirectory_TwiceWithOverwrite_DoesNotDuplicateFiles() {
		string local = RandomLocalFolder();
		string remote = RandomRemoteFolder();

		await UploadTree(SmallTree, local, remote);
		await _storage.UploadDirectory(local, remote, StorageExists.Overwrite);

		await AssertRemoteContainsExactly(remote, SmallTree.Length);
	}

	[Fact]
	public async Task UploadDirectory_MixedFileSizes() {
		string local = RandomLocalFolder();
		string remote = RandomRemoteFolder();

		var tree = new[]{
			new LocalFile("0.bin", 0),
			new LocalFile("1.bin", 1),
			new LocalFile("10.bin", 10),
			new LocalFile("100.bin", 100),
			new LocalFile("1000.bin", 1000),
			new LocalFile("10000.bin", 10000)
		};

		await UploadTree(tree, local, remote);

		await AssertRemoteTree(remote, tree);
	}

	[Fact]
	public async Task UploadDirectory_DeepHierarchy_FileCountMatches() {
		string local = RandomLocalFolder();
		string remote = RandomRemoteFolder();

		await UploadTree(DeepTree, local, remote);

		await AssertRemoteContainsExactly(remote, DeepTree.Length);
	}

}
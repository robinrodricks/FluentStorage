namespace FluentStorage.Tests.Integration.Storage.TestSuite;

public partial class IStoreTest {



	// ---------------------------------------------------------------------
	// Upload + Download Round Trip
	// ---------------------------------------------------------------------

	[Fact]
	public async Task UploadThenDownload_SingleFile_RoundTripsCorrectly() {
		string upload = RandomLocalFolder();
		string download = RandomLocalFolder();
		string remote = RandomRemoteFolder();

		var tree = new[] { new LocalFile("hello.txt", 123) };

		await UploadTree(tree, upload, remote);
		await DownloadTree(remote, download);

		AssertLocalTree(download, tree);
	}

	[Fact]
	public async Task UploadThenDownload_MultipleFiles_RoundTripsCorrectly() {
		string upload = RandomLocalFolder();
		string download = RandomLocalFolder();
		string remote = RandomRemoteFolder();

		await UploadTree(SmallTree, upload, remote);
		await DownloadTree(remote, download);

		AssertLocalTree(download, SmallTree);
	}

	[Fact]
	public async Task UploadThenDownload_DeepTree_RoundTripsCorrectly() {
		string upload = RandomLocalFolder();
		string download = RandomLocalFolder();
		string remote = RandomRemoteFolder();

		await UploadTree(DeepTree, upload, remote);
		await DownloadTree(remote, download);

		AssertLocalTree(download, DeepTree);
	}

	[Fact]
	public async Task UploadThenDownload_UnicodeTree_RoundTripsCorrectly() {
		string upload = RandomLocalFolder();
		string download = RandomLocalFolder();
		string remote = RandomRemoteFolder();

		await UploadTree(UnicodeTree, upload, remote);
		await DownloadTree(remote, download);

		AssertLocalTree(download, UnicodeTree);
	}

	[Fact]
	public async Task UploadThenDownload_EmptyFiles_RoundTripsCorrectly() {
		string upload = RandomLocalFolder();
		string download = RandomLocalFolder();
		string remote = RandomRemoteFolder();

		await UploadTree(EmptyFilesTree, upload, remote);
		await DownloadTree(remote, download);

		AssertLocalTree(download, EmptyFilesTree);
	}

	[Fact]
	public async Task UploadThenDownload_LargeFiles_RoundTripsCorrectly() {
		string upload = RandomLocalFolder();
		string download = RandomLocalFolder();
		string remote = RandomRemoteFolder();

		await UploadTree(LargeTree, upload, remote);
		await DownloadTree(remote, download);

		AssertLocalTree(download, LargeTree);
	}

	[Fact]
	public async Task UploadThenDownload_FileCountMatches() {
		string upload = RandomLocalFolder();
		string download = RandomLocalFolder();
		string remote = RandomRemoteFolder();

		await UploadTree(SmallTree, upload, remote);
		await DownloadTree(remote, download);

		Assert.Equal(SmallTree.Length, Directory.GetFiles(download, "*", SearchOption.AllDirectories).Length);
	}

	[Fact]
	public async Task UploadThenDownload_NoExtraFilesExist() {
		string upload = RandomLocalFolder();
		string download = RandomLocalFolder();
		string remote = RandomRemoteFolder();

		await UploadTree(DeepTree, upload, remote);
		await DownloadTree(remote, download);

		foreach (string file in Directory.GetFiles(download, "*", SearchOption.AllDirectories)) {
			string relative = Path.GetRelativePath(download, file).Replace('\\', '/');
			Assert.Contains(DeepTree, x => x.RelativePath == relative);
		}
	}

	[Fact]
	public async Task UploadThenDownload_RemoteFileLengthsRemainCorrect() {
		string upload = RandomLocalFolder();
		string remote = RandomRemoteFolder();

		await UploadTree(SmallTree, upload, remote);

		foreach (var file in SmallTree)
			Assert.Equal(file.Size, await _storage.GetObjectLength($"{remote}/{file.RelativePath}"));
	}

	[Fact]
	public async Task UploadThenDownload_LocalFileLengthsRemainCorrect() {
		string upload = RandomLocalFolder();
		string download = RandomLocalFolder();
		string remote = RandomRemoteFolder();

		await UploadTree(DeepTree, upload, remote);
		await DownloadTree(remote, download);

		foreach (var file in DeepTree)
			Assert.Equal(file.Size, new FileInfo(Path.Combine(download, file.RelativePath.Replace('/', Path.DirectorySeparatorChar))).Length);
	}

	[Fact]
	public async Task UploadThenDownload_AllRemoteFilesExist() {
		string upload = RandomLocalFolder();
		string remote = RandomRemoteFolder();

		await UploadTree(SmallTree, upload, remote);

		foreach (var file in SmallTree)
			Assert.True(await _storage.ObjectExists($"{remote}/{file.RelativePath}"));
	}

	[Fact]
	public async Task UploadThenDownload_AllLocalFilesExist() {
		string upload = RandomLocalFolder();
		string download = RandomLocalFolder();
		string remote = RandomRemoteFolder();

		await UploadTree(SmallTree, upload, remote);
		await DownloadTree(remote, download);

		foreach (var file in SmallTree)
			Assert.True(File.Exists(Path.Combine(download, file.RelativePath.Replace('/', Path.DirectorySeparatorChar))));
	}

	// ---------------------------------------------------------------------
	// Cross Validation / Integrity
	// ---------------------------------------------------------------------

	[Fact]
	public async Task UploadDirectory_RemoteAndLocalFileCountsMatch() {
		string local = RandomLocalFolder();
		string remote = RandomRemoteFolder();

		await UploadTree(DeepTree, local, remote);

		int localCount = Directory.GetFiles(local, "*", SearchOption.AllDirectories).Length;
		int remoteCount = (await _storage.ListDirectory(remote, true)).Count(x => x.IsFile);

		Assert.Equal(localCount, remoteCount);
	}

	[Fact]
	public async Task DownloadDirectory_RemoteAndLocalFileCountsMatch() {
		string upload = RandomLocalFolder();
		string download = RandomLocalFolder();
		string remote = RandomRemoteFolder();

		await UploadTree(DeepTree, upload, remote);
		await DownloadTree(remote, download);

		int localCount = Directory.GetFiles(download, "*", SearchOption.AllDirectories).Length;
		int remoteCount = (await _storage.ListDirectory(remote, true)).Count(x => x.IsFile);

		Assert.Equal(remoteCount, localCount);
	}

	[Fact]
	public async Task UploadDirectory_RemoteAndLocalLengthsMatch() {
		string local = RandomLocalFolder();
		string remote = RandomRemoteFolder();

		await UploadTree(SmallTree, local, remote);

		foreach (string file in Directory.GetFiles(local, "*", SearchOption.AllDirectories)) {
			string relative = Path.GetRelativePath(local, file).Replace('\\', '/');
			long localLength = new FileInfo(file).Length;
			long remoteLength = await _storage.GetObjectLength($"{remote}/{relative}");

			Assert.Equal(localLength, remoteLength);
		}
	}

	[Fact]
	public async Task DownloadDirectory_RemoteAndLocalLengthsMatch() {
		string upload = RandomLocalFolder();
		string download = RandomLocalFolder();
		string remote = RandomRemoteFolder();

		await UploadTree(SmallTree, upload, remote);
		await DownloadTree(remote, download);

		foreach (var file in SmallTree) {
			long localLength = new FileInfo(Path.Combine(download, file.RelativePath.Replace('/', Path.DirectorySeparatorChar))).Length;
			long remoteLength = await _storage.GetObjectLength($"{remote}/{file.RelativePath}");

			Assert.Equal(remoteLength, localLength);
		}
	}

	[Fact]
	public async Task UploadDirectory_NoUnexpectedRemoteFilesExist() {
		string local = RandomLocalFolder();
		string remote = RandomRemoteFolder();

		await UploadTree(SmallTree, local, remote);

		var remoteFiles = (await _storage.ListDirectory(remote, true)).Where(x => x.IsFile).Select(x => x.FullPath.Replace($"{remote}/", "")).OrderBy(x => x).ToArray();
		var expected = SmallTree.Select(x => x.RelativePath.Replace('\\', '/')).OrderBy(x => x).ToArray();

		Assert.Equal(expected, remoteFiles);
	}

	[Fact]
	public async Task DownloadDirectory_NoUnexpectedLocalFilesExist() {
		string upload = RandomLocalFolder();
		string download = RandomLocalFolder();
		string remote = RandomRemoteFolder();

		await UploadTree(SmallTree, upload, remote);
		await DownloadTree(remote, download);

		var localFiles = Directory.GetFiles(download, "*", SearchOption.AllDirectories).Select(x => Path.GetRelativePath(download, x).Replace('\\', '/')).OrderBy(x => x).ToArray();
		var expected = SmallTree.Select(x => x.RelativePath.Replace('\\', '/')).OrderBy(x => x).ToArray();

		Assert.Equal(expected, localFiles);
	}

	[Fact]
	public async Task UploadDirectory_DirectoryStructureMatchesExpected() {
		string local = RandomLocalFolder();
		string remote = RandomRemoteFolder();

		await UploadTree(DeepTree, local, remote);

		var expectedFolders = DeepTree.Select(x => Path.GetDirectoryName(x.RelativePath)?.Replace('\\', '/')).Where(x => !string.IsNullOrWhiteSpace(x)).Distinct().OrderBy(x => x).ToArray();

		foreach (string folder in expectedFolders)
			Assert.True(await _storage.DirectoryExists($"{remote}/{folder}"));
	}

	[Fact]
	public async Task DownloadDirectory_DirectoryStructureMatchesExpected() {
		string upload = RandomLocalFolder();
		string download = RandomLocalFolder();
		string remote = RandomRemoteFolder();

		await UploadTree(DeepTree, upload, remote);
		await DownloadTree(remote, download);

		var expectedFolders = DeepTree.Select(x => Path.GetDirectoryName(x.RelativePath)?.Replace('/', Path.DirectorySeparatorChar)).Where(x => !string.IsNullOrWhiteSpace(x)).Distinct().OrderBy(x => x).ToArray();

		foreach (string folder in expectedFolders)
			Assert.True(Directory.Exists(Path.Combine(download, folder)));
	}

	// ---------------------------------------------------------------------
	// Stress / Edge Cases
	// ---------------------------------------------------------------------

	[Fact]
	public async Task UploadDirectory_HundredFiles_UploadsEverything() {
		string local = RandomLocalFolder();
		string remote = RandomRemoteFolder();

		var tree = Enumerable.Range(1, 100).Select(i => new LocalFile($"file{i}.txt", i)).ToArray();

		await UploadTree(tree, local, remote);

		await AssertRemoteTree(remote, tree);
	}

	[Fact]
	public async Task DownloadDirectory_HundredFiles_DownloadsEverything() {
		string upload = RandomLocalFolder();
		string download = RandomLocalFolder();
		string remote = RandomRemoteFolder();

		var tree = Enumerable.Range(1, 100).Select(i => new LocalFile($"file{i}.txt", i)).ToArray();

		await UploadTree(tree, upload, remote);
		await DownloadTree(remote, download);

		AssertLocalTree(download, tree);
	}

	[Fact]
	public async Task UploadDirectory_ManyNestedFolders() {
		string local = RandomLocalFolder();
		string remote = RandomRemoteFolder();

		var tree = Enumerable.Range(1, 20).Select(i => new LocalFile($"{string.Join("/", Enumerable.Repeat("folder", i))}/file.txt", i)).ToArray();

		await UploadTree(tree, local, remote);

		await AssertRemoteTree(remote, tree);
	}

	[Fact]
	public async Task DownloadDirectory_ManyNestedFolders() {
		string upload = RandomLocalFolder();
		string download = RandomLocalFolder();
		string remote = RandomRemoteFolder();

		var tree = Enumerable.Range(1, 20).Select(i => new LocalFile($"{string.Join("/", Enumerable.Repeat("folder", i))}/file.txt", i)).ToArray();

		await UploadTree(tree, upload, remote);
		await DownloadTree(remote, download);

		AssertLocalTree(download, tree);
	}

	[Fact]
	public async Task UploadDirectory_ProgressCountMatchesFileCount() {
		string local = RandomLocalFolder();
		string remote = RandomRemoteFolder();

		var tree = Enumerable.Range(1, 50).Select(i => new LocalFile($"file{i}.txt", i)).ToArray();

		var recorder = new ProgressRecorder();

		await UploadTree(tree, local, remote, StorageExists.Skip, recorder);

		Assert.Equal(tree.Length, recorder.Count);
	}

	[Fact]
	public async Task DownloadDirectory_ProgressCountMatchesFileCount() {
		string upload = RandomLocalFolder();
		string download = RandomLocalFolder();
		string remote = RandomRemoteFolder();

		var tree = Enumerable.Range(1, 50).Select(i => new LocalFile($"file{i}.txt", i)).ToArray();

		await UploadTree(tree, upload, remote);

		var recorder = new ProgressRecorder();

		await DownloadTree(remote, download, StorageExists.Skip, recorder);

		Assert.Equal(tree.Length, recorder.Count);
	}

	[Fact]
	public async Task UploadDownload_Twice_RemainsConsistent() {
		string upload = RandomLocalFolder();
		string download1 = RandomLocalFolder();
		string download2 = RandomLocalFolder();
		string remote = RandomRemoteFolder();

		await UploadTree(SmallTree, upload, remote);

		await DownloadTree(remote, download1);
		await DownloadTree(remote, download2);

		AssertLocalTree(download1, SmallTree);
		AssertLocalTree(download2, SmallTree);
	}

	[Fact]
	public async Task UploadDirectory_RootFilesOnly() {
		string local = RandomLocalFolder();
		string remote = RandomRemoteFolder();

		var tree = new[]{new LocalFile("a.txt", 10),new LocalFile("b.txt", 20),new LocalFile("c.txt", 30)};

		await UploadTree(tree, local, remote);

		await AssertRemoteTree(remote, tree);
	}

	[Fact]
	public async Task DownloadDirectory_RootFilesOnly() {
		string upload = RandomLocalFolder();
		string download = RandomLocalFolder();
		string remote = RandomRemoteFolder();

		var tree = new[]{new LocalFile("a.txt", 10),new LocalFile("b.txt", 20),new LocalFile("c.txt", 30)};

		await UploadTree(tree, upload, remote);
		await DownloadTree(remote, download);

		AssertLocalTree(download, tree);
	}

	[Fact]
	public async Task UploadDirectory_AllRemoteLengthsMatchSourceLengths() {
		string local = RandomLocalFolder();
		string remote = RandomRemoteFolder();

		await UploadTree(DeepTree, local, remote);

		foreach (var file in DeepTree)
			Assert.Equal(file.Size, await _storage.GetObjectLength($"{remote}/{file.RelativePath}"));
	}

	[Fact]
	public async Task DownloadDirectory_AllDownloadedLengthsMatchRemoteLengths() {
		string upload = RandomLocalFolder();
		string download = RandomLocalFolder();
		string remote = RandomRemoteFolder();

		await UploadTree(DeepTree, upload, remote);
		await DownloadTree(remote, download);

		foreach (var file in DeepTree)
			Assert.Equal(file.Size, new FileInfo(Path.Combine(download, file.RelativePath.Replace('/', Path.DirectorySeparatorChar))).Length);
	}

	[Fact]
	public async Task UploadDownload_DirectoryStructureRemainsIdentical() {
		string upload = RandomLocalFolder();
		string download = RandomLocalFolder();
		string remote = RandomRemoteFolder();

		await UploadTree(DeepTree, upload, remote);
		await DownloadTree(remote, download);

		foreach (var file in DeepTree)
			Assert.True(File.Exists(Path.Combine(download, file.RelativePath.Replace('/', Path.DirectorySeparatorChar))));
	}

	// ---------------------------------------------------------------------
	// Edge Cases / Failure Handling
	// ---------------------------------------------------------------------

	[Fact]
	public async Task UploadDirectory_CanUploadSameTreeTwice() {
		string local = RandomLocalFolder();
		string remote = RandomRemoteFolder();

		await UploadTree(SmallTree, local, remote);
		await UploadTree(SmallTree, local, remote, StorageExists.Overwrite);

		await AssertRemoteTree(remote, SmallTree);
	}

	[Fact]
	public async Task UploadDirectory_OverwriteAfterLocalModification() {
		string local = RandomLocalFolder();
		string remote = RandomRemoteFolder();

		CreateLocalTree(local, SmallTree);

		await _storage.UploadDirectory(local, remote);

		File.WriteAllBytes(Path.Combine(local, "folder1", "a.txt"), new byte[999]);

		await _storage.UploadDirectory(local, remote, StorageExists.Overwrite);

		Assert.Equal(999, await _storage.GetObjectLength($"{remote}/folder1/a.txt"));
	}

	[Fact]
	public async Task UploadDirectory_SkipAfterLocalModification() {
		string local = RandomLocalFolder();
		string remote = RandomRemoteFolder();

		CreateLocalTree(local, SmallTree);

		await _storage.UploadDirectory(local, remote);

		long originalLength = await _storage.GetObjectLength($"{remote}/folder1/a.txt");

		File.WriteAllBytes(Path.Combine(local, "folder1", "a.txt"), new byte[999]);

		await _storage.UploadDirectory(local, remote, StorageExists.Skip);

		Assert.Equal(originalLength, await _storage.GetObjectLength($"{remote}/folder1/a.txt"));
	}

	[Fact]
	public async Task DownloadDirectory_OverwriteAfterRemoteModification() {
		string upload = RandomLocalFolder();
		string download = RandomLocalFolder();
		string remote = RandomRemoteFolder();

		CreateLocalTree(upload, SmallTree);

		await _storage.UploadDirectory(upload, remote);
		await _storage.DownloadDirectory(remote, download);

		await _storage.SetBytes($"{remote}/folder1/a.txt", new byte[888]);

		await _storage.DownloadDirectory(remote, download, StorageExists.Overwrite);

		Assert.Equal(888, new FileInfo(Path.Combine(download, "folder1", "a.txt")).Length);
	}

	[Fact]
	public async Task DownloadDirectory_SkipAfterRemoteModification() {
		string upload = RandomLocalFolder();
		string download = RandomLocalFolder();
		string remote = RandomRemoteFolder();

		CreateLocalTree(upload, SmallTree);

		await _storage.UploadDirectory(upload, remote);
		await _storage.DownloadDirectory(remote, download);

		long originalLength = new FileInfo(Path.Combine(download, "folder1", "a.txt")).Length;

		await _storage.SetBytes($"{remote}/folder1/a.txt", new byte[888]);

		await _storage.DownloadDirectory(remote, download, StorageExists.Skip);

		Assert.Equal(originalLength, new FileInfo(Path.Combine(download, "folder1", "a.txt")).Length);
	}

	[Fact]
	public async Task UploadDirectory_UploadedFilesAppearInListDirectory() {
		string local = RandomLocalFolder();
		string remote = RandomRemoteFolder();

		await UploadTree(SmallTree, local, remote);

		var list = await _storage.ListDirectory(remote, true);

		foreach (var file in SmallTree)
			Assert.Contains(list, x => x.IsFile && x.FullPath == $"{remote}/{file.RelativePath}");
	}

	[Fact]
	public async Task UploadDirectory_RemoteDirectoryExistsAfterUpload() {
		string local = RandomLocalFolder();
		string remote = RandomRemoteFolder();

		await UploadTree(SmallTree, local, remote);

		Assert.True(await _storage.DirectoryExists(remote));
	}

	[Fact]
	public async Task DownloadDirectory_DownloadIntoExistingEmptyDirectory() {
		string upload = RandomLocalFolder();
		string download = RandomLocalFolder();
		string remote = RandomRemoteFolder();

		await UploadTree(SmallTree, upload, remote);

		Directory.CreateDirectory(download);

		await _storage.DownloadDirectory(remote, download);

		AssertLocalTree(download, SmallTree);
	}

	[Fact]
	public async Task UploadThenDownload_ProducesIdenticalDirectoryTree() {
		string upload = RandomLocalFolder();
		string download = RandomLocalFolder();
		string remote = RandomRemoteFolder();

		await UploadTree(DeepTree, upload, remote);
		await DownloadTree(remote, download);

		var original = Directory.GetFiles(upload, "*", SearchOption.AllDirectories).Select(x => Path.GetRelativePath(upload, x).Replace('\\', '/')).OrderBy(x => x).ToArray();
		var downloaded = Directory.GetFiles(download, "*", SearchOption.AllDirectories).Select(x => Path.GetRelativePath(download, x).Replace('\\', '/')).OrderBy(x => x).ToArray();

		Assert.Equal(original, downloaded);
	}

	[Fact]
	public async Task UploadThenDownload_AllLengthsRemainIdentical() {
		string upload = RandomLocalFolder();
		string download = RandomLocalFolder();
		string remote = RandomRemoteFolder();

		await UploadTree(LargeTree, upload, remote);
		await DownloadTree(remote, download);

		foreach (string file in Directory.GetFiles(upload, "*", SearchOption.AllDirectories)) {
			string relative = Path.GetRelativePath(upload, file);
			long originalLength = new FileInfo(file).Length;
			long downloadedLength = new FileInfo(Path.Combine(download, relative)).Length;

			Assert.Equal(originalLength, downloadedLength);
		}
	}

}
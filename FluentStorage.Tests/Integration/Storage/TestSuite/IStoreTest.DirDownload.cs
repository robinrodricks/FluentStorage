namespace FluentStorage.Tests.Integration.Storage.TestSuite;

public partial class IStoreTest {


	// ---------------------------------------------------------------------
	// DownloadDirectory
	// ---------------------------------------------------------------------

	[Fact]
	public async Task DownloadDirectory_EmptyFolder_DownloadsNothing() {
		string remote = RandomRemoteFolder();
		string local = RandomLocalFolder();

		await DownloadTree(remote, local);

		Assert.Empty(Directory.GetFiles(local, "*", SearchOption.AllDirectories));
	}

	[Fact]
	public async Task DownloadDirectory_SingleFile() {
		string remote = RandomRemoteFolder();
		string upload = RandomLocalFolder();
		string download = RandomLocalFolder();

		var tree = new[] { new LocalFile("hello.txt", 123) };

		await UploadTree(tree, upload, remote);
		await DownloadTree(remote, download);

		AssertLocalTree(download, tree);
	}

	[Fact]
	public async Task DownloadDirectory_MultipleFiles() {
		string remote = RandomRemoteFolder();
		string upload = RandomLocalFolder();
		string download = RandomLocalFolder();

		await UploadTree(SmallTree, upload, remote);
		await DownloadTree(remote, download);

		AssertLocalTree(download, SmallTree);
	}

	[Fact]
	public async Task DownloadDirectory_RecursiveDirectories() {
		string remote = RandomRemoteFolder();
		string upload = RandomLocalFolder();
		string download = RandomLocalFolder();

		await UploadTree(DeepTree, upload, remote);
		await DownloadTree(remote, download);

		AssertLocalTree(download, DeepTree);
	}

	[Fact]
	public async Task DownloadDirectory_PreservesRelativePaths() {
		string remote = RandomRemoteFolder();
		string upload = RandomLocalFolder();
		string download = RandomLocalFolder();

		await UploadTree(SmallTree, upload, remote);
		await DownloadTree(remote, download);

		foreach (var file in SmallTree)
			Assert.True(File.Exists(Path.Combine(download, file.RelativePath.Replace('/', Path.DirectorySeparatorChar))));
	}

	[Fact]
	public async Task DownloadDirectory_DownloadsUnicodeNames() {
		string remote = RandomRemoteFolder();
		string upload = RandomLocalFolder();
		string download = RandomLocalFolder();

		await UploadTree(UnicodeTree, upload, remote);
		await DownloadTree(remote, download);

		AssertLocalTree(download, UnicodeTree);
	}

	[Fact]
	public async Task DownloadDirectory_DownloadsEmptyFiles() {
		string remote = RandomRemoteFolder();
		string upload = RandomLocalFolder();
		string download = RandomLocalFolder();

		await UploadTree(EmptyFilesTree, upload, remote);
		await DownloadTree(remote, download);

		AssertLocalTree(download, EmptyFilesTree);
	}

	[Fact]
	public async Task DownloadDirectory_DownloadsLargeFiles() {
		string remote = RandomRemoteFolder();
		string upload = RandomLocalFolder();
		string download = RandomLocalFolder();

		await UploadTree(LargeTree, upload, remote);
		await DownloadTree(remote, download);

		AssertLocalTree(download, LargeTree);
	}

	[Fact]
	public async Task DownloadDirectory_ReportsProgressPerFile() {
		string remote = RandomRemoteFolder();
		string upload = RandomLocalFolder();
		string download = RandomLocalFolder();

		await UploadTree(SmallTree, upload, remote);

		var recorder = new ProgressRecorder();

		await DownloadTree(remote, download, StorageExists.Skip, recorder);

		Assert.Equal(SmallTree.Length, recorder.Count);
		Assert.Equal(SmallTree.Length, recorder.SuccessCount);
		Assert.Equal(0, recorder.FailureCount);
	}

	[Fact]
	public async Task DownloadDirectory_DoesNotCreateExtraFiles() {
		string remote = RandomRemoteFolder();
		string upload = RandomLocalFolder();
		string download = RandomLocalFolder();

		await UploadTree(SmallTree, upload, remote);
		await DownloadTree(remote, download);

		Assert.Equal(SmallTree.Length, Directory.GetFiles(download, "*", SearchOption.AllDirectories).Length);
	}

	[Fact]
	public async Task DownloadDirectory_SkipExisting_DoesNotOverwrite() {
		string remote = RandomRemoteFolder();
		string upload = RandomLocalFolder();
		string download = RandomLocalFolder();

		await UploadTree(new[] { new LocalFile("a.txt", 100) }, upload, remote);

		File.WriteAllBytes(Path.Combine(download, "a.txt"), new byte[999]);

		await DownloadTree(remote, download, StorageExists.Skip);

		Assert.Equal(999, new FileInfo(Path.Combine(download, "a.txt")).Length);
	}

	[Fact]
	public async Task DownloadDirectory_Overwrite_ReplacesExistingFiles() {
		string remote = RandomRemoteFolder();
		string upload = RandomLocalFolder();
		string download = RandomLocalFolder();

		await UploadTree(new[] { new LocalFile("a.txt", 100) }, upload, remote);

		File.WriteAllBytes(Path.Combine(download, "a.txt"), new byte[999]);

		await DownloadTree(remote, download, StorageExists.Overwrite);

		Assert.Equal(100, new FileInfo(Path.Combine(download, "a.txt")).Length);
	}


}
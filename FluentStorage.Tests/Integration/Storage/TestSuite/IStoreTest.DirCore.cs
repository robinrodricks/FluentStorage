using FluentStorage.Rules;

namespace FluentStorage.Tests.Integration.Storage.TestSuite;

public sealed record LocalFile(string RelativePath, int Size);

public partial class IStoreTest {


	// ---------------------------------------------------------------------
	// Fake Trees
	// ---------------------------------------------------------------------

	private static LocalFile[] SmallTree =
	{
		new("root.txt",10),
		new("folder1/a.txt",25),
		new("folder1/b.txt",100),
		new("folder2/c.txt",500),
		new("folder2/sub/d.txt",1500),
	};

	/// <summary>A single deep chain, five levels nested. Good for testing folder-rule cascading.</summary>
	private static LocalFile[] DeepTree =
	{
		new("a.txt",5),
		new("one/b.txt",15),
		new("one/two/c.txt",25),
		new("one/two/three/d.txt",35),
		new("one/two/three/four/e.txt",45),
		new("one/two/three/four/five/f.txt",55),
	};

	private static LocalFile[] UnicodeTree =
	{
		new("你好.txt",12),
		new("日本語/ファイル.txt",50),
		new("😀/नमस्ते.bin",400),
	};

	private static LocalFile[] EmptyFilesTree =
	{
		new("a.txt",0),
		new("b.txt",0),
		new("folder/c.txt",0),
	};

	private static LocalFile[] LargeTree =
	{
		new("one.bin",1024 * 1024),
		new("two.bin",1024 * 1024),
		new("sub/three.bin",1024 * 1024),
	};

	/// <summary>
	/// A wide tree covering multiple sibling folders, a variety of extensions, "junk" folders
	/// (node_modules, .git, bin, obj) and a nested folder (docs/archive, images/thumbs) so every
	/// rule type has meaningful things to include/exclude.
	/// </summary>
	private static LocalFile[] WideTree =
	{
		new("a.txt", 10),
		new("b.log", 20),
		new("c.tmp", 30),
		new("docs/readme.txt", 40),
		new("docs/notes.md", 50),
		new("docs/archive/old.txt", 60),
		new("docs/archive/old.log", 70),
		new("images/photo.jpg", 80),
		new("images/photo.png", 90),
		new("images/thumbs/thumb1.jpg", 100),
		new("images/thumbs/thumb2.png", 110),
		new("src/main.cs", 120),
		new("src/helper.cs", 130),
		new("src/bin/app.exe", 140),
		new("src/bin/app.pdb", 150),
		new("src/obj/temp.tmp", 160),
		new("node_modules/lib/index.js", 170),
		new("node_modules/package.json", 180),
		new(".git/config", 190),
		new(".git/HEAD", 200),
	};

	/// <summary>Root-level files with mixed-case extensions, for ExtensionRule case-insensitivity.</summary>
	private static LocalFile[] ExtensionCaseTree =
	{
		new("lower.txt", 10),
		new("upper.TXT", 20),
		new("mixed.TxT", 30),
		new("image.JPG", 40),
		new("image.jpg", 50),
		new("doc.pdf", 60),
	};

	// ---------------------------------------------------------------------
	// Helpers
	// ---------------------------------------------------------------------

	/// <summary>
	/// Upload with rules applied, then download everything to inspect what made it remotely.
	/// Cleans up by deleting the entire local and remote dirs.
	/// </summary>
	private async Task AssertUploadFilter(LocalFile[] tree, IList<StorageRule> rules, LocalFile[] expected) {

		// generate random paths
		string upload = RandomLocalFolder();
		string remote = RandomRemoteFolder();
		string download = RandomLocalFolder();

		// upload and download
		await UploadTree(tree, upload, remote, rules: rules);
		await DownloadTree(remote, download);
		AssertLocalTree(download, expected);

		// cleanup
		await _storage.DeleteDirectory(remote, true);
	}

	/// <summary>
	/// Upload everything, then download with rules applied to inspect what made it locally.
	/// Cleans up by deleting the entire local and remote dirs.
	/// </summary>
	private async Task AssertDownloadFilter(LocalFile[] tree, IList<StorageRule> rules, LocalFile[] expected) {

		// generate random paths
		string upload = RandomLocalFolder();
		string remote = RandomRemoteFolder();
		string download = RandomLocalFolder();

		// upload and download
		await UploadTree(tree, upload, remote);
		await DownloadTree(remote, download, rules: rules);
		AssertLocalTree(download, expected);

		// cleanup
		await _storage.DeleteDirectory(remote, true);
	}

	/// <summary>
	/// Runs both upload and download filter checks for the same rule set.
	/// Cleans up by deleting the entire local and remote dirs.
	/// </summary>
	private async Task AssertUploadAndDownload(LocalFile[] tree, IList<StorageRule> rules, LocalFile[] expected) {
		await AssertUploadFilter(tree, rules, expected);
		await AssertDownloadFilter(tree, rules, expected);
	}

	private static void CreateLocalTree(
		string root,
		IEnumerable<LocalFile> files) {
		foreach (LocalFile file in files) {
			string full = Path.Combine(root, file.RelativePath.Replace('/', Path.DirectorySeparatorChar));

			Directory.CreateDirectory(Path.GetDirectoryName(full)!);

			File.WriteAllBytes(full, CreateBytes(file.Size));
		}
	}

	private static byte[] CreateBytes(int length) {
		byte[] data = new byte[length];

		for (int i = 0; i < length; i++)
			data[i] = (byte)(i % 251);

		return data;
	}

	// ---------------------------------------------------------------------
	// Remote Verification
	// ---------------------------------------------------------------------

	/// <summary>
	/// Ensure remote directory meets the expected criteria. Cleans up by deleting the entire remote dir.
	/// </summary>
	protected async Task AssertRemoteTree(
		string remoteRoot,
		IEnumerable<LocalFile> expected) {
		var files = expected.ToList();

		foreach (var file in files) {
			string remotePath = $"{remoteRoot}/{file.RelativePath.Replace('\\', '/')}";

			Assert.True(await _storage.ObjectExists(remotePath));

			Assert.Equal(file.Size, await _storage.GetObjectLength(remotePath));
		}

		var remoteFiles = await _storage.ListDirectory(remoteRoot, true);

		Assert.Equal(files.Count, remoteFiles.Count(x => x.IsFile));

		// cleanup
		await _storage.DeleteDirectory(remoteRoot, true);
	}

	protected async Task AssertRemoteDirectoriesExist(string remoteRoot,
		IEnumerable<LocalFile> expected) {
		var folders = expected
			.Select(x => Path.GetDirectoryName(x.RelativePath))
			.Where(x => !string.IsNullOrWhiteSpace(x))
			.Distinct();

		foreach (string folder in folders) {
			string remote = $"{remoteRoot}/{folder.Replace('\\', '/')}";

			Assert.True(await _storage.DirectoryExists(remote));
		}
	}

	protected async Task AssertRemoteContainsExactly(string remoteRoot, int expectedFiles) {
		var list = await _storage.ListDirectory(remoteRoot, true);

		Assert.Equal(expectedFiles, list.Count(x => x.IsFile));

		// cleanup
		await _storage.DeleteDirectory(remoteRoot, true);
	}

	// ---------------------------------------------------------------------
	// Local Verification
	// ---------------------------------------------------------------------

	/// <summary>
	/// Ensure disk directory meets the expected criteria. Cleans up by deleting the entire disk dir.
	/// </summary>
	private static void AssertLocalTree(string root,
		IEnumerable<LocalFile> expected) {
		var files = expected.ToList();

		foreach (var file in files) {
			string full = Path.Combine(root, file.RelativePath.Replace('/', Path.DirectorySeparatorChar));

			Assert.True(File.Exists(full));

			Assert.Equal(file.Size, new FileInfo(full).Length);
		}

		Assert.Equal(files.Count, Directory.GetFiles(root, "*", SearchOption.AllDirectories).Length);

		Directory.Delete(root, true);
	}

	// ---------------------------------------------------------------------
	// Progress Helpers
	// ---------------------------------------------------------------------

	protected sealed class ProgressRecorder {
		public List<StorageProgress> Items { get; } = new();

		public void Report(StorageProgress progress) {
			lock (Items) {
				Items.Add(progress);
			}
		}

		public int Count => Items.Count;

		public int SuccessCount =>
			Items.Count(x => x.Error == null);

		public int FailureCount =>
			Items.Count(x => x.Error != null);
	}

	// ---------------------------------------------------------------------
	// Convenience Helpers
	// ---------------------------------------------------------------------

	protected async Task UploadTree(
		IList<LocalFile> tree,
		string localRoot,
		string remoteRoot,
		StorageExists mode = StorageExists.Skip,
		ProgressRecorder recorder = null,
		IList<StorageRule> rules = null) {
		CreateLocalTree(localRoot, tree);

		await _storage.UploadDirectory(localRoot, remoteRoot, mode, recorder != null ? recorder.Report : null, rules);
	}

	protected async Task DownloadTree(
		string remoteRoot,
		string localRoot,
		StorageExists mode = StorageExists.Skip,
		ProgressRecorder recorder = null,
		IList<StorageRule> rules = null) {
		await _storage.DownloadDirectory(remoteRoot, localRoot, mode, recorder != null ? recorder.Report : null, rules);
	}

}
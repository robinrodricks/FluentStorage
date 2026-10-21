using FluentStorage.Storage;

namespace FluentStorage.Tests.Unit.Storage;

/// <summary>
/// Unit tests covering the folders the in-memory store (<see cref="StorageFactory.InMemory"/>) lists,
/// compared with DiskStore.
/// </summary>
public class MemoryStoreListTests {

	/// <summary>
	/// Verifies that a non-recursive listing returns the files and immediate subfolders (including
	/// empty ones) but nothing deeper, as DiskStore, <see cref="FluentStorage.AWS.Storage.S3Store"/> and <see cref="FluentStorage.Azure.Blobs.Storage.AzureBlobStore"/> do.
	/// </summary>
	[Fact]
	public async Task ListDirectory_NonRecursive_ReturnsFilesAndImmediateSubfoldersOnly() {
		var store = StorageFactory.InMemory();
		await store.SetText("root.txt", "x");
		await store.SetText("a/one.txt", "x");
		await store.SetText("a/b/two.txt", "x");
		await store.CreateDirectory("empty", false);

		List<StoreObject> items = await store.ListDirectory("", false);

		Assert.Equal(
			new[] { "a (Folder)", "empty (Folder)", "root.txt (File)" },
			Describe(items));
	}

	/// <summary>
	/// Verifies that <see cref="StorageListOptions.FilePrefix"/> filters folder entries as well as files,
	/// as on DiskStore.
	/// </summary>
	[Fact]
	public async Task ListObjects_NonRecursiveWithFilePrefix_FiltersFoldersByPrefixToo() {
		var store = StorageFactory.InMemory();
		await store.SetText("xa/one.txt", "x");
		await store.SetText("yb/two.txt", "x");
		await store.SetText("x.txt", "x");

		List<StoreObject> items = await store.ListObjects(new StorageListOptions { FilePrefix = "x" });

		Assert.Equal(new[] { "x.txt (File)", "xa (Folder)" }, Describe(items));
	}

	/// <summary>
	/// Verifies that folders are matched with the same exact case as files, so one listing never
	/// returns a folder's subfolders without its files.
	/// </summary>
	[Fact]
	public async Task ListDirectory_FolderPathInOtherCase_MatchesFoldersCaseSensitivelyLikeFiles() {
		var store = StorageFactory.InMemory();
		await store.SetText("a/x.txt", "x");
		await store.CreateDirectory("a/sub", false);

		Assert.Empty(await store.ListDirectory("A", false));
		Assert.Equal(new[] { "a/sub (Folder)", "a/x.txt (File)" }, Describe(await store.ListDirectory("a", false)));
	}

	private static IEnumerable<string> Describe(IEnumerable<StoreObject> items) =>
		items.Select(o => $"{o.FullPath} ({o.Type})").OrderBy(s => s, StringComparer.Ordinal);
}

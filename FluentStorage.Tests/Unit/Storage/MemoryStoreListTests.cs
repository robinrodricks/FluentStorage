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

	/// <summary>
	/// Verifies that after a move the source folder is gone and the destination, with any parent
	/// folders it needs, is listed, as on DiskStore.
	/// </summary>
	[Theory]
	[InlineData("b", "b (Folder)")]
	[InlineData("p/q", "p (Folder)")]
	public async Task MoveDirectory_ThenListRoot_ShowsOnlyTheDestination(string destination, string expectedRootFolder) {
		var store = StorageFactory.InMemory();
		await store.SetText("a/x.txt", "x");

		await store.MoveDirectory("a", destination);

		Assert.Equal(new[] { expectedRootFolder }, Describe(await store.ListDirectory("", false)));
		Assert.False(await store.DirectoryExists("a"));
	}

	/// <summary>
	/// Verifies that moving an empty folder moves it rather than copying it.
	/// </summary>
	[Fact]
	public async Task MoveDirectory_EmptyFolder_MovesIt() {
		var store = StorageFactory.InMemory();
		await store.CreateDirectory("e", false);

		await store.MoveDirectory("e", "f");

		Assert.Equal(new[] { "f (Folder)" }, Describe(await store.ListDirectory("", false)));
	}

	/// <summary>
	/// Verifies that files are moved even when their folder is no longer recorded. A non-recursive
	/// <see cref="IStore.DeleteDirectory"/> in a different case can leave a file in that state.
	/// </summary>
	[Fact]
	public async Task MoveDirectory_SourceFolderNoLongerRecorded_StillMovesItsFiles() {
		var store = StorageFactory.InMemory();
		await store.SetText("A/x.txt", "x");
		await store.DeleteDirectory("a", false);

		await store.MoveDirectory("A", "b");

		Assert.Equal("x", await store.GetText("b/x.txt"));
	}

	/// <summary>
	/// Verifies that moving a folder that does not exist leaves the store unchanged, as on DiskStore.
	/// </summary>
	[Fact]
	public async Task MoveDirectory_SourceDoesNotExist_CreatesNothing() {
		var store = StorageFactory.InMemory();
		await store.SetText("r.txt", "x");

		await store.MoveDirectory("nope", "dest");

		Assert.False(await store.DirectoryExists("dest"));
		Assert.Equal(new[] { "r.txt (File)" }, Describe(await store.ListDirectory("", false)));
	}

	/// <summary>
	/// Verifies that subfolders are moved even when their parent folder is no longer recorded and holds
	/// no files. A non-recursive
	/// <see cref="IStore.DeleteDirectory"/> in a different case can leave a folder in that state.
	/// </summary>
	[Fact]
	public async Task MoveDirectory_SourceHoldsOnlySubfolders_StillMovesThem() {
		var store = StorageFactory.InMemory();
		await store.CreateDirectory("A/sub", false);
		await store.DeleteDirectory("a", false);

		await store.MoveDirectory("A", "b");

		Assert.True(await store.DirectoryExists("b/sub"));
	}

	/// <summary>
	/// Verifies that moving a folder up into its parent keeps a subfolder that lands on the moved
	/// folder's own path. Here a/a becomes a, so a must still exist after the move.
	/// </summary>
	[Fact]
	public async Task MoveDirectory_ToParent_KeepsSubfolderThatLandsOnTheSourcePath() {
		var store = StorageFactory.InMemory();
		await store.CreateDirectory("p/a/a", false);

		await store.MoveDirectory("p/a", "p");

		Assert.True(await store.DirectoryExists("p/a"));
	}

	/// <summary>
	/// Verifies that a chain of same-named subfolders survives a move up into the parent, where each
	/// subfolder lands on the old path of the one below it: p/a/a becomes p/a, and p/a/a/a becomes p/a/a.
	/// </summary>
	[Fact]
	public async Task MoveDirectory_ToParent_KeepsAChainOfSameNamedSubfolders() {
		var store = StorageFactory.InMemory();
		await store.CreateDirectory("p/a/a/a", false);

		await store.MoveDirectory("p/a", "p");

		Assert.True(await store.DirectoryExists("p/a"));
		Assert.True(await store.DirectoryExists("p/a/a"));
	}

	/// <summary>
	/// Verifies the same for a move to the root, with a file at the bottom of the chain, which must
	/// still be listed in its folder afterwards.
	/// </summary>
	[Fact]
	public async Task MoveDirectory_ToRoot_KeepsAChainOfSameNamedSubfolders() {
		var store = StorageFactory.InMemory();
		await store.SetText("b/b/b/f.txt", "x");

		await store.MoveDirectory("b", "");

		Assert.Equal(new[] { "b/b (Folder)" }, Describe(await store.ListDirectory("b", false)));
		Assert.Equal(new[] { "b/b/f.txt (File)" }, Describe(await store.ListDirectory("b/b", false)));
	}

	private static IEnumerable<string> Describe(IEnumerable<StoreObject> items) =>
		items.Select(o => $"{o.FullPath} ({o.Type})").OrderBy(s => s, StringComparer.Ordinal);
}

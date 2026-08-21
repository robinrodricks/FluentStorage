using FluentStorage.Exceptions;
using FluentStorage.Git.Storage;
using LibGit2Sharp;

namespace FluentStorage.Tests.Integration.Git;

public class GitStoreTest {

	private static GitStore CreateStore(GitStorageOptions options = null) {
		string remotePath = GitTestHelpers.CreateSeedRepository();
		options = options ?? new GitStorageOptions { PullBeforeWrite = false };

		return (GitStore)GitStorage.FromUrl(remotePath, options);
	}

	private static GitStore CreateStoreFromRemote(string remotePath, GitStorageOptions options = null) {
		options = options ?? new GitStorageOptions { PullBeforeWrite = false };
		return (GitStore)GitStorage.FromUrl(remotePath, options);
	}

	[Fact]
	public async Task Commit_GroupsMultipleFilesIntoOneCommit() {
		using GitStore store = CreateStore();

		await store.SetText("a.txt", "aaa");
		await store.SetText("b.txt", "bbb");
		await store.SetText("c.txt", "ccc");

		Commit commit = await store.GitCommit("batch");

		Assert.NotNull(commit);
		Assert.NotNull(commit.Tree["a.txt"]);
		Assert.NotNull(commit.Tree["b.txt"]);
		Assert.NotNull(commit.Tree["c.txt"]);
	}

	[Fact]
	public async Task Commit_NoChanges_ReturnsNull() {
		using GitStore store = CreateStore();

		Commit commit = await store.GitCommit("nothing");

		Assert.Null(commit);
	}

	[Fact]
	public async Task Push_IsVisibleInSecondClone() {
		string remotePath = GitTestHelpers.CreateSeedRepository();
		using GitStore store = CreateStoreFromRemote(remotePath);

		await store.SetText("pushed.txt", "hello");
		await store.GitCommitAndPush("push test");

		string secondClone = Path.Combine(Path.GetTempPath(), "FluentStorage.Git.Tests", Guid.NewGuid().ToString("N"));
		Directory.CreateDirectory(secondClone);
		Repository.Clone(remotePath, secondClone);

		Assert.True(File.Exists(Path.Combine(secondClone, "pushed.txt")));
		Assert.Equal("hello", File.ReadAllText(Path.Combine(secondClone, "pushed.txt")));
	}

	[Fact]
	public async Task Pull_BringsRemoteChanges() {
		string remotePath = GitTestHelpers.CreateSeedRepository();
		using GitStore storeA = CreateStoreFromRemote(remotePath);
		using GitStore storeB = CreateStoreFromRemote(remotePath);

		await storeA.SetText("shared.txt", "from-a");
		await storeA.GitCommitAndPush("a");

		Assert.False(await storeB.ObjectExists("shared.txt"));

		await storeB.GitPull();

		Assert.True(await storeB.ObjectExists("shared.txt"));
		Assert.Equal("from-a", await storeB.GetText("shared.txt"));
	}

	[Fact]
	public async Task AutoCommit_CreatesCommitOnWrite() {
		string remotePath = GitTestHelpers.CreateSeedRepository();
		using GitStore store = CreateStoreFromRemote(remotePath, new GitStorageOptions { AutoCommit = true, PullBeforeWrite = false });

		await store.SetText("auto.txt", "x");

		Commit head = store.GetCurrentCommit();
		Assert.NotNull(head);
		Assert.NotNull(head.Tree["auto.txt"]);
	}

	[Fact]
	public async Task RootPath_IsolatesSubfolder() {
		using GitStore store = CreateStore(new GitStorageOptions { RootPath = "data", PullBeforeWrite = false });

		await store.SetText("file.txt", "in-data");

		Assert.True(File.Exists(Path.Combine(store.LocalWorkingDirectory, "data", "file.txt")));

		List<StoreObject> all = await store.ListDirectory(null, true);
		Assert.Contains(all, o => o.FullPath == "file.txt");
	}

	[Fact]
	public async Task PathTraversal_IsBlocked() {
		using GitStore store = CreateStore();

		await Assert.ThrowsAsync<StorageException>(() => store.SetText("../../outside.txt", "evil"));
	}

	[Fact]
	public async Task Versioning_ListGetRestore() {
		using GitStore store = CreateStore(new GitStorageOptions { AutoCommit = true, PullBeforeWrite = false });

		await store.SetText("v.txt", "v1");
		await store.SetText("v.txt", "v2");

		List<StorageObjectVersion> versions = await store.ListObjectVersions("v.txt");
		Assert.True(versions.Count >= 2);
		Assert.Contains(versions, v => v.IsCurrent);

		StorageObjectVersion older = versions.First(v => !v.IsCurrent);
		StorageObjectVersion info = await store.GetObjectVersion("v.txt", older.VersionId);
		Assert.Equal(older.VersionId, info.VersionId);

		bool restored = await store.RestoreObjectVersion("v.txt", older.VersionId);
		Assert.True(restored);
		Assert.Equal("v1", await store.GetText("v.txt"));
	}

	[Fact]
	public async Task Metadata_SidecarRoundtrips() {
		using GitStore store = CreateStore();

		await store.SetText("m.txt", "data");

		var obj = new StoreObject("m.txt");
		obj.Metadata["user"] = "ivanilson";
		obj.Metadata["fun"] = "no";
		await store.SetObjectInfo(obj);

		StoreObject info = await store.GetObjectInfo("m.txt");
		Assert.Equal("ivanilson", info.Metadata["user"]);
		Assert.Equal("no", info.Metadata["fun"]);

		List<StoreObject> all = await store.ListDirectory(null, true);
		Assert.DoesNotContain(all, o => o.FullPath.EndsWith(".attr"));
	}

	[Fact]
	public async Task FromToken_ClonesLocalRepository() {
		string remotePath = GitTestHelpers.CreateSeedRepository();

		using var store = (GitStore)GitStorage.FromToken(remotePath, "some-token");

		Assert.True(await store.ObjectExists("README.md"));
	}
}
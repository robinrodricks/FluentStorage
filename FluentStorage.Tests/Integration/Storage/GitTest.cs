using FluentStorage.Git.Storage;
using FluentStorage.Tests.Integration.Git;

namespace FluentStorage.Tests.Integration.Storage;

public class GitTestFixture : StoreFixture {

	public GitTestFixture() : base("tests") {
	}

	protected override IStore CreateStorage(TestConfig settings) {
		string remotePath = GitTestHelpers.CreateSeedRepository();

		return GitStorage.FromUrl(remotePath, new GitStorageOptions {
			AutoCommit = false,
			AutoPush = false,
			PullBeforeWrite = false,
		});
	}
}

public class GitTest : IStoreTest, IClassFixture<GitTestFixture> {
	public GitTest(GitTestFixture fixture) : base(fixture) {
	}
}
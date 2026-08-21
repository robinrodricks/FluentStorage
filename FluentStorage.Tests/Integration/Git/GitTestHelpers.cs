using LibGit2Sharp;

namespace FluentStorage.Tests.Integration.Git;

internal static class GitTestHelpers {
	/// <summary>
	/// Creates a local git repository with a single seed commit that can be used as a clone source (remote).
	/// The returned path points to a bare repository so it can also be pushed to.
	/// </summary>
	public static string CreateSeedRepository(string seedFile = "README.md", string seedContent = "seed") {
		string workDir = Path.Combine(Path.GetTempPath(), "FluentStorage.Git.Tests", Guid.NewGuid().ToString("N"));
		Directory.CreateDirectory(workDir);

		Repository.Init(workDir);

		using (var repo = new Repository(workDir)) {
			File.WriteAllText(Path.Combine(workDir, seedFile), seedContent);

			Commands.Stage(repo, "*");

			var signature = new Signature("FluentStorage.Test", "fluentstorage-test@example.com", DateTimeOffset.Now);
			repo.Commit("seed commit", signature, signature);
		}

		string bareDir = workDir + ".bare";
		Repository.Clone(workDir, bareDir, new CloneOptions { IsBare = true });

		return bareDir;
	}
}
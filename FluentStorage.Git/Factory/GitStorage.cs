using FluentStorage.ConnectionStrings;
using FluentStorage.Git;
using FluentStorage.Git.Storage;
using FluentStorage.Storage;

namespace FluentStorage;

/// <summary>
/// LibGit2Sharp factory to create instances of <see cref="IStore"/> backed by a git repository.
/// </summary>
public static class GitStorage {

	/// <summary>
	/// Enable git connection string support.
	/// </summary>
	public static void Use() {
		StorageFactory.Use(new Module());
	}

	private class Module : IExternalModule {
		public IConnectionFactory ConnectionFactory => new ConnectionFactory();
	}

	/// <summary>
	/// Constructs a git store that clones the given remote using HTTPS username/password authentication.
	/// </summary>
	public static IStore FromCredentials(
		string url, string userName, string password,
		string branch = null, string rootPath = null) {
		return new GitStore(new GitStorageOptions {
			Url = url,
			UserName = userName,
			Password = password,
			Branch = branch,
			RootPath = rootPath
		});
	}

	/// <summary>
	/// Constructs a git store that clones the given remote using a personal access token.
	/// </summary>
	public static IStore FromToken(
		string url, string token,
		string branch = null, string rootPath = null) {
		return new GitStore(new GitStorageOptions {
			Url = url,
			Token = token,
			Branch = branch,
			RootPath = rootPath
		});
	}

	/// <summary>
	/// Constructs a git store from a repository URL and a full set of options.
	/// </summary>
	public static IStore FromUrl(string url, GitStorageOptions options = null) {
		if (options == null) {
			options = new GitStorageOptions();
		}

		options.Url = url;
		return new GitStore(options);
	}
}
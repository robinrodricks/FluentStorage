using FluentStorage.ConnectionStrings;
using FluentStorage.Git.Storage;
using FluentStorage.Queue;
using FluentStorage.Storage;

namespace FluentStorage.Git;

/// <summary>
/// Creates <see cref="GitStore"/> instances from git connection strings.
/// </summary>
class ConnectionFactory : IConnectionFactory {

	/// <inheritdoc />
	public IStore CreateStore(ConnectionString connectionString) {
		if (connectionString.Prefix == "git") {
			connectionString.GetRequired("url", true, out string url);

			var options = new GitStorageOptions {
				Url = url,
				UserName = connectionString.Get("user"),
				Password = connectionString.Get("password"),
				Token = connectionString.Get("token"),
				Branch = connectionString.Get("branch"),
				RootPath = connectionString.Get("root"),
				LocalWorkingDirectory = connectionString.Get("localpath"),
			};

			return new GitStore(options);
		}

		return null;
	}

	/// <inheritdoc />
	public IQueue CreateQueue(ConnectionString connectionString) => null;
}
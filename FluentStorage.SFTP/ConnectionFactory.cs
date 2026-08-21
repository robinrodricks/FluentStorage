using FluentStorage.ConnectionStrings;
using FluentStorage.Queue;
using FluentStorage.Storage;

namespace FluentStorage.SFTP;

/// <summary>
/// The <see cref="T:FluentStorage.SFTP.ConnectionFactory"/> class is responsible for creating
/// <see cref="T:FluentStorage.SFTP.SshNetSftpBlobStorage"/> instances from supported connection strings.
/// </summary>
/// <seealso cref="T:FluentStorage.ConnectionStrings.IConnectionFactory" />
class ConnectionFactory : IConnectionFactory {
	/// <summary>
	/// The default port for SFTP connections.
	/// </summary>
	public const ushort DefaultPort = 22;

	/// <summary>
	/// Creates a blob storage instance from the specified connection string if supported; Otherwise it returns null.
	/// </summary>
	/// <param name="connectionString">The connection string to parse.</param>
	/// <returns></returns>
	public IStore CreateStore(ConnectionString connectionString) {
		if (connectionString.Prefix == "sftp") {
			connectionString.GetRequired("host", true, out string host);
			connectionString.GetRequired("user", true, out string user);
			connectionString.GetRequired("password", true, out string password);
			var path = connectionString.Get("path");

			ushort port = ushort.TryParse(connectionString.Get("port"), out port) ? port : DefaultPort;

			return new SftpStore(host, port, user, password, path);
		}

		return null;
	}

	/// <summary>
	/// Creates a message publisher instance from the specified connection string if supported; Otherwise it returns null.
	/// </summary>
	/// <param name="connectionString">The connection string to parse.</param>
	/// <returns></returns>
	public IQueue CreateQueue(ConnectionString connectionString) => null;
}
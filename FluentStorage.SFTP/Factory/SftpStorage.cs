using FluentStorage.ConnectionStrings;
using FluentStorage.SFTP;
using FluentStorage.Storage;
using Renci.SshNet;

namespace FluentStorage;

/// <summary>
/// SSH.NET SFTP factory to create instances of `IStore` using this provider.
/// </summary>
public static class SftpStorage {
	private class Module : IExternalModule {
		public IConnectionFactory ConnectionFactory => new ConnectionFactory();
	}

	/// <summary>
	/// Enable SFTP connection string support.
	/// </summary>
	public static void Use() {
		StorageFactory.Use(new Module());
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="T:FluentStorage.SFTP.SshNetSftpBlobStorage" /> class.
	/// </summary>
	/// <param name="connectionInfo">The connection info.</param>
	public static IStore FromConnectionInfo(ConnectionInfo connectionInfo)
		=> new SftpStore(connectionInfo);

	/// <summary>
	/// Initializes a new instance of the <see cref="T:FluentStorage.SFTP.SshNetSftpBlobStorage" /> class.
	/// </summary>
	/// <param name="host">Connection host.</param>
	/// <param name="port">Connection port.</param>
	/// <param name="username">Authentication username.</param>
	/// <param name="password">Authentication password.</param>
	public static IStore FromCredentials(string host, int port, string username, string password)
		=> new SftpStore(host, port, username, password, null);

	/// <summary>
	/// Initializes a new instance of the <see cref="T:FluentStorage.SFTP.SshNetSftpBlobStorage" /> class.
	/// </summary>
	/// <param name="host">Connection host.</param>
	/// <param name="username">Authentication username.</param>
	/// <param name="password">Authentication password.</param>
	public static IStore FromCredentials(string host, string username, string password)
		=> new SftpStore(host, username, password);

	/// <summary>
	/// Initializes a new instance of the <see cref="T:FluentStorage.SFTP.SshNetSftpBlobStorage" /> class.
	/// </summary>
	/// <param name="host">Connection host.</param>
	/// <param name="port">Connection port.</param>
	/// <param name="username">Authentication username.</param>
	/// <param name="keyFiles">Authentication private key file(s) .</param>
	public static IStore FromPrivateKey(string host, int port, string username, params PrivateKeyFile[] keyFiles)
		=> new SftpStore(host, port, username, keyFiles);

	/// <summary>
	/// Initializes a new instance of the <see cref="T:FluentStorage.SFTP.SshNetSftpBlobStorage" /> class.
	/// </summary>
	/// <param name="host">Connection host.</param>
	/// <param name="username">Authentication username.</param>
	/// <param name="keyFiles">Authentication private key file(s) .</param>
	public static IStore FromPrivateKey(string host, string username, params PrivateKeyFile[] keyFiles)
		=> new SftpStore(host, username, keyFiles);

	/// <summary>
	/// Initializes a new instance of the <see cref="T:FluentStorage.SFTP.SshNetSftpBlobStorage" /> class.
	/// </summary>
	/// <param name="sftpClient">The SFTP client.</param>
	/// <param name="disposeClient">if set to true [dispose client].</param>
	public static IStore FromClient(SftpClient sftpClient, bool disposeClient = false)
		=> new SftpStore(sftpClient, new SshClient(sftpClient.ConnectionInfo), disposeClient);
}
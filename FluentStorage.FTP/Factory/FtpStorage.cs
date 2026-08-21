using System.Net;
using FluentFTP;
using FluentStorage.ConnectionStrings;
using FluentStorage.FTP;
using FluentStorage.FTP.Storage;
using FluentStorage.Storage;

namespace FluentStorage;

/// <summary>
/// FluentFTP factory to create instances of `IStore` using this provider.
/// </summary>
public static class FtpStorage {
	/// <summary>
	/// Enable FTP connection string support.
	/// </summary>
	public static void Use() {
		StorageFactory.Use(new Module());
	}

	private class Module : IExternalModule {
		public IConnectionFactory ConnectionFactory => new ConnectionFactory();
	}

	/// <summary>
	/// Constructs an instance of FTP client from host name and credentials
	/// </summary>
	public static IStore FromCredentials(
		string hostNameOrAddress, NetworkCredential credentials,
		FtpDataConnectionType dataConnectionType = FtpDataConnectionType.AutoActive) {
		return new FtpStore(hostNameOrAddress, credentials, dataConnectionType);
	}

	/// <summary>
	/// Constructs an instance of FTP client by accepting a custom instance of FluentFTP client
	/// </summary>
	public static IStore FromClient(
		AsyncFtpClient ftpClient) {
		return new FtpStore(ftpClient, false);
	}

}
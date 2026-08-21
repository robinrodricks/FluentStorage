using System.Net;
using FluentStorage.ConnectionStrings;
using FluentStorage.FTP.Storage;
using FluentStorage.Queue;
using FluentStorage.Storage;

namespace FluentStorage.FTP;

class ConnectionFactory : IConnectionFactory {
	public IStore CreateStore(ConnectionString connectionString) {
		if (connectionString.Prefix == "ftp") {
			connectionString.GetRequired("host", true, out string host);
			connectionString.GetRequired("user", true, out string user);
			connectionString.GetRequired("password", true, out string password);

			return new FtpStore(host, new NetworkCredential(user, password));
		}

		return null;
	}

	public IQueue CreateQueue(ConnectionString connectionString) => null;
}
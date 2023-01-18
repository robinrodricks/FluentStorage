using System.Net;
using FluentStorage.Blobs;
using FluentStorage.ConnectionString;
using FluentStorage.Messaging;

namespace FluentStorage.FTP {
	class ConnectionFactory : IConnectionFactory {
		public IBlobStorage CreateBlobStorage(StorageConnectionString connectionString) {
			if (connectionString.Prefix == "ftp") {
				connectionString.GetRequired("host", true, out string host);
				connectionString.GetRequired("user", true, out string user);
				connectionString.GetRequired("password", true, out string password);

				return new FluentFtpBlobStorage(host, new NetworkCredential(user, password));
			}

			return null;
		}

		public IMessenger CreateMessenger(StorageConnectionString connectionString) => null;
	}
}

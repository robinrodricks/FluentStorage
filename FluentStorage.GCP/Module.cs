using FluentStorage.Blobs;
using FluentStorage.ConnectionString;
using FluentStorage.Messaging;

namespace FluentStorage.Gcp.CloudStorage {
	class Module : IExternalModule, IConnectionFactory {
		public IConnectionFactory ConnectionFactory => new Module();

		public IBlobStorage CreateBlobStorage(StorageConnectionString connectionString) {
			if (connectionString.Prefix == "google.storage") {
				connectionString.GetRequired("bucket", true, out string bucketName);

				var base64EncodedJson = connectionString.Get("cred");
				
				// if cred is empty, the Google Storage SDK will handle default application credentials for us, or fail.
				if (string.IsNullOrWhiteSpace(base64EncodedJson))
					return StorageFactory.Blobs.GoogleCloudStorageFromEnvironmentVariable(bucketName);

				return StorageFactory.Blobs.GoogleCloudStorageFromJson(bucketName, base64EncodedJson, true);
			}

			return null;
		}

		public IMessenger CreateMessenger(StorageConnectionString connectionString) => null;
	}
}

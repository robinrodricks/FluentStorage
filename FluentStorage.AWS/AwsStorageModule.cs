using System;
using FluentStorage.AWS.Blobs;
using FluentStorage.Blobs;
using FluentStorage.ConnectionString;
using FluentStorage.Messaging;

namespace FluentStorage.AWS {
	class AwsStorageModule : IExternalModule, IConnectionFactory {
		public IConnectionFactory ConnectionFactory => this;

		public IBlobStorage CreateBlobStorage(StorageConnectionString connectionString) {
			if (connectionString.Prefix == KnownPrefix.AwsS3) {

				string region = String.Empty;
				
				string cliProfileName = connectionString.Get(KnownParameter.LocalProfileName);
				connectionString.GetRequired(KnownParameter.BucketName, true, out string bucket);

				if (string.IsNullOrEmpty(cliProfileName)) {
					string keyId = connectionString.Get(KnownParameter.KeyId);
					string key = connectionString.Get(KnownParameter.KeyOrPassword);

					if (string.IsNullOrEmpty(keyId) != string.IsNullOrEmpty(key)) {
						throw new ArgumentException($"connection string requires both 'key' and 'keyId' parameters, or neither.");
					}
					
					if (string.IsNullOrEmpty(keyId)) {
						connectionString.GetRequired(KnownParameter.Region, true, out region);

						return new AwsS3BlobStorage(bucket, region);
					}

					// get region and/or serviceUrl options from connection string ...
					
					var serviceUrl = connectionString.Get(KnownParameter.ServiceUrl);
					region = connectionString.Get(KnownParameter.Region);

					// only one or the other is allowed simultaneously, so throw if both are specified
					
					if (!String.IsNullOrWhiteSpace(serviceUrl) && !String.IsNullOrWhiteSpace(region)) {
						throw new ArgumentException($"connection string can have either 'region' or 'serviceUrl' parameters, but not both.");
						
					}
					
					string sessionToken = connectionString.Get(KnownParameter.SessionToken);

					// pass serviceUrl in to blob storage constructor as well as region - previous guard clause ensures that only one will ever be non-NULL
					
					return new AwsS3BlobStorage(keyId, key, sessionToken, bucket, region, serviceUrl);
				}
#if !NET16
				else {
					return AwsS3BlobStorage.FromAwsCliProfile(cliProfileName, bucket, region);
				}
#endif
			}


			return null;
		}

		public IMessenger CreateMessenger(StorageConnectionString connectionString) => null;
	}
}

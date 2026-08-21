using System;
using FluentStorage.ConnectionStrings;
using FluentStorage.Queue;
using FluentStorage.Storage;

namespace FluentStorage.AWS.Storage;

class AwsStorageModule : IExternalModule, IConnectionFactory {
	public IConnectionFactory ConnectionFactory => this;

	public IStore CreateStore(ConnectionString connectionString) {

		// handle service specific prefixes
		if (ConnectionStringPrefix.IsS3Compatible(connectionString.Prefix)) {

			string region = string.Empty;

			string cliProfileName = connectionString.Get(ConnectionStringParam.LocalProfileName);
			connectionString.GetRequired(ConnectionStringParam.BucketName, true, out string bucket);

			if (string.IsNullOrEmpty(cliProfileName)) {
				string keyId = connectionString.Get(ConnectionStringParam.KeyId);
				string key = connectionString.Get(ConnectionStringParam.KeyOrPassword);

				if (string.IsNullOrEmpty(keyId) != string.IsNullOrEmpty(key)) {
					throw new ArgumentException($"connection string requires both 'key' and 'keyId' parameters, or neither.");
				}

				if (string.IsNullOrEmpty(keyId)) {
					connectionString.GetRequired(ConnectionStringParam.Region, true, out region);

					return new S3Store(bucket, region);
				}

				// get region and/or serviceUrl options from connection string ...

				var serviceUrl = connectionString.Get(ConnectionStringParam.ServiceUrl);
				region = connectionString.Get(ConnectionStringParam.Region);

				// only one or the other is allowed simultaneously, so throw if both are specified

				if (!string.IsNullOrWhiteSpace(serviceUrl) && !string.IsNullOrWhiteSpace(region)) {
					throw new ArgumentException($"connection string can have either 'region' or 'serviceUrl' parameters, but not both.");

				}

				string sessionToken = connectionString.Get(ConnectionStringParam.SessionToken);



				// [ADD STORAGE PROVIDER]

				// USE SPECIAL CONSTRUCTORS for special providers

				if (connectionString.Prefix == ConnectionStringPrefix.MinIoS3) {
					return S3Store.FromMinIO(keyId, key, bucket, region, serviceUrl, sessionToken);
				}
				else if (connectionString.Prefix == ConnectionStringPrefix.CloudflareR2) {
					string accountId = connectionString.Get(ConnectionStringParam.AccountId);
					return S3Store.FromCloudflareR2(keyId, key, bucket, accountId);
				}
				else if (connectionString.Prefix == ConnectionStringPrefix.Wasabi) {
					return S3Store.FromWasabi(keyId, key, bucket, serviceUrl, sessionToken);
				}
				else if (connectionString.Prefix == ConnectionStringPrefix.DigitalOceanSpaces) {
					return S3Store.FromDigitalOcean(keyId, key, bucket, region, sessionToken);
				}
				else if (connectionString.Prefix == ConnectionStringPrefix.BackblazeB2) {
					return S3Store.FromBackblazeB2(keyId, key, bucket, region);
				}
				else if (connectionString.Prefix == ConnectionStringPrefix.Hetzner) {
					return S3Store.FromHetzner(keyId, key, bucket, region);
				}
				else if (connectionString.Prefix == ConnectionStringPrefix.Vultr) {
					string hostName = connectionString.Get(ConnectionStringParam.HostName);
					return S3Store.FromVultr(keyId, key, bucket, hostName);
				}

				// fallback to S3 constructor if its not a special providr

				else if (connectionString.Prefix == ConnectionStringPrefix.AwsS3) {
					return new S3Store(keyId, key, sessionToken, bucket, region, serviceUrl);
				}

			}
#if !NET16
			else {
				return S3Store.FromAwsCliProfile(cliProfileName, bucket, region);
			}
#endif
		}


		return null;
	}

	public IQueue CreateQueue(ConnectionString connectionString) => null;
}
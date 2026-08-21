using FluentStorage.ConnectionStrings;
using FluentStorage.Queue;
using FluentStorage.Storage;

namespace FluentStorage.Azure.Blobs.Storage;

class Module : IExternalModule, IConnectionFactory {
	public IConnectionFactory ConnectionFactory => this;

	public IStore CreateStore(ConnectionString connectionString) {
		if (connectionString.Prefix == ConnectionStringPrefix.AzureBlobStorage) {
			if (connectionString.Parameters.ContainsKey(ConnectionStringParam.IsLocalEmulator)) {
				return AzureBlobStorage.FromLocalEmulator();
			}

			connectionString.GetRequired(ConnectionStringParam.AccountName, true, out string accountName);

			string sharedKey = connectionString.Get(ConnectionStringParam.KeyOrPassword);
			if (!string.IsNullOrEmpty(sharedKey)) {
				return AzureBlobStorage.FromSharedKey(accountName, sharedKey);
			}

			string tenantId = connectionString.Get(ConnectionStringParam.TenantId);
			if (!string.IsNullOrEmpty(tenantId)) {
				connectionString.GetRequired(ConnectionStringParam.ClientId, true, out string clientId);
				connectionString.GetRequired(ConnectionStringParam.ClientSecret, true, out string clientSecret);

				return AzureBlobStorage.FromAzureAd(accountName, tenantId, clientId, clientSecret);
			}

			if (connectionString.Parameters.ContainsKey(ConnectionStringParam.MsiEnabled)) {
				return AzureBlobStorage.FromMsi(accountName);
			}
		}
		else if (connectionString.Prefix == ConnectionStringPrefix.AzureDataLakeGen2 || connectionString.Prefix == ConnectionStringPrefix.AzureDataLake) {
			connectionString.GetRequired(ConnectionStringParam.AccountName, true, out string accountName);

			string sharedKey = connectionString.Get(ConnectionStringParam.KeyOrPassword);
			if (!string.IsNullOrEmpty(sharedKey)) {
				return AzureDataLakeStorage.FromSharedKey(accountName, sharedKey);
			}

			string tenantId = connectionString.Get(ConnectionStringParam.TenantId);
			if (!string.IsNullOrEmpty(tenantId)) {
				connectionString.GetRequired(ConnectionStringParam.ClientId, true, out string clientId);
				connectionString.GetRequired(ConnectionStringParam.ClientSecret, true, out string clientSecret);

				return AzureDataLakeStorage.FromAzureAd(accountName, tenantId, clientId, clientSecret);
			}

			if (connectionString.Parameters.ContainsKey(ConnectionStringParam.MsiEnabled)) {
				return AzureDataLakeStorage.FromMsi(accountName);
			}

		}

		return null;
	}

	public IQueue CreateQueue(ConnectionString connectionString) => null;
}
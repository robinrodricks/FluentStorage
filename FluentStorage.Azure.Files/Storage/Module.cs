using FluentStorage.ConnectionStrings;
using FluentStorage.Queue;
using FluentStorage.Storage;

namespace FluentStorage.Azure.Files.Storage;

class Module : IExternalModule, IConnectionFactory {
	public IConnectionFactory ConnectionFactory => this;

	public IStore CreateStore(ConnectionString connectionString) {
		if (connectionString.Prefix == ConnectionStringPrefix.AzureFilesStorage) {
			connectionString.GetRequired(ConnectionStringParam.AccountName, true, out string accountName);

			string sharedKey = connectionString.Get(ConnectionStringParam.KeyOrPassword);
			if (!string.IsNullOrEmpty(sharedKey)) {
				return AzureFilesStorage.FromSharedKey(accountName, sharedKey);
			}

			string tenantId = connectionString.Get(ConnectionStringParam.TenantId);
			if (!string.IsNullOrEmpty(tenantId)) {
				connectionString.GetRequired(ConnectionStringParam.ClientId, true, out string clientId);
				connectionString.GetRequired(ConnectionStringParam.ClientSecret, true, out string clientSecret);

				return AzureFilesStorage.FromAzureAd(accountName, tenantId, clientId, clientSecret);
			}

			if (connectionString.Parameters.ContainsKey(ConnectionStringParam.MsiEnabled)) {
				return AzureFilesStorage.FromMsi(accountName);
			}
		}

		return null;
	}

	public IQueue CreateQueue(ConnectionString connectionString) => null;
}
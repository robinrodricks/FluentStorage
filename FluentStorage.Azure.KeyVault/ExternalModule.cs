using System;
using FluentStorage.ConnectionStrings;
using FluentStorage.Queue;
using FluentStorage.Storage;

namespace FluentStorage.Azure.KeyVault;

class ExternalModule : IExternalModule, IConnectionFactory {
	public IConnectionFactory ConnectionFactory => this;

	public IStore CreateStore(ConnectionString connectionString) {
		if (connectionString.Prefix == ConnectionStringPrefix.AzureKeyVault) {
			connectionString.GetRequired(ConnectionStringParam.VaultUri, true, out string uri);

			if (connectionString.Parameters.ContainsKey(ConnectionStringParam.MsiEnabled)) {
				return AzureKeyVaultStorage.FromMsi(new Uri(uri));
			}
			else {
				connectionString.GetRequired(ConnectionStringParam.TenantId, true, out string tenantId);
				connectionString.GetRequired(ConnectionStringParam.ClientId, true, out string clientId);
				connectionString.GetRequired(ConnectionStringParam.ClientSecret, true, out string clientSecret);

				return AzureKeyVaultStorage.FromCredentials(new Uri(uri), tenantId, clientId, clientSecret);
			}
		}

		return null;
	}

	public IQueue CreateQueue(ConnectionString connectionString) => null;
}
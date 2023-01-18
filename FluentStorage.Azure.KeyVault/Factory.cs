using Azure.Core;
using Azure.Identity;
using FluentStorage.Blobs;
using FluentStorage.Azure.KeyVault;
using FluentStorage.Azure.KeyVault.Blobs;
using System;
using System.Net;

namespace FluentStorage {
	public static class Factory {
		public static IModulesFactory UseAzureKeyVault(this IModulesFactory factory) {
			return factory.Use(new ExternalModule());
		}

		/// <summary>
		/// Azure Key Vault secrets.
		/// </summary>
		/// <param name="factory">The factory.</param>
		/// <param name="vaultUri">The vault URI.</param>
		/// <param name="azureAadClientId">The azure aad client identifier.</param>
		/// <param name="azureAadClientSecret">The azure aad client secret.</param>
		/// <returns></returns>
		public static IBlobStorage AzureKeyVault(this IBlobStorageFactory factory,
		   Uri vaultUri,
		   string tenantId,
		   string applicationId,
		   string applicationSecret,
		   string activeDirectoryAuthEndpoint = "https://login.microsoftonline.com/") {
			TokenCredential credential =
				new ClientSecretCredential(
					tenantId,
					applicationId,
					applicationSecret,
					new TokenCredentialOptions() { AuthorityHost = new Uri(activeDirectoryAuthEndpoint) });

			return new AzureKeyVaultBlobStorageProvider(vaultUri, credential);
		}

		/// <summary>
		/// Azure Key Vault secrets
		/// </summary>
		/// <param name="factory"></param>
		/// <param name="vaultUri"></param>
		/// <returns></returns>
		public static IBlobStorage AzureKeyVaultWithMsi(this IBlobStorageFactory factory, Uri vaultUri) {
			return new AzureKeyVaultBlobStorageProvider(vaultUri, new ManagedIdentityCredential());
		}

	}
}

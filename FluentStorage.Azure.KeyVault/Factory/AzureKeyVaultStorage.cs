using System;
using Azure.Core;
using Azure.Identity;
using FluentStorage.Azure.KeyVault;
using FluentStorage.Azure.KeyVault.Storage;
using FluentStorage.Storage;

namespace FluentStorage;

public static class AzureKeyVaultStorage {
	/// <summary>
	/// Enable Azure KeyVault connection string support.
	/// </summary>
	public static void Use() {
		StorageFactory.Use(new ExternalModule());
	}

	/// <summary>
	/// Azure Key Vault secrets.
	/// </summary>
	/// <param name="factory">The factory.</param>
	/// <param name="vaultUri">The vault URI.</param>
	/// <param name="azureAadClientId">The azure aad client identifier.</param>
	/// <param name="azureAadClientSecret">The azure aad client secret.</param>
	/// <returns></returns>
	public static IStore FromCredentials(
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

		return new AzureKeyVaultStore(vaultUri, credential);
	}

	/// <summary>
	/// Azure Key Vault secrets
	/// </summary>
	/// <param name="factory"></param>
	/// <param name="vaultUri"></param>
	/// <returns></returns>
	public static IStore FromMsi(Uri vaultUri) {
		return new AzureKeyVaultStore(vaultUri, new ManagedIdentityCredential());
	}

}
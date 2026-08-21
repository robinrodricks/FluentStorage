using System;
using Azure.Core;
using Azure.Storage;
using Azure.Storage.Files.Shares;
using FluentStorage.Azure;
using FluentStorage.Azure.Files.Storage;
using FluentStorage.ConnectionStrings;
using FluentStorage.Storage;

namespace FluentStorage;

/// <summary>
/// Azure Files/Data Lake Factory to create instances of `IStore` using this provider.
/// </summary>
public static class AzureFilesStorage {
	/// <summary>
	/// Enable Azure Files connection string support.
	/// </summary>
	public static void Use() {
		StorageFactory.Use(new Module());
	}

	/// <summary>
	/// Creates Azure Files from an existing <see cref="ShareServiceClient"/>.
	/// </summary>
	public static IStore FromClient(
		ShareServiceClient shareServiceClient) {
		if (shareServiceClient is null) {
			throw new ArgumentNullException(nameof(shareServiceClient));
		}

		return new AzureFilesStore(shareServiceClient, shareServiceClient.AccountName);
	}

	/// <summary>
	/// Creates Azure Files from account name and key
	/// </summary>
	/// <param name="factory">Reference to factory</param>
	/// <param name="accountName">Storage Account name</param>
	/// <param name="key">Storage Account key</param>
	/// <returns>Generic blob storage interface</returns>
	public static IStore FromCredentials(
		string accountName,
		string key) {
		return FromSharedKey(accountName, key);
	}

	/// <summary>
	/// Creates Azure Files with Shared Key
	/// </summary>
	public static IStore FromSharedKey(
		string accountName,
		string key,
		Uri serviceUri) {
		return FromSharedKey(accountName, key, serviceUri, default);
	}

	/// <summary>
	///Creates Azure Files with Shared Key
	/// </summary>
	public static IStore FromSharedKey(
		string accountName,
		string key) {
		return FromSharedKey(accountName, key, null, default);
	}

	/// <summary>
	///Creates Azure Files with Shared Key
	/// </summary>
	public static IStore FromSharedKey(
		string accountName,
		string key,
		AzureCloudEnvironment cloudEnvironment) {
		return FromSharedKey(accountName, key, null, cloudEnvironment);
	}

	/// <summary>
	///Creates Azure Files with Shared Key
	/// </summary>
	public static IStore FromSharedKey(
		string accountName,
		string key,
		Uri serviceUri,
		AzureCloudEnvironment cloudEnvironment) {
		if (accountName is null) {
			throw new ArgumentNullException(nameof(accountName));
		}
		if (key is null) {
			throw new ArgumentNullException(nameof(key));
		}

		var credential = new StorageSharedKeyCredential(accountName, key);
		var client = new ShareServiceClient(serviceUri ?? GetServiceUri(accountName, cloudEnvironment), credential);

		return new AzureFilesStore(client, accountName);
	}

	/// <summary>
	/// Create Azure Files with Azure AD 
	/// </summary>
	public static IStore FromAzureAd(
		string accountName,
		string tenantId,
		string applicationId,
		string applicationSecret,
		AzureCloudEnvironment cloudEnvironment) {
		return FromAzureAd(accountName, tenantId, applicationId, applicationSecret, null, cloudEnvironment);
	}

	/// <summary>
	/// Create Azure Files with Azure AD and Active Directory Authority endpoint.
	/// </summary>
	public static IStore FromAzureAd(
		string accountName,
		string tenantId,
		string applicationId,
		string applicationSecret) {
		return FromAzureAd(accountName, tenantId, applicationId, applicationSecret, null, AzureCloudEnvironment.Global);
	}

	/// <summary>
	/// Create Azure Files with Azure AD and Active Directory Authority endpoint.
	/// </summary>
	public static IStore FromAzureAd(
		string accountName,
		string tenantId,
		string applicationId,
		string applicationSecret,
		string activeDirectoryAuthEndpoint) {
		return FromAzureAd(accountName, tenantId, applicationId, applicationSecret, activeDirectoryAuthEndpoint, default);
	}

	/// <summary>
	/// Create Azure Files with Azure AD and Active Directory Authority endpoint.
	/// </summary>
	public static IStore FromAzureAd(
		string accountName,
		string tenantId,
		string applicationId,
		string applicationSecret,
		string activeDirectoryAuthEndpoint,
		AzureCloudEnvironment cloudEnvironment) {
		if (accountName is null) {
			throw new ArgumentNullException(nameof(accountName));
		}
		if (tenantId is null) {
			throw new ArgumentNullException(nameof(tenantId));
		}
		if (applicationId is null) {
			throw new ArgumentNullException(nameof(applicationId));
		}
		if (applicationSecret is null) {
			throw new ArgumentNullException(nameof(applicationSecret));
		}

		TokenCredential credential = AzureStorageIdentity.CreateClientSecretCredential(
			tenantId,
			applicationId,
			applicationSecret,
			activeDirectoryAuthEndpoint,
			cloudEnvironment);

		var client = new ShareServiceClient(GetServiceUri(accountName, cloudEnvironment), credential);

		return new AzureFilesStore(client, accountName);
	}

	/// <summary>
	/// Create Azure Files with Token Credentials
	/// </summary>
	public static IStore FromTokenCredential(
		string accountName,
		TokenCredential tokenCredential) {
		return FromTokenCredential(accountName, tokenCredential, default);
	}

	/// <summary>
	///Create Azure Files with Token Credentials
	/// </summary>
	public static IStore FromTokenCredential(
		string accountName,
		TokenCredential tokenCredential,
		AzureCloudEnvironment azureCloudEnvironment) {
		if (accountName is null) {
			throw new ArgumentNullException(nameof(accountName));
		}
		if (tokenCredential is null) {
			throw new ArgumentNullException(nameof(tokenCredential));
		}

		var client = new ShareServiceClient(GetServiceUri(accountName, azureCloudEnvironment), tokenCredential);

		return new AzureFilesStore(client, accountName);
	}

	/// <summary>
	/// Creates Azure Files with Managed Identity (Managed Service Identity)
	/// </summary>
	public static IStore FromMsi(
		string accountName,
		AzureCloudEnvironment azureCloudEnvironment) {
		return FromMsi(accountName, null, azureCloudEnvironment);
	}

	/// <summary>
	/// Creates Azure Files with Managed Identity (Managed Service Identity)
	/// </summary>
	public static IStore FromMsi(string accountName) {
		return FromMsi(accountName, null, default);
	}

	/// <summary>
	/// Creates Azure Files with Managed Identity (Managed Service Identity)
	/// </summary>
	public static IStore FromMsi(
		string accountName,
		string clientId) {
		return FromMsi(accountName, clientId, default);
	}

	/// <summary>
	/// Creates Azure Files with Managed Identity (Managed Service Identity)
	/// </summary>
	public static IStore FromMsi(
		string accountName,
		string clientId,
		AzureCloudEnvironment azureCloudEnvironment) {
		if (accountName is null) {
			throw new ArgumentNullException(nameof(accountName));
		}

		TokenCredential credential = AzureStorageIdentity.CreateManagedIdentityCredential(clientId);

		var client = new ShareServiceClient(GetServiceUri(accountName, azureCloudEnvironment), credential);

		return new AzureFilesStore(client, accountName);
	}

	/// <summary>
	/// Create connection string for Azure Files with Shared Key
	/// </summary>
	public static ConnectionString CreateConnectionStringFromSharedKey(
		string accountName,
		string accountKey) {
		var cs = new ConnectionString(ConnectionStringPrefix.AzureFilesStorage);
		cs.Parameters[ConnectionStringParam.AccountName] = accountName;
		cs.Parameters[ConnectionStringParam.KeyOrPassword] = accountKey;
		return cs;
	}

	/// <summary>
	/// Create connection string for Azure Files with Azure AD
	/// </summary>
	public static ConnectionString CreateConnectionStringFromAzureAd(
		string accountName,
		string tenantId,
		string applicationId,
		string applicationSecret) {
		var cs = new ConnectionString(ConnectionStringPrefix.AzureFilesStorage);
		cs.Parameters[ConnectionStringParam.AccountName] = accountName;
		cs.Parameters[ConnectionStringParam.TenantId] = tenantId;
		cs.Parameters[ConnectionStringParam.ClientId] = applicationId;
		cs.Parameters[ConnectionStringParam.ClientSecret] = applicationSecret;
		return cs;
	}

	internal static Uri GetServiceUri(string accountName, AzureCloudEnvironment cloudEnvironment = default) {
		return AzureStorageIdentity.CreateFileServiceUri(accountName, cloudEnvironment);
	}
}
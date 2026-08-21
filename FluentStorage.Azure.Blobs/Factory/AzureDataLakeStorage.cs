using System;
using Azure.Core;
using Azure.Storage;
using Azure.Storage.Blobs;
using FluentStorage.Azure;
using FluentStorage.Azure.Blobs;
using FluentStorage.Azure.Blobs.DataLake;
using FluentStorage.Azure.Blobs.Utils;
using FluentStorage.ConnectionStrings;

namespace FluentStorage;

public static class AzureDataLakeStorage {

	/// <summary>
	/// Creates Azure DataLake Storage with Shared Key
	/// </summary>
	public static IAzureDataLakeStore FromSharedKey(
		string accountName,
		string key,
		Uri serviceUri) {
		return FromSharedKey(accountName, key, serviceUri, default);
	}

	/// <summary>
	/// Creates Azure DataLake Storage with Shared Key
	/// </summary>
	public static IAzureDataLakeStore FromSharedKey(string accountName, string key) {
		return FromSharedKey(accountName, key, null, default);
	}

	/// <summary>
	/// Creates Azure DataLake Storage with Shared Key
	/// </summary>
	public static IAzureDataLakeStore FromSharedKey(
		string accountName,
		string key,
		AzureCloudEnvironment cloudEnvironment) {
		return FromSharedKey(accountName, key, null, cloudEnvironment);
	}

	/// <summary>
	/// Creates Azure DataLake Storage with Shared Key
	/// </summary>
	public static IAzureDataLakeStore FromSharedKey(
		string accountName,
		string key,
		Uri serviceUri,
		AzureCloudEnvironment cloudEnvironment) {
		if (accountName is null)
			throw new ArgumentNullException(nameof(accountName));
		if (key is null)
			throw new ArgumentNullException(nameof(key));

		var credential = new StorageSharedKeyCredential(accountName, key);

		var client = new BlobServiceClient(serviceUri ?? AzureBlobUtils.GetServiceUri(accountName, cloudEnvironment), credential);

		return new AzureDataLakeStore(client, accountName, credential, azureCloudEnvironment: cloudEnvironment);
	}


	/// <summary>
	/// Creates Azure DataLake Storage with Azure AD
	/// </summary>
	public static IAzureDataLakeStore FromAzureAd(
		string accountName,
		string tenantId,
		string applicationId,
		string applicationSecret,
		AzureCloudEnvironment cloudEnvironment) {
		return FromAzureAd(accountName, tenantId, applicationId, applicationSecret, null, cloudEnvironment);
	}

	/// <summary>
	/// Creates Azure DataLake Storage with Azure AD and AD Authority endpoint
	/// </summary>
	public static IAzureDataLakeStore FromAzureAd(
		string accountName,
		string tenantId,
		string applicationId,
		string applicationSecret) {
		return FromAzureAd(accountName, tenantId, applicationId, applicationSecret, null, AzureCloudEnvironment.Global);
	}

	/// <summary>
	/// Creates Azure DataLake Storage with Azure AD and AD Authority endpoint
	/// </summary>
	public static IAzureDataLakeStore FromAzureAd(
		string accountName,
		string tenantId,
		string applicationId,
		string applicationSecret,
		string activeDirectoryAuthEndpoint) {
		return FromAzureAd(accountName, tenantId, applicationId, applicationSecret, activeDirectoryAuthEndpoint, default);
	}

	/// <summary>
	/// Create Azure DataLake Gen 2 Storage with Azure AD
	/// </summary>
	public static IAzureDataLakeStore FromAzureAd(
		string accountName,
		string tenantId,
		string applicationId,
		string applicationSecret,
		string activeDirectoryAuthEndpoint,
		AzureCloudEnvironment cloudEnvironment) {
		if (accountName is null)
			throw new ArgumentNullException(nameof(accountName));
		if (tenantId is null)
			throw new ArgumentNullException(nameof(tenantId));
		if (applicationId is null)
			throw new ArgumentNullException(nameof(applicationId));
		if (applicationSecret is null)
			throw new ArgumentNullException(nameof(applicationSecret));

		TokenCredential credential = AzureStorageIdentity.CreateClientSecretCredential(
			tenantId,
			applicationId,
			applicationSecret,
			activeDirectoryAuthEndpoint,
			cloudEnvironment);

		// Create a client that can authenticate using our token credential
		var client = new BlobServiceClient(AzureBlobUtils.GetServiceUri(accountName, cloudEnvironment), credential);

		return new AzureDataLakeStore(client, accountName, azureCloudEnvironment: cloudEnvironment);
	}

	/// <summary>
	/// Creates Azure Data Lake Gen 2 with Managed Identity
	/// </summary>
	public static IAzureDataLakeStore FromMsi(
		string accountName,
		AzureCloudEnvironment azureCloudEnvironment) {
		return FromMsi(accountName, null, azureCloudEnvironment);
	}

	/// <summary>
	/// Creates Azure Data Lake Gen 2 with Managed Identity
	/// </summary>
	public static IAzureDataLakeStore FromMsi(string accountName) {
		return FromMsi(accountName, null, default);
	}

	/// <summary>
	/// Creates Azure Data Lake Gen 2 with Managed Identity (client id)
	/// </summary>
	public static IAzureDataLakeStore FromMsi(
		string accountName,
		string clientId) {
		return FromMsi(accountName, clientId, default);
	}

	/// <summary>
	/// Creates Azure Data Lake Gen 2 Storage with Managed Identity
	/// </summary>
	public static IAzureDataLakeStore FromMsi(
		string accountName,
		string clientId,
		AzureCloudEnvironment azureCloudEnvironment) {
		TokenCredential credential = AzureStorageIdentity.CreateManagedIdentityCredential(clientId);

		var client = new BlobServiceClient(AzureBlobUtils.GetServiceUri(accountName, azureCloudEnvironment), credential);

		return new AzureDataLakeStore(client, accountName, azureCloudEnvironment: azureCloudEnvironment);
	}

	/// <summary>
	/// Create connection string for Azure DataLake with Shared Key
	/// </summary>
	public static ConnectionString CreateConnectionStringFromSharedKey(
		string accountName,
		string accountKey) {
		var cs = new ConnectionString(ConnectionStringPrefix.AzureDataLakeGen2);
		cs.Parameters[ConnectionStringParam.AccountName] = accountName;
		cs.Parameters[ConnectionStringParam.KeyOrPassword] = accountKey;
		return cs;
	}

	/// <summary>
	/// Create connection string for Azure DataLake with Azure AD
	/// </summary>
	public static ConnectionString CreateConnectionStringFromAzureAd(
		string accountName,
		string tenantId,
		string applicationId,
		string applicationSecret) {
		var cs = new ConnectionString(ConnectionStringPrefix.AzureDataLakeGen2);
		cs.Parameters[ConnectionStringParam.AccountName] = accountName;
		cs.Parameters[ConnectionStringParam.TenantId] = tenantId;
		cs.Parameters[ConnectionStringParam.ClientId] = applicationId;
		cs.Parameters[ConnectionStringParam.ClientSecret] = applicationSecret;
		return cs;
	}

}
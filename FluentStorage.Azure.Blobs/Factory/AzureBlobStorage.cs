using System;
using Azure.Core;
using Azure.Storage;
using Azure.Storage.Blobs;
using FluentStorage.Azure;
using FluentStorage.Azure.Blobs;
using FluentStorage.Azure.Blobs.Storage;
using FluentStorage.Azure.Blobs.Utils;
using FluentStorage.ConnectionStrings;

namespace FluentStorage;

/// <summary>
/// Azuree Blob Factory to create instances of `IStore` using this provider.
/// </summary>
public static class AzureBlobStorage {
	/// <summary>
	/// Enable Azure Blob connection string support.
	/// </summary>
	public static void Use() {
		StorageFactory.Use(new Module());
	}

	/// <summary>
	/// Creates Azure Blob Storage from an existing <see cref="BlobServiceClient"/>.
	/// </summary>
	public static IAzureBlobStore FromClient(
		BlobServiceClient blobServiceClient) {
		return FromClient(blobServiceClient, null);
	}

	/// <summary>
	/// Creates Azure Blob Storage from an existing <see cref="BlobServiceClient"/>.
	/// </summary>
	public static IAzureBlobStore FromClient(
		BlobServiceClient blobServiceClient,
		string containerName) {
		if (blobServiceClient is null) {
			throw new ArgumentNullException(nameof(blobServiceClient));
		}

		return new AzureBlobStore(blobServiceClient, blobServiceClient.AccountName, containerName: containerName);
	}

	/// <summary>
	/// Connect to local emulator
	/// </summary>
	public static IAzureBlobStore FromLocalEmulator() {
		var credential = new StorageSharedKeyCredential(
			"devstoreaccount1",
			"Eby8vdM02xNOcqFlqUwJPLlmEtlCDXJ1OUzFT50uSRZ6IFsuFq2UVErCz4I6tq/K1SZFPTOtr/KBHBeksoGMGw==");

		var client = new BlobServiceClient(
			new Uri("http://127.0.0.1:10000/devstoreaccount1"),
			credential);

		return new AzureBlobStore(client, "devstoreaccount1", credential);
	}

	/// <summary>
	/// Creates Azure Blob Storage with Shared Key
	/// </summary>
	public static IAzureBlobStore FromSharedKey(
		string accountName,
		string key,
		Uri serviceUri) {
		return FromSharedKey(accountName, key, serviceUri, default);
	}

	/// <summary>
	/// Creates Azure Blob Storage with Shared Key
	/// </summary>
	public static IAzureBlobStore FromSharedKey(string accountName, string key) {
		return FromSharedKey(accountName, key, null, default);
	}

	/// <summary>
	/// Creates Azure Blob Storage with Shared Key
	/// </summary>
	public static IAzureBlobStore FromSharedKey(
		string accountName,
		string key,
		AzureCloudEnvironment cloudEnvironment) {
		return FromSharedKey(accountName, key, null, cloudEnvironment);
	}

	/// <summary>
	/// Creates Azure Blob Storage with Shared Key
	/// </summary>
	public static IAzureBlobStore FromSharedKey(
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

		return new AzureBlobStore(client, accountName, credential);
	}

	/// <summary>
	/// Creates Azure Blob Storage with Azure AD
	/// </summary>
	public static IAzureBlobStore FromAzureAd(
		string accountName,
		string tenantId,
		string applicationId,
		string applicationSecret,
		AzureCloudEnvironment cloudEnvironment) {
		return FromAzureAd(accountName, tenantId, applicationId, applicationSecret, null, cloudEnvironment);
	}

	/// <summary>
	/// Creates Azure Blob Storage with Azure AD and AD Authority endpoint
	/// </summary>
	public static IAzureBlobStore FromAzureAd(
		string accountName,
		string tenantId,
		string applicationId,
		string applicationSecret) {
		return FromAzureAd(accountName, tenantId, applicationId, applicationSecret, null, AzureCloudEnvironment.Global);
	}

	/// <summary>
	/// Creates Azure Blob Storage with Azure AD and AD Authority endpoint
	/// </summary>
	public static IAzureBlobStore FromAzureAd(
		string accountName,
		string tenantId,
		string applicationId,
		string applicationSecret,
		string activeDirectoryAuthEndpoint) {
		return FromAzureAd(accountName, tenantId, applicationId, applicationSecret, activeDirectoryAuthEndpoint, default);
	}

	/// <summary>
	/// Creates Azure Blob Storage with Azure AD and AD Authority endpoint
	/// </summary>
	public static IAzureBlobStore FromAzureAd(
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

		return new AzureBlobStore(client, accountName);
	}

	/// <summary>
	/// Creates Azure Blob Storage with Token Credential
	/// </summary>
	public static IAzureBlobStore FromTokenCredential(
		string accountName,
		TokenCredential tokenCredential) {
		return FromTokenCredential(accountName, tokenCredential, default);
	}

	/// <summary>
	/// Creates Azure Blob Storage with Token Credential
	/// </summary>
	public static IAzureBlobStore FromTokenCredential(
		string accountName,
		TokenCredential tokenCredential,
		AzureCloudEnvironment azureCloudEnvironment) {
		var client = new BlobServiceClient(AzureBlobUtils.GetServiceUri(accountName, azureCloudEnvironment), tokenCredential);

		return new AzureBlobStore(client, accountName);
	}

	/// <summary>
	/// Creates Azure Blob Storage with Token Credential
	/// </summary>
	public static IAzureBlobStore FromSas(
		string sas, BlobClientOptions options = default) {
		AzureBlobUtils.TryParseSasUrl(sas, out string accountName, out string containerName, out string sasQuery);

		var client = new BlobServiceClient(new Uri(sas), options);

		return new AzureBlobStore(client, accountName, containerName: containerName);
	}

	/// <summary>
	/// Creates Azure Blob Storage with Managed Identity
	/// </summary>
	public static IAzureBlobStore FromMsi(
		string accountName,
		AzureCloudEnvironment azureCloudEnvironment) {
		return FromMsi(accountName, null, azureCloudEnvironment);
	}

	/// <summary>
	/// Creates Azure Blob Storage with Managed Identity
	/// </summary>
	public static IAzureBlobStore FromMsi(string accountName) {
		return FromMsi(accountName, null, default);
	}

	/// <summary>
	/// Creates Azure Blob Storage with Managed Identity (client id)
	/// </summary>
	public static IAzureBlobStore FromMsi(
		string accountName,
		string clientId) {
		return FromMsi(accountName, clientId, default);
	}

	/// <summary>
	/// Creates Azure Blob Storage with Managed Identity
	/// </summary>
	public static IAzureBlobStore FromMsi(
		string accountName,
		string clientId,
		AzureCloudEnvironment azureCloudEnvironment) {
		TokenCredential credential = AzureStorageIdentity.CreateManagedIdentityCredential(clientId);

		var client = new BlobServiceClient(AzureBlobUtils.GetServiceUri(accountName, azureCloudEnvironment), credential);

		return new AzureBlobStore(client, accountName);
	}


	/// <summary>
	/// Create connection string for azure blob storage
	/// </summary>
	public static ConnectionString CreateConnectionStringFromSharedKey(
		string accountName,
		string accountKey) {
		var cs = new ConnectionString(ConnectionStringPrefix.AzureBlobStorage);
		cs.Parameters[ConnectionStringParam.AccountName] = accountName;
		cs.Parameters[ConnectionStringParam.KeyOrPassword] = accountKey;
		return cs;
	}

	/// <summary>
	/// Create connection string for Azure Blob with Azure AD
	/// </summary>
	public static ConnectionString CreateConnectionStringFromAzureAd(
		string accountName,
		string tenantId,
		string applicationId,
		string applicationSecret) {
		var cs = new ConnectionString(ConnectionStringPrefix.AzureBlobStorage);
		cs.Parameters[ConnectionStringParam.AccountName] = accountName;
		cs.Parameters[ConnectionStringParam.TenantId] = tenantId;
		cs.Parameters[ConnectionStringParam.ClientId] = applicationId;
		cs.Parameters[ConnectionStringParam.ClientSecret] = applicationSecret;
		return cs;
	}

}
using Azure.Identity;
using Azure.Storage.Blobs;
using Azure.Storage.Files.Shares;
using FluentStorage.Azure;
using FluentStorage.Azure.Blobs;

namespace FluentStorage.Tests.Unit.Utils;

public class AzureRegionTest {
	private const string Account = "testaccount";

	[Theory]
	[InlineData(AzureCloudEnvironment.Global)]
	[InlineData(AzureCloudEnvironment.China)]
	[InlineData(AzureCloudEnvironment.USGovernment)]
	public void Blob_Factory_methods_use_correct_cloud_endpoint(AzureCloudEnvironment environment) {
		var endpoint = AzureCloudEndpoints.GetBlobEndpoint(environment);
		var authorityHost = AzureCloudEndpoints.GetAuthorityEndpoint(environment);
		var expectedHost = $"{Account}.blob.{endpoint}";

		// Use a valid base64 key so StorageSharedKeyCredential does not throw
		string validBase64Key = Convert.ToBase64String(new byte[32]);

		// Shared key
		IAzureBlobStore blobSharedKey = AzureBlobStorage.FromSharedKey(Account, validBase64Key, serviceUri: null, cloudEnvironment: environment);
		var client = GetBlobServiceClient(blobSharedKey);
		Assert.Equal(expectedHost, client.Uri.Host);

		// Token credential
		var tokenCred = new ClientSecretCredential("test-tenant", "test-application", "test-secret", new TokenCredentialOptions { AuthorityHost = authorityHost });

		IAzureBlobStore blobToken = AzureBlobStorage.FromTokenCredential(Account, tokenCred, environment);
		var client2 = GetBlobServiceClient(blobToken);
		Assert.Equal(expectedHost, client2.Uri.Host);

		// Managed identity
		IAzureBlobStore blobMsi = AzureBlobStorage.FromMsi(Account, clientId: null, azureCloudEnvironment: environment);
		var client3 = GetBlobServiceClient(blobMsi);
		Assert.Equal(expectedHost, client3.Uri.Host);

		// Azure Ad
		IAzureBlobStore blobAzureAd = AzureBlobStorage.FromAzureAd(
			Account,
			tenantId: "test-tenant",
			applicationId: "test-application",
			applicationSecret: "test-secret",
			cloudEnvironment: environment);
		var client4 = GetBlobServiceClient(blobAzureAd);
		Assert.Equal(expectedHost, client4.Uri.Host);
	}

	[Theory]
	[InlineData(AzureCloudEnvironment.Global)]
	[InlineData(AzureCloudEnvironment.China)]
	[InlineData(AzureCloudEnvironment.USGovernment)]
	public void Files_Factory_methods_use_correct_cloud_endpoint(AzureCloudEnvironment environment) {
		var endpoint = AzureCloudEndpoints.GetBlobEndpoint(environment);
		var authorityHost = AzureCloudEndpoints.GetAuthorityEndpoint(environment);
		var expectedHost = $"{Account}.file.{endpoint}";

		string validBase64Key = Convert.ToBase64String(new byte[32]);

		IStore filesSharedKey = AzureFilesStorage.FromSharedKey(Account, validBase64Key, serviceUri: null, cloudEnvironment: environment);
		var client = GetShareServiceClient(filesSharedKey);
		Assert.Equal(expectedHost, client.Uri.Host);

		var tokenCred = new ClientSecretCredential("test-tenant", "test-application", "test-secret", new TokenCredentialOptions { AuthorityHost = authorityHost });

		IStore filesToken = AzureFilesStorage.FromTokenCredential(Account, tokenCred, environment);
		var client2 = GetShareServiceClient(filesToken);
		Assert.Equal(expectedHost, client2.Uri.Host);

		IStore filesMsi = AzureFilesStorage.FromMsi(Account, clientId: null, azureCloudEnvironment: environment);
		var client3 = GetShareServiceClient(filesMsi);
		Assert.Equal(expectedHost, client3.Uri.Host);

		IStore filesAzureAd = AzureFilesStorage.FromAzureAd(
			Account,
			tenantId: "test-tenant",
			applicationId: "test-application",
			applicationSecret: "test-secret",
			cloudEnvironment: environment);
		var client4 = GetShareServiceClient(filesAzureAd);
		Assert.Equal(expectedHost, client4.Uri.Host);
	}

	[Theory]
	[InlineData("azure.file://account=testaccount;key=AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA=")]
	[InlineData("azure.file://account=testaccount;tenantId=test-tenant;principalId=test-application;principalSecret=test-secret")]
	[InlineData("azure.file://account=testaccount;msi")]
	public void Files_connection_string_authentication_modes_construct_storage(string connectionString) {
		AzureFilesStorage.Use();

		IStore storage = StorageFactory.FromConnectionString(connectionString);

		Assert.NotNull(storage);
		var client = GetShareServiceClient(storage);
		Assert.Equal($"{Account}.file.core.windows.net", client.Uri.Host);
	}

	private static BlobServiceClient GetBlobServiceClient(object storageInstance) {
		ArgumentNullException.ThrowIfNull(storageInstance);

		FieldInfo fi = storageInstance.GetType().GetField("_client", BindingFlags.NonPublic | BindingFlags.Instance) ?? (storageInstance.GetType().BaseType?.GetField("_client", BindingFlags.NonPublic | BindingFlags.Instance));

		if (fi is null) {
			throw new InvalidOperationException("Could not find _client field on storage instance.");
		}

		if (fi.GetValue(storageInstance) is not BlobServiceClient client) {
			throw new InvalidOperationException("_client field is not a BlobServiceClient.");
		}

		return client;
	}

	private static ShareServiceClient GetShareServiceClient(object storageInstance) {
		ArgumentNullException.ThrowIfNull(storageInstance);

		FieldInfo fi = storageInstance.GetType().GetField("_client", BindingFlags.NonPublic | BindingFlags.Instance) ?? (storageInstance.GetType().BaseType?.GetField("_client", BindingFlags.NonPublic | BindingFlags.Instance));

		if (fi is null) {
			throw new InvalidOperationException("Could not find _client field on storage instance.");
		}

		if (fi.GetValue(storageInstance) is not ShareServiceClient client) {
			throw new InvalidOperationException("_client field is not a ShareServiceClient.");
		}

		return client;
	}
}
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Azure.Core.Pipeline;
using Azure.Storage;
using Azure.Storage.Blobs;
using FluentStorage.Azure.Blobs.DataLake.Model;
using FluentStorage.Azure.Blobs.Storage;
using FluentStorage.Azure.Blobs.Utils;
using FluentStorage.Model;
using FluentStorage.Storage;
using FluentStorage.Utils.Objects;

namespace FluentStorage.Azure.Blobs.DataLake;

public class AzureDataLakeStore : AzureBlobStore, IAzureDataLakeStore {
	private readonly ExtendedSdk _extended;

	public AzureDataLakeStore(BlobServiceClient client, string accountName, StorageSharedKeyCredential sasSigningCredentials = null, string containerName = null, AzureCloudEnvironment azureCloudEnvironment = default) : base(client, accountName, sasSigningCredentials, containerName) {
		_extended = new ExtendedSdk(client, accountName, azureCloudEnvironment);


		// Fix #41: `ExtendedSdk.GetHttpPipeline` needs to be manually set otherwise connection to DataLake Gen2 fails

		// get `client.ClientConfiguration.Pipeline`
		var config = Reflections.GetProp(client, "_clientConfiguration", false);
		var pipeline = Reflections.GetPropTyped<HttpPipeline>(config, "Pipeline", false);

		// link the pipeline to the client
		Reflections.SetProp(_extended, "_httpPipeline", pipeline);

	}

	/// <summary>
	/// Returns the ExtendedSdk instance for this store.
	/// </summary>
	public override async Task<object> GetClient() {
		return _extended;
	}

	public Task<List<Filesystem>> ListFilesystems(CancellationToken cancellationToken = default) {
		return _extended.ListFilesystemsAsync(cancellationToken);
	}

	public Task CreateFilesystem(string filesystemName, CancellationToken cancellationToken = default) {
		return _extended.CreateFilesystemAsync(filesystemName, cancellationToken);
	}

	public Task DeleteFilesystem(string filesystemName, CancellationToken cancellationToken = default) {
		return _extended.DeleteFilesystemAsync(filesystemName, cancellationToken);
	}

	public Task SetAccessControl(string fullPath, AccessControl accessControl, CancellationToken cancellationToken = default) {
		return _extended.SetAccessControlAsync(fullPath, accessControl, cancellationToken);
	}

	public Task<AccessControl> GetAccessControl(string fullPath, bool getUpn = false, CancellationToken cancellationToken = default) {
		return _extended.GetAccessControlAsync(fullPath, getUpn, cancellationToken);
	}


	protected override Task DeleteObjects(string fullPath, CancellationToken cancellationToken = default) {
		return _extended.DeleteAsync(fullPath, cancellationToken);
	}

	public override Task<List<StoreObject>> ListObjects(
		StorageListOptions options, CancellationToken cancellationToken = default) {
		return _extended.ListAsync(options, cancellationToken);
	}

	public override Task<StoreObject> GetObjectInfo(string fullPath, CancellationToken cancellationToken = default) {
		return _extended.GetBlobAsync(fullPath, cancellationToken);
	}

}
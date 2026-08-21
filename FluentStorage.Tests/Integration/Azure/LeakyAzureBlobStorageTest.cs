using System.Net.Http;
using FluentStorage.Azure.Blobs;
using FluentStorage.Azure.Blobs.Policy;
using FluentStorage.Exceptions;

namespace FluentStorage.Tests.Integration.Azure;

public class LeakyAzureBlobStorageTest {
	private readonly IAzureBlobStore _native;

	public LeakyAzureBlobStorageTest() {
		TestConfig settings = TestConfigLoader.Config;

		IStore storage = AzureBlobStorage.FromSharedKey(
			settings.AzureStorageName, settings.AzureStorageKey);
		_native = (IAzureBlobStore)storage;
	}

	[Fact]
	public async Task Sas_Account() {

		var policy = new AccountSasPolicy(DateTime.UtcNow, TimeSpan.FromHours(1));
		policy.Permissions =
			AccountSasPermission.List |
			AccountSasPermission.Read |
			AccountSasPermission.Write;
		string sas = await _native.GetStorageSas(policy);
		Assert.NotNull(sas);

		//check we can connect and list containers
		IStore sasInstance = AzureBlobStorage.FromSas(sas);
		List<StoreObject> containers = await sasInstance.ListDirectory("/");
		Assert.True(containers.Count > 0);
	}

	/*[Fact]
	public async Task Sas_Container()
	{
	   string fileName = Guid.NewGuid().ToString() + ".containersas.txt";
	   string filePath = StoragePath.Combine("test", fileName);
	   await _native.WriteTextAsync(filePath, "whack!");

	   var policy = new ContainerSasPolicy(DateTime.UtcNow, TimeSpan.FromHours(1));
	   string sas = await _native.GetContainerSasAsync("test", policy, true);

	   //check we can connect and list test file in the root
	   IBlobStorage sasInstance = AzureBlobStorage.FromSas(sas);
	   List<Blob> blobs = await sasInstance.ListAsync(StoragePath.RootFolderPath);
	   Blob testBlob = blobs.FirstOrDefault(b => b.FullPath == fileName);
	   Assert.NotNull(testBlob);
	}*/

	[Fact]
	public async Task ContainerPublicAccess() {
		//make sure container exists
		await _native.SetText("test/one", "test");
		await _native.SetContainerPublicAccess("test", ContainerPublicAccessType.Off);

		ContainerPublicAccessType pa = await _native.GetContainerPublicAccess("test");
		Assert.Equal(ContainerPublicAccessType.Off, pa);   //it's off by default

		//set to public
		await _native.SetContainerPublicAccess("test", ContainerPublicAccessType.Container);
		pa = await _native.GetContainerPublicAccess("test");
		Assert.Equal(ContainerPublicAccessType.Container, pa);
	}

	[Fact]
	public async Task Sas_BlobPublicAccess() {
		string path = StoragePath.Combine("test", Guid.NewGuid().ToString() + ".txt");

		await _native.SetText(path, "read me!");

		var policy = new BlobSasPolicy(DateTime.UtcNow, TimeSpan.FromHours(12)) {
			Permissions = BlobSasPermission.Read | BlobSasPermission.Write
		};

		string publicUrl = await _native.GetBlobSas(path);

		Assert.NotNull(publicUrl);

		string text = await new HttpClient().GetStringAsync(publicUrl);
		Assert.Equal("read me!", text);
	}

	[Fact]
	public async Task Lease_CanAcquireAndRelease() {
		string id = $"test/{nameof(Lease_CanAcquireAndRelease)}.lck";

		await _native.BreakLease(id, true);

		using (AzureStorageLease lease = await _native.AcquireLease(id, TimeSpan.FromSeconds(20))) {

		}
	}

	[Fact]
	public async Task Lease_Break() {
		string id = $"test/{nameof(Lease_Break)}.lck";

		await _native.BreakLease(id, true);

		await _native.AcquireLease(id, TimeSpan.FromSeconds(20));

		await _native.BreakLease(id);
	}

	[Fact]
	public async Task Lease_FailsOnAcquiredLeasedBlob() {
		string id = $"test/{nameof(Lease_FailsOnAcquiredLeasedBlob)}.lck";

		await _native.BreakLease(id, true);

		using (AzureStorageLease lease1 = await _native.AcquireLease(id, TimeSpan.FromSeconds(20))) {
			await Assert.ThrowsAsync<StorageException>(() => _native.AcquireLease(id, TimeSpan.FromSeconds(20)));
		}
	}

	[Fact]
	public async Task Lease_WaitsToReleaseAcquiredLease() {
		string id = $"test/{nameof(Lease_WaitsToReleaseAcquiredLease)}.lck";

		await _native.BreakLease(id, true);

		using (AzureStorageLease lease1 = await _native.AcquireLease(id, TimeSpan.FromSeconds(20))) {
			await _native.AcquireLease(id, TimeSpan.FromSeconds(20), null, true);
		}
	}

	[Fact]
	public async Task Lease_Container_CanAcquireAndRelease() {
		string id = "test";

		await _native.BreakLease(id, true);

		using (AzureStorageLease lease = await _native.AcquireLease(id, TimeSpan.FromSeconds(15))) {

		}
	}

	[Fact]
	public async Task Lease_Container_Break() {
		string id = "test";

		await _native.BreakLease(id, true);

		await _native.AcquireLease(id, TimeSpan.FromSeconds(15));

		await _native.BreakLease(id);
	}

	[Fact]
	public async Task Top_level_folders_are_containers() {
		List<StoreObject> containers = await _native.ListObjects();

		foreach (StoreObject container in containers) {
			Assert.Equal(StorageObjectType.Folder, container.Type);
			Assert.True(container.Properties?.ContainsKey("IsContainer"), "isContainer property not present at all");
			Assert.Equal(true, container.Properties["IsContainer"]);
		}
	}

	[Fact]
	public async Task Delete_container() {
		string containerName = Guid.NewGuid().ToString();
		await _native.SetText($"{containerName}/test.txt", "test");

		List<StoreObject> containers = await _native.ListObjects();
		Assert.Contains(containers, c => c.Name == containerName);

		await _native.DeleteObject(containerName);
		containers = await _native.ListObjects();
		Assert.DoesNotContain(containers, c => c.Name == containerName);
	}


	[Fact]
	public async Task OpenWrite_CanSendFileInChunks() {
		// Arrange
		string containerName = Guid.NewGuid().ToString();
		var expectedContents = new string('x', 50000);
		using var sourceStream = new MemoryStream(Encoding.UTF8.GetBytes(expectedContents));

		var buffer = new byte[256];
		// Act
		using (var targetStream = await _native.OpenWrite($"{containerName}/test.txt", true))
		{
			int bytesRead = 0;
			do
			{
				bytesRead = await sourceStream.ReadAsync(buffer, 0, buffer.Length, CancellationToken.None);
				await targetStream.WriteAsync(buffer, 0, bytesRead);
			} while (bytesRead > 0);
		}

		// Assert
		var actualContents = await _native.GetText($"{containerName}/test.txt");
		Assert.Equal(expectedContents, actualContents);
	}

	/*[Fact]
	public async Task Analytics_has_logs_container()
	{
	   List<Blob> containers = await _native.ListAsync();
	   Assert.Contains(containers, c => c.Name == "$logs");
	}*/

	/*[Fact]
	public async Task Snapshots_create()
	{
	   string path = "test/test.txt";

	   await _native.WriteTextAsync(path, "test");

	   Blob snapshot = await _native.CreateSnapshotAsync(path);

	   Assert.NotNull(snapshot);
	}*/
}
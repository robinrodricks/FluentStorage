using Storage.Net.Blobs;
using Storage.Net.Microsoft.Azure.Storage.Blobs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Xunit;

namespace Storage.Net.Tests.Integration.Azure
{
   [Trait("Category", "Blobs")]
   public class LeakyAzureBlobStorageTest
   {
      private readonly IAzureBlobStorage _native;

      public LeakyAzureBlobStorageTest()
      {
         ITestSettings settings = Settings.Instance;

         IBlobStorage storage = StorageFactory.Blobs.AzureBlobStorage(settings.AzureStorageName, settings.AzureStorageKey);
         _native = (IAzureBlobStorage)storage;
      }

      [Fact]
      public async Task GetAccountSas()
      {
         var policy = new AccountSasPolicy(DateTime.UtcNow, TimeSpan.FromHours(1));
         policy.Permissions =
            AccountSasPermission.List |
            AccountSasPermission.Read |
            AccountSasPermission.Write;
         string sas = await _native.GetStorageSasAsync(policy);
         Assert.NotNull(sas);

         //check we can connect and list containers
         IBlobStorage sasInstance = StorageFactory.Blobs.AzureBlobStorageFromSas(sas);
         IReadOnlyCollection<Blob> containers = await sasInstance.ListAsync(StoragePath.RootFolderPath);
         Assert.True(containers.Count > 0);
      }

      [Fact]
      public async Task GetContainerSas()
      {
         string fileName = Guid.NewGuid().ToString() + ".containersas.txt";
         string filePath = StoragePath.Combine("test", fileName);
         await _native.WriteTextAsync(filePath, "whack!");

         var policy = new ContainerSasPolicy(DateTime.UtcNow, TimeSpan.FromHours(1));
         string sas = await _native.GetContainerSasAsync("test", policy, true);

         //check we can connect and list test file in the root
         IBlobStorage sasInstance = StorageFactory.Blobs.AzureBlobStorageFromSas(sas);
         IReadOnlyCollection<Blob> blobs = await sasInstance.ListAsync(StoragePath.RootFolderPath);
         Blob testBlob = blobs.FirstOrDefault(b => b.FullPath == fileName);
         Assert.NotNull(testBlob);
      }

      [Fact]
      public async Task Lease_CanAcquireAndRelease()
      {
         string id = $"test/{nameof(Lease_CanAcquireAndRelease)}.lck";

         using(BlobLease lease = await _native.AcquireBlobLeaseAsync(id, TimeSpan.FromSeconds(20)))
         {
            
         }
      }

      [Fact]
      public async Task Lease_FailsOnAcquiredLeasedBlob()
      {
         string id = $"test/{nameof(Lease_FailsOnAcquiredLeasedBlob)}.lck";

         using(BlobLease lease1 = await _native.AcquireBlobLeaseAsync(id, TimeSpan.FromSeconds(20)))
         {
            await Assert.ThrowsAsync<StorageException>(() => _native.AcquireBlobLeaseAsync(id, TimeSpan.FromSeconds(20)));
         }
      }

      [Fact]
      public async Task Lease_WaitsToReleaseAcquiredLease()
      {
         string id = $"test/{nameof(Lease_WaitsToReleaseAcquiredLease)}.lck";

         using(BlobLease lease1 = await _native.AcquireBlobLeaseAsync(id, TimeSpan.FromSeconds(20)))
         {
            await _native.AcquireBlobLeaseAsync(id, TimeSpan.FromSeconds(20), true);
         }
      }

      [Fact]
      public async Task Top_level_folders_are_containers()
      {
         IReadOnlyCollection<Blob> containers = await _native.ListAsync();

         foreach(Blob container in containers)
         {
            Assert.Equal(BlobItemKind.Folder, container.Kind);
            Assert.True(container.Properties?.ContainsKey("IsContainer"), "isContainer property not present at all");
            Assert.Equal("True", container.Properties["IsContainer"]);
         }
      }
   }
}
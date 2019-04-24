using Storage.Net.Blob;
using Storage.Net.Microsoft.Azure.Storage.Blob;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Storage.Net.Tests.Integration.Azure
{
   public class LeakyAzureBlobStorageTest
   {
      private readonly IBlobStorage _storage;
      private readonly IAzureBlobStorageNativeOperations _native;

      public LeakyAzureBlobStorageTest()
      {
         ITestSettings settings = Settings.Instance;

         _storage = StorageFactory.Blobs.AzureBlobStorage(settings.AzureStorageName, settings.AzureStorageKey);
         _native = (IAzureBlobStorageNativeOperations)_storage;
      }

      [Fact]
      public async Task GetReadOnlySasUriAsync()
      {
         string id = "test/single.txt";
         await _storage.WriteTextAsync(id, "test");

         string uri = await _native.GetReadOnlySasUriAsync(id);
         string content = await new WebClient().DownloadStringTaskAsync(new Uri(uri));
         Assert.Equal("test", content);
      }


#if DEBUG
      //[Fact]
      public async Task Read_large_fileAsync()
      {
         IBlobStorage blobsGeneric = StorageFactory.Blobs.AzureBlobStorage("", "");

         var blobsAzure = (IAzureBlobStorageNativeOperations)blobsGeneric;

         Stream s = await blobsAzure.OpenRandomAccessReadAsync("large/bee1.zip");

         //s.Seek(-(s.Length - 20), SeekOrigin.End);
         byte[] data = new byte[100];
         int read = s.Read(data, 0, 20);
      }
#endif
   }
}
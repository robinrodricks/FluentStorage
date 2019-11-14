using System;
using Azure.Storage;
using Azure.Storage.Blobs;
using Storage.Net.Microsoft.Azure.Storage.Blobs;

namespace Storage.Net
{
   public static class Factory
   {
      public static IAzureBlobStorage12 AzureBlob12Storage(this IBlobStorageFactory factory,
         string accountName,
         string key,
         Uri serviceUri = null)
      {
         if(accountName is null)
            throw new ArgumentNullException(nameof(accountName));
         if(key is null)
            throw new ArgumentNullException(nameof(key));

         var credential = new StorageSharedKeyCredential(accountName, key);

         var client = new BlobServiceClient(serviceUri ?? GetServiceUri(accountName), credential);

         return new AzureBlobStorage(client);
      }

      private static Uri GetServiceUri(string accountName)
      {
         return new Uri($"https://{accountName}.blob.core.windows.net/");
      }
   }
}

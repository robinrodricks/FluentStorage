using System;
using Azure.Storage;
using Azure.Storage.Blobs;
using Storage.Net.Microsoft.Azure.Storage.Blobs;

namespace Storage.Net
{
   /// <summary>
   /// Blob storage factory
   /// </summary>
   public static class Factory
   {
      /// <summary>
      /// 
      /// </summary>
      public static IAzureBlobStorage AzureBlobStorage(this IBlobStorageFactory factory,
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

         return new AzureBlobStorage(client, accountName);
      }

      /// <summary>
      /// 
      /// </summary>
      public static IAzureDataLakeStorage AzureDataLakeStorage(this IBlobStorageFactory factory,
         string accountName,
         string key,
         Uri serviceUri = null)
      {
         return (IAzureDataLakeStorage)factory.AzureBlobStorage(accountName, key, serviceUri);
      }


      /*
      /// <summary>
      /// Creates a blob storage implementation based on Microsoft Azure Blob Storage using account name and key.
      /// </summary>
      /// <param name="factory">Reference to factory</param>
      /// <param name="accountName">Storage Account name</param>
      /// <param name="key">Storage Account key</param>
      /// <returns>Generic blob storage interface</returns>
      public static IAzureBlobStorage AzureBlobStorage(this IBlobStorageFactory factory,
         string accountName,
         string key)
      {
         return AzureUniversalBlobStorageProvider.CreateFromAccountNameAndKey(accountName, key);
      }

      /// <summary>
      /// Creates a blob storage implementation using Shared Access Signature.
      /// </summary>
      /// <param name="factory">Reference to factory</param>
      /// <param name="sasUrl"></param>
      /// <returns></returns>
      public static IAzureBlobStorage AzureBlobStorageFromSas(this IBlobStorageFactory factory,
         string sasUrl)
      {
         return AzureUniversalBlobStorageProvider.CreateFromSasUrl(sasUrl);
      }

      /// <summary>
      /// Creates a blob storage implementation using a bearer token
      /// </summary>
      /// <param name="factory"></param>
      /// <param name="accountName"></param>
      /// <param name="token"></param>
      /// <returns></returns>
      public static IAzureBlobStorage AzureBlobStorageFromAadToken(this IBlobStorageFactory factory,
         string accountName,
         string token)
      {
         return AzureUniversalBlobStorageProvider.CreateWithAadToken(accountName, token);
      }



      /// <summary>
      /// Creates a blob storage implementation based on Microsoft Azure Blob Storage using development storage.
      /// </summary>
      /// <param name="factory">Reference to factory</param>
      /// <returns>Generic blob storage interface</returns>
      public static IAzureBlobStorage AzureBlobDevelopmentStorage(this IBlobStorageFactory factory)
      {
         return AzureUniversalBlobStorageProvider.CreateForLocalEmulator();
      }

      /// <summary>
      /// Creates an instance of Microsoft Azure Blob Storage that wraps around native <see cref="CloudBlobClient"/>.
      /// Avoid using if possible as it's a subject to change in future.
      /// </summary>
      public static IAzureBlobStorage AzureBlobStorage(this IBlobStorageFactory factory, CloudBlobClient cloudBlobClient)
      {
         if(cloudBlobClient == null)
            throw new ArgumentNullException(nameof(cloudBlobClient));

         return new AzureUniversalBlobStorageProvider(cloudBlobClient, null);
      }
      */

      private static Uri GetServiceUri(string accountName)
      {
         return new Uri($"https://{accountName}.blob.core.windows.net/");
      }
   }
}

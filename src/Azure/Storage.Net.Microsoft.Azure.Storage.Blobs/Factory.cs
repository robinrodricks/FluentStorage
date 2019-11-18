using System;
using Azure.Core;
using Azure.Identity;
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
      public static IAzureBlobStorage AzureBlobStorageWithSharedKey(this IBlobStorageFactory factory,
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
      /// Create Azure Blob Storage with AAD authentication
      /// </summary>
      /// <param name="factory"></param>
      /// <param name="accountName"></param>
      /// <param name="tenantId"></param>
      /// <param name="applicationId"></param>
      /// <param name="applicationSecret"></param>
      /// <param name="activeDirectoryAuthEndpoint"></param>
      /// <returns></returns>
      public static IAzureBlobStorage AzureBlobStorageWithAzureAd(this IBlobStorageFactory factory,
         string accountName,
         string tenantId,
         string applicationId,
         string applicationSecret,
         string activeDirectoryAuthEndpoint = "https://login.microsoftonline.com/")
      {
         if(accountName is null)
            throw new ArgumentNullException(nameof(accountName));
         if(tenantId is null)
            throw new ArgumentNullException(nameof(tenantId));
         if(applicationId is null)
            throw new ArgumentNullException(nameof(applicationId));
         if(applicationSecret is null)
            throw new ArgumentNullException(nameof(applicationSecret));
         if(activeDirectoryAuthEndpoint is null)
            throw new ArgumentNullException(nameof(activeDirectoryAuthEndpoint));

         // Create a token credential that can use our Azure Active
         // Directory application to authenticate with Azure Storage
         TokenCredential credential =
             new ClientSecretCredential(
                 tenantId,
                 applicationId,
                 applicationSecret,
                 new TokenCredentialOptions() { AuthorityHost = new Uri(activeDirectoryAuthEndpoint) });

         // Create a client that can authenticate using our token credential
         var client = new BlobServiceClient(GetServiceUri(accountName), credential);

         return new AzureBlobStorage(client, accountName);
      }

      /// <summary>
      /// Creates Azure Blob Storage with Managed Identity
      /// </summary>
      /// <param name="factory"></param>
      /// <param name="accountName"></param>
      /// <param name="clientId"></param>
      /// <returns></returns>
      public static IAzureBlobStorage AzureBlobStorageWithMsi(this IBlobStorageFactory factory,
         string accountName,
         string clientId = null)
      {
         TokenCredential credential = new ManagedIdentityCredential(clientId, null);

         var client = new BlobServiceClient(GetServiceUri(accountName), credential);

         return new AzureBlobStorage(client, accountName);
      }

      /// <summary>
      /// 
      /// </summary>
      public static IAzureDataLakeStorage AzureDataLakeStorageWithSharedKey(this IBlobStorageFactory factory,
         string accountName,
         string key,
         Uri serviceUri = null)
      {
         return (IAzureDataLakeStorage)factory.AzureBlobStorageWithSharedKey(accountName, key, serviceUri);
      }

      /// <summary>
      /// Create Azure Data Lake Gen 2 Storage with AAD authentication
      /// </summary>
      /// <param name="factory"></param>
      /// <param name="accountName"></param>
      /// <param name="tenantId"></param>
      /// <param name="applicationId"></param>
      /// <param name="applicationSecret"></param>
      /// <param name="activeDirectoryAuthEndpoint"></param>
      /// <returns></returns>
      public static IAzureDataLakeStorage AzureDataLakeStorageWithAzureAd(this IBlobStorageFactory factory,
         string accountName,
         string tenantId,
         string applicationId,
         string applicationSecret,
         string activeDirectoryAuthEndpoint = "https://login.microsoftonline.com/")
      {
         return (IAzureDataLakeStorage)factory.AzureBlobStorageWithAzureAd(
            accountName,
            tenantId,
            applicationId,
            applicationSecret,
            activeDirectoryAuthEndpoint);
      }

      /// <summary>
      /// Creates Azure Data Lake Gen 2 Storage with Managed Identity
      /// </summary>
      /// <param name="factory"></param>
      /// <param name="accountName"></param>
      /// <param name="clientId"></param>
      /// <returns></returns>
      public static IAzureDataLakeStorage AzureDataLakeStorageWithMsi(this IBlobStorageFactory factory,
         string accountName,
         string clientId = null)
      {
         return (IAzureDataLakeStorage)factory.AzureBlobStorageWithMsi(accountName, clientId);
      }


      /*


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
      /// Creates a blob storage implementation based on Microsoft Azure Blob Storage using development storage.
      /// </summary>
      /// <param name="factory">Reference to factory</param>
      /// <returns>Generic blob storage interface</returns>
      public static IAzureBlobStorage AzureBlobDevelopmentStorage(this IBlobStorageFactory factory)
      {
         return AzureUniversalBlobStorageProvider.CreateForLocalEmulator();
      }
      }
      */

      private static Uri GetServiceUri(string accountName)
      {
         return new Uri($"https://{accountName}.blob.core.windows.net/");
      }
   }
}

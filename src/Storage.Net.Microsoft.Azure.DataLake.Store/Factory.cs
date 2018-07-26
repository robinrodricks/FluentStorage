using Storage.Net.Blob;
using Storage.Net.Microsoft.Azure.DataLake.Store;
using Storage.Net.Microsoft.Azure.DataLake.Store.Blob;
using System;
using System.Net;

namespace Storage.Net
{
   /// <summary>
   /// Factory class that implement factory methods for Microsoft Azure implememtations
   /// </summary>
   public static class Factory
   {
      public static IModulesFactory UseAzureDataLakeStore(this IModulesFactory factory)
      {
         return factory.Use(new ExternalModule());
      }

      /// <summary>
      /// Creates and instance of Azure Data Lake Store client
      /// </summary>
      /// <param name="factory">Factory reference</param>
      /// <param name="accountName">Data Lake account name</param>
      /// <param name="credential">Credential object where username is Principal ID and password is Principal Secret, and Domain is set to Tenant ID</param>
      /// <returns></returns>
      public static IBlobStorage AzureDataLakeStoreByClientSecret(this IBlobStorageFactory factory,
         string accountName,
         NetworkCredential credential)
      {
         return AzureDataLakeStoreBlobStorageProvider.CreateByClientSecret(accountName, credential);
      }

      /// <summary>
      /// Creates and instance of Azure Data Lake Store client
      /// </summary>
      /// <param name="factory">Factory reference</param>
      /// <param name="accountName">Data Lake account name</param>
      /// <param name="tenantId">Tenant ID</param>
      /// <param name="principalId">Principal ID</param>
      /// <param name="principalSecret">Principal Secret</param>
      /// <returns></returns>
      public static IBlobStorage AzureDataLakeStoreByClientSecret(this IBlobStorageFactory factory,
         string accountName,
         string tenantId,
         string principalId,
         string principalSecret)
      {
         if (accountName == null)
            throw new ArgumentNullException(nameof(accountName));

         if (tenantId == null)
            throw new ArgumentNullException(nameof(tenantId));

         if (principalId == null)
            throw new ArgumentNullException(nameof(principalId));

         if (principalSecret == null)
            throw new ArgumentNullException(nameof(principalSecret));

         return AzureDataLakeStoreBlobStorageProvider.CreateByClientSecret(accountName, new NetworkCredential(principalId, principalSecret, tenantId));
      }
   }
}

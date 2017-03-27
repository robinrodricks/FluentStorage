using Storage.Net.Blob;
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
      public static IBlobStorage AzureDataLakeStoreByClientSecret(this IBlobStorageFactory factory,
         string accountName,
         NetworkCredential credential)
      {
         return DataLakeStoreBlobStorage.CreateByClientSecret(accountName, credential);
      }

      /*public static IBlobStorage AzureDataLakeStoreByClientSecret(this IBlobStorageFactory factory,
         string domain,
         string clientId,
         string clientSecret)
      {
         //return new DataLakeStoreBlobStorage();
      }*/
   }
}

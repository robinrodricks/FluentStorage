using System.Text;
using Storage.Net.Blobs;
using Storage.Net.ConnectionString;
using Storage.Net.KeyValue;
using Storage.Net.Messaging;

namespace Storage.Net.Microsoft.Azure.Storage.Blobs
{
   class Module : IExternalModule, IConnectionFactory
   {
      public IConnectionFactory ConnectionFactory => this;

      public IBlobStorage CreateBlobStorage(StorageConnectionString connectionString)
      {
         if(connectionString.Prefix == KnownPrefix.AzureBlobStorage)
         {
            connectionString.GetRequired(KnownParameter.AccountName, true, out string accountName);

            string sharedKey = connectionString.Get(KnownParameter.KeyOrPassword);
            if(!string.IsNullOrEmpty(sharedKey))
            {
               return StorageFactory.Blobs.AzureBlobStorageWithSharedKey(accountName, sharedKey);
            }
         }
         else if(connectionString.Prefix == KnownPrefix.AzureDataLakeGen2 || connectionString.Prefix == KnownPrefix.AzureDataLake)
         {
            connectionString.GetRequired(KnownParameter.AccountName, true, out string accountName);

            string sharedKey = connectionString.Get(KnownParameter.KeyOrPassword);
            if(!string.IsNullOrEmpty(sharedKey))
            {
               return StorageFactory.Blobs.AzureDataLakeStorageWithSharedKey(accountName, sharedKey);
            }

            string tenantId = connectionString.Get(KnownParameter.TenantId);
            if(!string.IsNullOrEmpty(tenantId))
            {
               connectionString.GetRequired(KnownParameter.ClientId, true, out string clientId);
               connectionString.GetRequired(KnownParameter.ClientSecret, true, out string clientSecret);

               return StorageFactory.Blobs.AzureDataLakeStorageWithAzureAd(accountName, tenantId, clientId, clientSecret);
            }
         }

         return null;
      }

      public IKeyValueStorage CreateKeyValueStorage(StorageConnectionString connectionString) => null;
      public IMessenger CreateMessenger(StorageConnectionString connectionString) => null;
   }
}

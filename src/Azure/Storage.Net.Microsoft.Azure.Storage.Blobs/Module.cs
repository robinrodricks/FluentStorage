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

         return null;
      }

      public IKeyValueStorage CreateKeyValueStorage(StorageConnectionString connectionString) => null;
      public IMessenger CreateMessenger(StorageConnectionString connectionString) => null;
   }
}

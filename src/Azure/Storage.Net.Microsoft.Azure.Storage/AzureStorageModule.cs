using Storage.Net.Blobs;
using Storage.Net.ConnectionString;
using Storage.Net.KeyValue;
using Storage.Net.Messaging;
using Storage.Net.Microsoft.Azure.Storage.Messaging;

namespace Storage.Net.Microsoft.Azure.Storage
{
   class Module : IExternalModule, IConnectionFactory
   {
      public IConnectionFactory ConnectionFactory => this;

      public IBlobStorage CreateBlobStorage(StorageConnectionString connectionString) => null;


      public IKeyValueStorage CreateKeyValueStorage(StorageConnectionString connectionString)
      {

         return null;
      }

      public IMessenger CreateMessenger(StorageConnectionString connectionString)
      {
         if(connectionString.Prefix == Constants.AzureQueueConnectionPrefix)
         {
            connectionString.GetRequired(KnownParameter.AccountName, true, out string accountName);
            connectionString.GetRequired(KnownParameter.KeyOrPassword, true, out string key);

            return new AzureStorageQueueMessenger(accountName, key);
         }

         return null;
      }
   }
}

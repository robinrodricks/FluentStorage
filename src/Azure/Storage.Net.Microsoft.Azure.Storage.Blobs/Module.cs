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

         return null;
      }

      public IKeyValueStorage CreateKeyValueStorage(StorageConnectionString connectionString) => null;
      public IMessenger CreateMessenger(StorageConnectionString connectionString) => null;
   }
}

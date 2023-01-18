using FluentStorage.Blobs;
using FluentStorage.ConnectionString;
using FluentStorage.Messaging;

namespace FluentStorage.Azure.EventHub
{
   class Module : IExternalModule, IConnectionFactory
   {
      public IConnectionFactory ConnectionFactory => this;

      public IBlobStorage CreateBlobStorage(StorageConnectionString connectionString) => null;

      public IMessenger CreateMessenger(StorageConnectionString connectionString)
      {
         if(connectionString.Prefix == KnownPrefix.AzureEventHub)
         {
            if(connectionString.IsNative)
            {
               return new AzureEventHubMessenger(connectionString.Native, null);
            }
         }

         return null;
      }
   }
}

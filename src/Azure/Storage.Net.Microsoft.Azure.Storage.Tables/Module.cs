using System;
using System.Collections.Generic;
using System.Text;
using Storage.Net.Blobs;
using Storage.Net.ConnectionString;
using Storage.Net.KeyValue;
using Storage.Net.Messaging;

namespace Storage.Net.Microsoft.Azure.Storage.Tables
{
   class Module : IExternalModule, IConnectionFactory
   {
      public IConnectionFactory ConnectionFactory => this;

      public IBlobStorage CreateBlobStorage(StorageConnectionString connectionString) => null;
      public IKeyValueStorage CreateKeyValueStorage(StorageConnectionString connectionString)
      {
         if(connectionString.Prefix == KnownPrefix.AzureTableStorage)
         {
            if(bool.TryParse(connectionString.Get(KnownParameter.UseDevelopmentStorage), out bool useDevelopment)
               && useDevelopment)
            {
               return new AzureTableStorageKeyValueStorage();
            }
            else
            {
               connectionString.GetRequired(KnownParameter.AccountName, true, out string accountName);
               connectionString.GetRequired(KnownParameter.KeyOrPassword, true, out string key);

               return new AzureTableStorageKeyValueStorage(accountName, key);
            }
         }

         return null;
      }
      public IMessenger CreateMessenger(StorageConnectionString connectionString) => null;
   }
}

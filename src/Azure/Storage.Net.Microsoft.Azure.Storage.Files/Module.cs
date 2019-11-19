using System;
using System.Collections.Generic;
using System.Text;
using Storage.Net.Blobs;
using Storage.Net.ConnectionString;
using Storage.Net.KeyValue;
using Storage.Net.Messaging;

namespace Storage.Net.Microsoft.Azure.Storage.Files
{
   class Module : IExternalModule, IConnectionFactory
   {
      public IConnectionFactory ConnectionFactory => this;

      public IBlobStorage CreateBlobStorage(StorageConnectionString connectionString)
      {
         if(connectionString.Prefix == KnownPrefix.AzureFilesStorage)
         {
            connectionString.GetRequired(KnownParameter.AccountName, true, out string accountName);
            connectionString.GetRequired(KnownParameter.KeyOrPassword, true, out string key);

            return AzureFilesBlobStorage.CreateFromAccountNameAndKey(accountName, key);
         }

         return null;
      }

      public IKeyValueStorage CreateKeyValueStorage(StorageConnectionString connectionString) => null;
      public IMessenger CreateMessenger(StorageConnectionString connectionString) => null;
   }
}

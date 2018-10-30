using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Storage.Net.Blob;
using Storage.Net.ConnectionString;
using Storage.Net.KeyValue;
using Storage.Net.Microsoft.Azure.Storage.Blob;
using Storage.Net.Microsoft.Azure.Storage.KeyValue;

namespace Storage.Net.Microsoft.Azure.Storage
{
   class ConnectionFactory : IConnectionFactory
   {
      public IBlobStorage CreateBlobStorage(StorageConnectionString connectionString)
      {
         if(connectionString.Prefix == "azure.blob")
         {
            connectionString.GetRequired("account", true, out string accountName);
            string containerName = connectionString.Get("container");
            connectionString.GetRequired("key", true, out string key);

            return new AzureUniversalBlobStorageProvider(accountName, key, containerName);
         }

         return null;
      }

      public IKeyValueStorage CreateKeyValueStorage(StorageConnectionString connectionString)
      {
         if(connectionString.Prefix == "azure.tables")
         {
            connectionString.GetRequired("account", true, out string acctountName);
            connectionString.GetRequired("key", true, out string key);

            return new AzureTableStorageKeyValueStorage(acctountName, key);
         }

         return null;
      }
   }
}

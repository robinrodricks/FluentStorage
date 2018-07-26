using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Storage.Net.Blob;
using Storage.Net.ConnectionString;
using Storage.Net.Microsoft.Azure.Storage.Blob;

namespace Storage.Net.Microsoft.Azure.Storage
{
   class ConnectionFactory : IConnectionFactory
   {
      public IBlobStorage CreateBlobStorage(StorageConnectionString connectionString)
      {
         if(connectionString.Prefix == "azure.blob")
         {
            connectionString.GetRequired("account", true, out string accountName);
            connectionString.GetRequired("container", true, out string containerName);
            connectionString.GetRequired("key", true, out string key);

            if(!bool.TryParse(connectionString.Get("createIfNotExists"), out bool createIfNotExists))
            {
               createIfNotExists = true;
            }

            return new AzureBlobStorageProvider(accountName, key, containerName, createIfNotExists);
         }

         return null;
      }
   }
}

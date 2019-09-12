using System;
using System.Collections.Generic;
using System.Text;
using Storage.Net.Blobs;
using Storage.Net.ConnectionString;
using Storage.Net.KeyValue;
using Storage.Net.Messaging;
using Storage.Net.Microsoft.Azure.KeyVault.Blobs;

namespace Storage.Net.Microsoft.Azure.KeyVault
{
   class ExternalModule : IExternalModule, IConnectionFactory
   {
      public IConnectionFactory ConnectionFactory => this;

      public IBlobStorage CreateBlobStorage(StorageConnectionString connectionString)
      {
         if(connectionString.Prefix == "azure.keyvault")
         {
            connectionString.GetRequired("vaultUri", true, out string uri);
            connectionString.GetRequired("clientId", true, out string clientId);
            connectionString.GetRequired("clientSecret", true, out string clientSecret);

            return new AzureKeyVaultBlobStorageProvider(new Uri(uri), clientId, clientSecret);
         }

         return null;
      }
      public IKeyValueStorage CreateKeyValueStorage(StorageConnectionString connectionString) => null;

      public IMessenger CreateMessenger(StorageConnectionString connectionString) => null;
   }
}

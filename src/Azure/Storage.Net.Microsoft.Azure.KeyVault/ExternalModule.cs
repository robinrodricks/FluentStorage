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
         if(connectionString.Prefix == KnownPrefix.AzureKeyVault)
         {
            connectionString.GetRequired("vaultUri", true, out string uri);

            if(connectionString.Parameters.ContainsKey("msi"))
            {
               return StorageFactory.Blobs.AzureKeyVaultWithMsi(new Uri(uri));
            }
            else
            {
               connectionString.GetRequired(KnownParameter.TenantId, true, out string tenantId);
               connectionString.GetRequired(KnownParameter.ClientId, true, out string clientId);
               connectionString.GetRequired(KnownParameter.ClientSecret, true, out string clientSecret);

               return StorageFactory.Blobs.AzureKeyVault(new Uri(uri), tenantId, clientId, clientSecret);
            }
         }

         return null;
      }
      public IKeyValueStorage CreateKeyValueStorage(StorageConnectionString connectionString) => null;

      public IMessenger CreateMessenger(StorageConnectionString connectionString) => null;
   }
}

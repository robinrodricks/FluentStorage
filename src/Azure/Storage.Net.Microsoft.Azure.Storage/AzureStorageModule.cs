using System;
using Storage.Net.Blobs;
using Storage.Net.ConnectionString;
using Storage.Net.KeyValue;
using Storage.Net.Messaging;
using Storage.Net.Microsoft.Azure.Storage.Blobs;
using Storage.Net.Microsoft.Azure.Storage.KeyValue;
using Storage.Net.Microsoft.Azure.Storage.Messaging;

namespace Storage.Net.Microsoft.Azure.Storage
{
   class Module : IExternalModule, IConnectionFactory
   {
      public IConnectionFactory ConnectionFactory => this;

      public IBlobStorage CreateBlobStorage(StorageConnectionString connectionString)
      {
         if(connectionString.Prefix == KnownPrefix.AzureBlobStorage)
         {
            if(bool.TryParse(connectionString.Get(Constants.UseDevelopmentStorage), out bool useDevelopment) && useDevelopment)
            {
               return AzureUniversalBlobStorageProvider.CreateForLocalEmulator();
            }
            else
            {
               connectionString.GetRequired(KnownParameter.AccountName, true, out string accountName);
               connectionString.GetRequired(KnownParameter.KeyOrPassword, true, out string key);

               return AzureUniversalBlobStorageProvider.CreateFromAccountNameAndKey(accountName, key);
            }
         }
         else if(connectionString.Prefix == KnownPrefix.AzureFilesStorage)
         {
            connectionString.GetRequired(KnownParameter.AccountName, true, out string accountName);
            connectionString.GetRequired(KnownParameter.KeyOrPassword, true, out string key);

            return AzureFilesBlobStorage.CreateFromAccountNameAndKey(accountName, key);
         }
         else
         {
            //try to re-parse native connection string
            var newcs = new StorageConnectionString(KnownPrefix.AzureBlobStorage + "://" + connectionString.Prefix);

            if(newcs.Parameters.TryGetValue("AccountName", out string accountName) &&
               newcs.Parameters.TryGetValue("AccountKey", out string accountKey))
            {
               return AzureUniversalBlobStorageProvider.CreateFromAccountNameAndKey(accountName, accountKey);
            }
         }

         return null;
      }

      public IKeyValueStorage CreateKeyValueStorage(StorageConnectionString connectionString)
      {
         if(connectionString.Prefix == Constants.AzureTablesConnectionPrefix)
         {
            if(bool.TryParse(connectionString.Get(Constants.UseDevelopmentStorage), out bool useDevelopment)
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

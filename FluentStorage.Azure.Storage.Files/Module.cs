using FluentStorage.Blobs;
using FluentStorage.ConnectionString;
using FluentStorage.Messaging;

namespace FluentStorage.Azure.Files
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

      public IMessenger CreateMessenger(StorageConnectionString connectionString) => null;
   }
}

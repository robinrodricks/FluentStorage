using Storage.Net.ConnectionString;
using Storage.Net.KeyValue;
using Storage.Net.Microsoft.Azure.Storage.Tables;

namespace Storage.Net
{
   /// <summary>
   /// Factory
   /// </summary>
   public static class TablesFactory
   {
      /// <summary>
      /// Register Azure module.
      /// </summary>
      public static IModulesFactory UseAzureTableStorage(this IModulesFactory factory)
      {
         return factory.Use(new Module());
      }

      /// <summary>
      /// Creates an instance of Azure Table Storage using account name and key.
      /// </summary>
      /// <param name="factory">Factory reference</param>
      /// <param name="accountName">Account name</param>
      /// <param name="storageKey">Account key</param>
      /// <returns></returns>
      public static IKeyValueStorage AzureTableStorage(this IKeyValueStorageFactory factory,
         string accountName,
         string storageKey)
      {
         return new AzureTableStorageKeyValueStorage(accountName, storageKey);
      }

      /// <summary>
      /// Create connection string for azure table storage
      /// </summary>
      public static StorageConnectionString ForAzureTableStorageWithSharedKey(this IConnectionStringFactory factory,
         string accountName,
         string accountKey)
      {
         var cs = new StorageConnectionString(KnownPrefix.AzureDataLakeGen2);
         cs.Parameters[KnownParameter.AccountName] = accountName;
         cs.Parameters[KnownParameter.KeyOrPassword] = accountKey;
         return cs;
      }
   }
}

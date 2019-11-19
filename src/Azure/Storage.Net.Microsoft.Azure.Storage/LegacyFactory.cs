using Storage.Net.Messaging;
using Storage.Net.Microsoft.Azure.Storage.Messaging;
using Storage.Net.Microsoft.Azure.Storage;

namespace Storage.Net
{
   /// <summary>
   /// Factory class that implement factory methods for Microsoft Azure implememtations
   /// </summary>
   public static class LegacyFactory
   {

      /// <summary>
      /// Register Azure module.
      /// </summary>
      public static IModulesFactory UseAzureStorage(this IModulesFactory factory)
      {
         return factory.Use(new Module());
      }

      /// <summary>
      /// Creates an instance of a publisher to Azure Storage Queues
      /// </summary>
      /// <param name="factory">Factory reference</param>
      /// <param name="accountName">Account name</param>
      /// <param name="storageKey">Storage key</param>
      /// <returns>Generic message publisher interface</returns>
      public static IMessenger AzureStorageQueue(this IMessagingFactory factory,
         string accountName,
         string storageKey)
      {
         return new AzureStorageQueueMessenger(accountName, storageKey);
      }
   }
}

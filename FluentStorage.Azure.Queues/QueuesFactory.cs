using System;
using FluentStorage.ConnectionString;
using FluentStorage.Messaging;
using FluentStorage.Azure.Queues;

namespace FluentStorage
{
   public static class QueuesFactory
   {
      /// <summary>
      /// Register Azure module.
      /// </summary>
      public static IModulesFactory UseAzureQueues(this IModulesFactory factory)
      {
         return factory.Use(new Module());
      }

      /// <summary>
      /// Creates an instance of a publisher to Azure Storage Queues
      /// </summary>
      /// <param name="factory">Factory reference</param>
      /// <param name="accountName">Account name. Must not be <see langword="null"/> or empty.</param>
      /// <param name="storageKey">Storage key. Must not be <see langword="null"/> or empty.</param>
      /// <param name="serviceUri">Alternative service uri. Pass <see langword="null"/> for default.</param>
      /// <returns>Generic message publisher interface</returns>
      public static IMessenger AzureStorageQueue(this IMessagingFactory factory,
         string accountName,
         string storageKey,
         Uri serviceUri = null)
      {
         if (serviceUri == null)
            return new AzureStorageQueueMessenger(accountName, storageKey);
         
         return new AzureStorageQueueMessenger(accountName, storageKey, serviceUri);
      }

      /// <summary>
      /// 
      /// </summary>
      /// <param name="factory"></param>
      /// <param name="accountName"></param>
      /// <param name="accountKey"></param>
      /// <returns></returns>
      public static StorageConnectionString ForAzureStorageQueuesWithSharedKey(
         this IConnectionStringFactory factory,
         string accountName,
         string accountKey)
      {
         var cs = new StorageConnectionString(KnownPrefix.AzureQueueStorage);
         cs.Parameters[KnownParameter.AccountName] = accountName;
         cs.Parameters[KnownParameter.KeyOrPassword] = accountKey;
         return cs;
      }
   }
}

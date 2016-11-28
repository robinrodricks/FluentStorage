using System;
using Storage.Net.Blob;
using Storage.Net.Messaging;
using Storage.Net.Microsoft.Azure.Blob;
using Storage.Net.Microsoft.Azure.Messaging.Storage;
using Storage.Net.Microsoft.Azure.Table;
using Storage.Net.Table;

namespace Storage.Net
{
   public static class Factory
   {
      public static ITableStorage AzureTableStorage(this ITableStorageFactory factory,
         string accountName,
         string storageKey)
      {
         return new AzureTableStorage(accountName, storageKey);
      }

      public static IBlobStorage AzureBlobStorage(this IBlobStorageFactory factory,
         string accountName,
         string key,
         string containerName)
      {
         return new AzureBlobStorage(accountName, key, containerName);
      }

      public static IMessagePublisher AzureStorageQueuePublisher(this IMessagingFactory factory,
         string accountName,
         string storageKey,
         string queueName)
      {
         return new AzureStorageQueuePublisher(accountName, storageKey, queueName);
      }

      public static IMessageReceiver AzureStorageQueueReceiver(this IMessagingFactory factory,
         string accountName,
         string storageKey,
         string queueName,
         TimeSpan messageVisibilityTimeout)
      {
         return new AzureStorageQueueReceiver(accountName, storageKey, queueName, messageVisibilityTimeout);
      }
   }
}

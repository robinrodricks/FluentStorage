using System;
using Storage.Net.Blob;
using Storage.Net.Messaging;
using Storage.Net.Microsoft.Azure.Blob;
#if NETFULL
using Storage.Net.Microsoft.Azure.Messaging.ServiceBus;
#endif
using Storage.Net.Microsoft.Azure.Messaging.Storage;
using Storage.Net.Microsoft.Azure.Table;
using Storage.Net.Table;

namespace Storage.Net
{
   /// <summary>
   /// Factory class that implement factory methods for Microsoft Azure implememtations
   /// </summary>
   public static class Factory
   {
      public static ITableStorage AzureTableStorage(this ITableStorageFactory factory,
         string accountName,
         string storageKey)
      {
         return new AzureTableStorage(accountName, storageKey);
      }

      /// <summary>
      /// Creates a blob storage implementation based on Microsoft Azure Blob Storage using account name and key.
      /// </summary>
      /// <param name="factory">Reference to factory</param>
      /// <param name="accountName">Storage Account name</param>
      /// <param name="key">Storage Account key</param>
      /// <param name="containerName">Container name in the blob storage. If the container doesn't exist it will be automatically
      /// created for you.</param>
      /// <returns>Generic blob storage interface</returns>
      public static IBlobStorage AzureBlobStorage(this IBlobStorageFactory factory,
         string accountName,
         string key,
         string containerName)
      {
         return new AzureBlobStorage(accountName, key, containerName);
      }

      /// <summary>
      /// Creates a blob storage implementation
      /// </summary>
      /// <param name="factory">Reference to factory</param>
      /// <param name="connectionString">Storage account connection string</param>
      /// <param name="containerName">Container name in the blob storage. If the container doesn't exist it will be automatically
      /// create for you.</param>
      /// <returns>Generic blob storage  interface</returns>
      public static IBlobStorage AzureBlobStorage(this IBlobStorageFactory factory,
         string connectionString,
         string containerName)
      {
         return new AzureBlobStorage(connectionString, containerName);
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

#if NETFULL
      /// <summary>
      /// Creates a new instance of Azure Service Bus Queue by connection string and queue name
      /// </summary>
      /// <param name="connectionString">Service Bus connection string</param>
      /// <param name="queueName">Queue name in Service Bus. If queue doesn't exist it will be created for you.</param>
      public static IMessagePublisher AzureServiceBusQueuePublisher(this IMessagingFactory factory,
         string connectionString,
         string queueName)
      {
         return new AzureServiceBusQueuePublisher(connectionString, queueName);
      }

      /// <summary>
      /// Creates an instance of Azure Service Bus receiver with connection
      /// </summary>
      /// <param name="connectionString">Service Bus connection string</param>
      /// <param name="queueName">Queue name in Service Bus</param>
      /// <param name="peekLock">When true listens in PeekLock mode, otherwise ReceiveAndDelete</param>
      public static IMessageReceiver AzureServiceBusQueueReceiver(this IMessagingFactory factory,
         string connectionString, string queueName, bool peekLock = true)
      {
         return new AzureServiceBusQueueReceiver(connectionString, queueName, peekLock);
      }

      /// <summary>
      /// Creates an instance of Azure Service Bus Topic publisher
      /// </summary>
      /// <param name="connectionString">Service Bus connection string</param>
      /// <param name="topicName">Name of the Service Bus topic</param>
      public static IMessagePublisher AzureServiceBusTopicPublisher(this IMessagingFactory factory,
         string connectionString,
         string topicName)
      {
         return new AzureServiceBusTopicPublisher(connectionString, topicName);
      }

      /// <summary>
      /// Creates an instance by connection string and topic name
      /// </summary>
      /// <param name="connectionString">Full connection string to the Service Bus service, it looks like Endpoint=sb://myservice.servicebus.windows.net/;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=aLongKey</param>
      /// <param name="topicName">Name of the topic to subscribe to. If the topic does not exist it is created on the go.</param>
      /// <param name="subscriptionName">Name of the subscription inside the topic. It is created on the go when does not exist.</param>
      /// <param name="subscriptionSqlFilter">
      /// Optional. When specified creates the subscription with specific filter, otherwise subscribes
      /// to all messages withing the topic. Please see https://msdn.microsoft.com/library/azure/microsoft.servicebus.messaging.sqlfilter.sqlexpression.aspx for
      /// the filter syntax or refer to Service Fabric documentation on how to create SQL filters.
      /// </param>
      /// <param name="peekLock">Indicates the Service Bus mode (PeekLock or ReceiveAndDelete). PeekLock (true) is the most common scenario to use.</param>
      public static IMessageReceiver AzureServiceBusTopicReceiver(this IMessagingFactory factory,
         string connectionString, string topicName, string subscriptionName, string subscriptionSqlFilter, bool peekLock)
      {
         return new AzureServiceBusTopicReceiver(connectionString, topicName, subscriptionName, subscriptionSqlFilter, peekLock);
      }
#endif

   }
}

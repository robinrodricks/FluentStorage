using System;
using Storage.Net.Blob;
using Storage.Net.Messaging;
#if NETFULL
using Storage.Net.Microsoft.Azure.Messaging.ServiceBus;
#endif
using Storage.Net.Table;
using System.Net;
using Storage.Net.Microsoft.Azure.Messaging.EventHub;
using System.Collections.Generic;
using EHP = Storage.Net.Microsoft.Azure.Messaging.EventHub.AzureEventHubPublisher;
using EHR = Storage.Net.Microsoft.Azure.Messaging.EventHub.AzureEventHubReceiver;

namespace Storage.Net
{
   /// <summary>
   /// Factory class that implement factory methods for Microsoft Azure implememtations
   /// </summary>
   public static class Factory
   {

#if NETFULL
      /// <summary>
      /// Creates a new instance of Azure Service Bus Queue by connection string and queue name
      /// </summary>
      /// <param name="factory">Factory reference</param>
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
      /// <param name="factory">Factory reference</param>
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
      /// <param name="factory">Factory reference</param>
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
      /// <param name="factory">Factory reference</param>
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

      /// <summary>
      /// Creates Azure Event Hub publisher by namespace connection string and hub path
      /// </summary>
      /// <param name="factory">Factory reference</param>
      /// <param name="connectionString">Connection string</param>
      /// <param name="hubPath">Hub path (name)</param>
      /// <returns>Message publisher</returns>
      public static IMessagePublisher AzureEventHubPublisher(this IMessagingFactory factory, string connectionString, string hubPath)
      {
         return EHP.Create(connectionString, hubPath);
      }

      /// <summary>
      /// Create Azure Event Hub publisher by full connection string
      /// </summary>
      /// <param name="factory">Factory reference</param>
      /// <param name="fullConnectionString">Connection string</param>
      public static IMessagePublisher AzureEventHubPublisher(this IMessagingFactory factory, string fullConnectionString)
      {
         return new EHP(fullConnectionString);
      }

      /// <summary>
      /// The most detailed method with full fragmentation
      /// </summary>
      /// <param name="factory">Factory reference</param>
      /// <param name="endpointAddress">Endpoint address</param>
      /// <param name="entityPath">Entity path</param>
      /// <param name="sharedAccessKeyName">Shared access key name</param>
      /// <param name="sharedAccessKey">Shared access key value</param>
      /// <returns></returns>
      public static IMessagePublisher AzureEventHubPublisher(this IMessagingFactory factory, Uri endpointAddress, string entityPath, string sharedAccessKeyName, string sharedAccessKey)
      {
         return EHP.Create(endpointAddress, entityPath, sharedAccessKeyName, sharedAccessKey);
      }

      /// <summary>
      /// Creates Azure Event Hub receiver
      /// </summary>
      /// <param name="factory">Factory reference</param>
      /// <param name="connectionString"></param>
      /// <param name="hubPath"></param>
      /// <param name="partitionIds"></param>
      /// <param name="consumerGroupName"></param>
      /// <param name="stateStorage"></param>
      /// <returns></returns>
      public static IMessageReceiver AzureEventHubReceiver(this IMessagingFactory factory,
         string connectionString, string hubPath,
         IEnumerable<string> partitionIds = null,
         string consumerGroupName = null,
         IBlobStorage stateStorage = null
         )
      {
         return new AzureEventHubReceiver(connectionString, hubPath, partitionIds, consumerGroupName, stateStorage);
      }
   }
}

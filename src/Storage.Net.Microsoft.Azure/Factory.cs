using System;
using Storage.Net.Blob;
using Storage.Net.Messaging;
#if NETFULL
using Storage.Net.Microsoft.Azure.Messaging.ServiceBus;
#endif
using Storage.Net.Table;
using System.Net;
using System.Collections.Generic;

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
   }
}

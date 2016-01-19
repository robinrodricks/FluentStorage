using System;
using Storage.Net.Messaging;
using Microsoft.ServiceBus;
using Microsoft.ServiceBus.Messaging;
using System.Collections.Generic;
using System.Linq;

namespace Storage.Net.Azure.Messaging.ServiceBus
{
   /// <summary>
   /// Represents queues as Azure Service Bus Topics. Note that you must have at least one subscription
   /// for messages not to be lost. Subscriptions represent <see cref="AzureServiceBusTopicReceiver"/>
   /// in this library
   /// </summary>
   public class AzureServiceBusTopicPublisher : IMessagePublisher
   {
      private NamespaceManager _nsMgr;
      readonly private string _connectionString;
      readonly private string _topicName;
      private TopicClient _client;

      /// <summary>
      /// Constructor
      /// </summary>
      /// <param name="connectionString">Service Bus connection string</param>
      /// <param name="topicName">Name of the Service Bus topic</param>
      public AzureServiceBusTopicPublisher(string connectionString, string topicName)
      {
         _connectionString = connectionString;
         _nsMgr = NamespaceManager.CreateFromConnectionString(connectionString);
         _topicName = topicName;

         TopicHelper.PrepareTopic(_nsMgr, topicName);
         _client = TopicClient.CreateFromConnectionString(_connectionString, _topicName);
      }

      /// <summary>
      /// Sends a <see cref="BrokeredMessage"/> with passed content
      /// </summary>
      public void PutMessage(QueueMessage message)
      {
         _client.Send(Converter.ToBrokeredMessage(message));
      }

      /// <summary>
      /// Sends a <see cref="BrokeredMessage"/> with passed content
      /// </summary>
      public void PutMessages(IEnumerable<QueueMessage> messages)
      {
         if(messages == null) return;
         IEnumerable<BrokeredMessage> bms = messages.Select(Converter.ToBrokeredMessage);
         _client.SendBatch(bms);
      }

      /// <summary>
      /// Doesn't do anything
      /// </summary>
      public void Dispose()
      {
      }
   }
}

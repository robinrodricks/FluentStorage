using System;
using Storage.Net.Messaging;
using Microsoft.ServiceBus;
using Microsoft.ServiceBus.Messaging;

namespace Storage.Net.Azure.Queue.ServiceBus
{
   /// <summary>
   /// Represents queues as Azure Service Bus Topics
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

         PrepareTopic();
      }

      private void PrepareTopic()
      {
         if(!_nsMgr.TopicExists(_topicName))
         {
            var td = new TopicDescription(_topicName);
            //todo: more options on TD

            _nsMgr.CreateTopic(td);
         }

         _client = TopicClient.CreateFromConnectionString(_connectionString, _topicName);
      }

      private void PrepareSubscription()
      {
         //
      }

      /// <summary>
      /// Sends a <see cref="BrokeredMessage"/> with passed content
      /// </summary>
      /// <param name="message"></param>
      public void PutMessage(QueueMessage message)
      {
         _client.Send(ServiceBusConverter.ToBrokeredMessage(message));
      }
   }
}

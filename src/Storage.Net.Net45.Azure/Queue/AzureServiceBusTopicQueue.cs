using System;
using Storage.Net.Messaging;
using Microsoft.ServiceBus;
using Microsoft.ServiceBus.Messaging;

namespace Storage.Net.Azure.Queue
{
   /// <summary>
   /// Represents queues as Azure Service Bus Topics
   /// </summary>
   public class AzureServiceBusTopicQueue : IMessageQueue
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
      public AzureServiceBusTopicQueue(string connectionString, string topicName)
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

      public void Clear()
      {
         throw new NotImplementedException();
      }

      public void DeleteMessage(string id)
      {
         throw new NotImplementedException();
      }

      public QueueMessage GetMessage(TimeSpan? visibilityTimeout = default(TimeSpan?))
      {
         throw new NotSupportedException();
      }

      public QueueMessage PeekMesssage()
      {
         throw new NotSupportedException();
      }

      /// <summary>
      /// Sends a <see cref="BrokeredMessage"/> with passed content
      /// </summary>
      /// <param name="message"></param>
      public void PutMessage(QueueMessage message)
      {
         _client.Send(ToBrokeredMessage(message));
      }

      private static BrokeredMessage ToBrokeredMessage(QueueMessage message)
      {
         var result = new BrokeredMessage(message.Content.ToMemoryStream());
         if(message.Properties != null && message.Properties.Count > 0)
         {
            foreach(var prop in message.Properties)
            {
               result.Properties.Add(prop.Key, prop.Value);
            }
         }
         return result;
      }

      private QueueMessage ToQueueMessage(BrokeredMessage message)
      {
         return null;
      }
   }
}

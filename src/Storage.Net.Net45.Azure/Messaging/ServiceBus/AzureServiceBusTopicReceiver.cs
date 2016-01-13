using Microsoft.ServiceBus;
using Microsoft.ServiceBus.Messaging;
using Storage.Net.Messaging;
using System;
using System.Collections.Concurrent;

namespace Storage.Net.Azure.Messaging.ServiceBus
{
   /// <summary>
   /// Subscribes to messages in a Service Bus Topic. This version subscribes to ALL messages.
   /// </summary>
   public class AzureServiceBusTopicReceiver : IMessageReceiver
   {
      private readonly NamespaceManager _nsMgr;
      private readonly SubscriptionClient _client;
      private readonly bool _peekLock;
      private readonly ConcurrentDictionary<string, BrokeredMessage> _messageIdToBrokeredMessage = new ConcurrentDictionary<string, BrokeredMessage>();

      /// <summary>
      /// Creates an instance by connection string and topic name
      /// </summary>
      public AzureServiceBusTopicReceiver(string connectionString, string topicName, string subscriptionName, bool peekLock = true)
      {
         _nsMgr = NamespaceManager.CreateFromConnectionString(connectionString);

         TopicHelper.PrepareTopic(_nsMgr, topicName);

         if(!_nsMgr.SubscriptionExists(topicName, subscriptionName))
         {
            _nsMgr.CreateSubscription(topicName, subscriptionName);
         }

         _peekLock = peekLock;
         _client = SubscriptionClient.CreateFromConnectionString(connectionString,
            topicName,
            subscriptionName,
            peekLock ? ReceiveMode.PeekLock : ReceiveMode.ReceiveAndDelete);
      }

      /// <summary>
      /// Calls .Complete on the message if this subscription is in PeekLock mode, otherwise the call is ignored
      /// </summary>
      /// <param name="message"></param>
      public void ConfirmMessage(QueueMessage message)
      {
         if(!_peekLock) return;

         BrokeredMessage bm;
         //delete the message and get the deleted element, very nice method!
         if(!_messageIdToBrokeredMessage.TryRemove(message.Id, out bm)) return;

         bm.Complete();
      }

      /// <summary>
      /// Nothing to dispose
      /// </summary>
      public void Dispose()
      {
      }

      /// <summary>
      /// As per interface
      /// </summary>
      /// <returns></returns>
      public QueueMessage ReceiveMessage()
      {
         BrokeredMessage bm = _client.Receive();
         if(bm == null) return null;

         QueueMessage qm = Converter.ToQueueMessage(bm);
         if(_peekLock) _messageIdToBrokeredMessage[qm.Id] = bm;
         return qm;
      }
   }
}

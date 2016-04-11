using Microsoft.ServiceBus;
using Microsoft.ServiceBus.Messaging;
using Storage.Net.Messaging;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

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
      /// <param name="connectionString">Full connection string to the Service Bus service, it looks like Endpoint=sb://myservice.servicebus.windows.net/;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=aLongKey</param>
      /// <param name="topicName">Name of the topic to subscribe to. If the topic does not exist it is created on the go.</param>
      /// <param name="subscriptionName">Name of the subscription inside the topic. It is created on the go when does not exist.</param>
      /// <param name="subscriptionSqlFilter">
      /// Optional. When specified creates the subscription with specific filter, otherwise subscribes
      /// to all messages withing the topic. Please see https://msdn.microsoft.com/library/azure/microsoft.servicebus.messaging.sqlfilter.sqlexpression.aspx for
      /// the filter syntax or refer to Service Fabric documentation on how to create SQL filters.
      /// </param>
      /// <param name="peekLock">Indicates the Service Bus mode (PeekLock or ReceiveAndDelete). PeekLock (true) is the most common scenario to use.</param>
      public AzureServiceBusTopicReceiver(string connectionString, string topicName, string subscriptionName, string subscriptionSqlFilter, bool peekLock)
      {
         _nsMgr = NamespaceManager.CreateFromConnectionString(connectionString);

         TopicHelper.PrepareTopic(_nsMgr, topicName);

         if(!_nsMgr.SubscriptionExists(topicName, subscriptionName))
         {
            if (string.IsNullOrEmpty(subscriptionSqlFilter))
            {
               _nsMgr.CreateSubscription(topicName, subscriptionName);
            }
            else
            {
               _nsMgr.CreateSubscription(topicName, subscriptionName, new SqlFilter(subscriptionSqlFilter));
            }
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

      public void StartMessagePump(Action<QueueMessage> onMessage)
      {
         throw new NotImplementedException();
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
      public QueueMessage ReceiveMessage()
      {
         BrokeredMessage bm = _client.Receive();
         if(bm == null) return null;
         return ProcessAndConvert(bm);
      }

      /// <summary>
      /// As per interface
      /// </summary>
      public IEnumerable<QueueMessage> ReceiveMessages(int count)
      {
         IEnumerable<BrokeredMessage> batch = _client.ReceiveBatch(count);
         if(batch == null) return null;
         return batch.Select(ProcessAndConvert).ToList();
      }

      private QueueMessage ProcessAndConvert(BrokeredMessage bm)
      {
         QueueMessage qm = Converter.ToQueueMessage(bm);
         if(_peekLock) _messageIdToBrokeredMessage[qm.Id] = bm;
         return qm;
      }
   }
}

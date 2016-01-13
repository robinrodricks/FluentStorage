using Microsoft.ServiceBus.Messaging;
using Storage.Net.Messaging;
using System;
using System.Collections.Concurrent;

namespace Storage.Net.Azure.Messaging.ServiceBus
{
   /// <summary>
   /// Implements message receiver on Azure Service Bus Queues
   /// </summary>
   public class AzureServiceBusQueueReceiver : IMessageReceiver
   {
      private static readonly TimeSpan AutoRenewTimeout = TimeSpan.FromMinutes(1);

      private readonly QueueClient _client;
      private readonly bool _peekLock;
      private readonly ConcurrentDictionary<string, BrokeredMessage> _messageIdToBrokeredMessage = new ConcurrentDictionary<string, BrokeredMessage>();

      /// <summary>
      /// Creates 
      /// </summary>
      /// <param name="connectionString"></param>
      /// <param name="queueName"></param>
      /// <param name="peekLock"></param>
      public AzureServiceBusQueueReceiver(string connectionString, string queueName, bool peekLock = true)
      {
         _client = QueueClient.CreateFromConnectionString(connectionString, queueName,
            peekLock ? ReceiveMode.PeekLock : ReceiveMode.ReceiveAndDelete);

         _peekLock = peekLock;
      }

      /// <summary>
      /// Tries to receive the message from queue client by calling .Receive explicitly.
      /// </summary>
      /// <returns></returns>
      public QueueMessage ReceiveMessage()
      {
         BrokeredMessage bm =_client.Receive();
         if(bm == null) return null;

         QueueMessage qm = Converter.ToQueueMessage(bm);
         if(_peekLock) _messageIdToBrokeredMessage[qm.Id] = bm;
         return qm;
      }

      /// <summary>
      /// Call at the end when done with the message.
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
      /// Doesn't do anything
      /// </summary>
      public void Dispose()
      {
      }
   }
}

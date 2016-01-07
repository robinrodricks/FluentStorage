using Microsoft.ServiceBus.Messaging;
using Storage.Net.Messaging;
using System;
using System.Collections.Concurrent;

namespace Storage.Net.Azure.Queue.ServiceBus
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

         SubscribeQueue();
      }

      private void SubscribeQueue()
      {
         var options = new OnMessageOptions();
         options.AutoComplete = false;
         options.AutoRenewTimeout = AutoRenewTimeout;

         _client.OnMessage(OnBrokeredMessage, options);
      }

      private void OnBrokeredMessage(BrokeredMessage message)
      {
         QueueMessage result = Converter.ToQueueMessage(message);

         if(_peekLock)
         {
            //only cache messages in PeekLock mode
            _messageIdToBrokeredMessage[result.Id] = message;
         }

         if(OnNewMessage != null) OnNewMessage(this, result);
      }

      /// <summary>
      /// Fired when a new message appears in the queue
      /// </summary>
      public event EventHandler<QueueMessage> OnNewMessage;

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

      public void Dispose()
      {
         //_client.OnMessage -= OnBrokeredMessage;
      }
   }
}

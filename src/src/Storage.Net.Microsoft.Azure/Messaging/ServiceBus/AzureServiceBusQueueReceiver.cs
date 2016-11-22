using Microsoft.ServiceBus.Messaging;
using Storage.Net.Messaging;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace Storage.Net.Microsoft.Azure.Messaging.ServiceBus
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
      private Action<QueueMessage> _onMessageAction;

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
      public QueueMessage ReceiveMessage()
      {
         BrokeredMessage bm = _client.Receive(TimeSpan.FromMilliseconds(1));
         if(bm == null) return null;
         return ProcessAndConvert(bm);
      }

      /// <summary>
      /// Tries to receive the message from queue client by calling .Receive explicitly.
      /// </summary>
      public IEnumerable<QueueMessage> ReceiveMessages(int count)
      {
         IEnumerable<BrokeredMessage> batch = _client.ReceiveBatch(count, TimeSpan.FromMilliseconds(1));
         if(batch == null) return null;

         return batch.Select(ProcessAndConvert).ToList();
      }

      /// <summary>
      /// Calls .DeadLetter explicitly
      /// </summary>
      public void DeadLetter(QueueMessage message, string reason, string errorDescription)
      {
         if (!_peekLock) return;

         BrokeredMessage bm;
         if (!_messageIdToBrokeredMessage.TryRemove(message.Id, out bm)) return;

         _client.DeadLetter(bm.LockToken, reason, errorDescription);
      }

      private QueueMessage ProcessAndConvert(BrokeredMessage bm)
      {
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
      /// Starts message pump with AutoComplete = false, 1 minute session renewal and 1 concurrent call.
      /// </summary>
      /// <param name="onMessage"></param>
      public void StartMessagePump(Action<QueueMessage> onMessage)
      {
         if (onMessage == null) throw new ArgumentNullException(nameof(onMessage));
         if (_onMessageAction != null) throw new ArgumentException("message pump already started", nameof(onMessage));

         _onMessageAction = onMessage;

         var options = new OnMessageOptions
         {
            AutoComplete = false,
            AutoRenewTimeout = TimeSpan.FromMinutes(1),
            MaxConcurrentCalls = 1
         };

         _client.OnMessage(OnMessage, options);
      }

      private void OnMessage(BrokeredMessage bm)
      {
         QueueMessage qm = ProcessAndConvert(bm);

         _onMessageAction?.Invoke(qm);
      } 

      /// <summary>
      /// Stops message pump if started
      /// </summary>
      public void Dispose()
      {
         _client.Close();  //this also stops the message pump
      }
   }
}

using Microsoft.Azure.ServiceBus;
using Storage.Net.Messaging;
using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace Storage.Net.Microsoft.Azure.ServiceBus
{
   /// <summary>
   /// Implements message receiver on Azure Service Bus Queues
   /// </summary>
   class AzureServiceBusTopicReceiver : IMessageReceiver
   {
      //https://github.com/Azure/azure-service-bus/blob/master/samples/DotNet/Microsoft.Azure.ServiceBus/ReceiveSample/readme.md

      private static readonly TimeSpan AutoRenewTimeout = TimeSpan.FromMinutes(1);

      private readonly SubscriptionClient _client;
      private readonly bool _peekLock;
      private readonly ConcurrentDictionary<string, Message> _messageIdToBrokeredMessage = new ConcurrentDictionary<string, Message>();

      /// <summary>
      /// Creates an instance of Azure Service Bus receiver with connection
      /// </summary>
      /// <param name="connectionString">Service Bus connection string</param>
      /// <param name="topicName">Queue name in Service Bus</param>
      public AzureServiceBusTopicReceiver(string connectionString, string topicName, string subscriptionName, bool peekLock = true)
      {
         _client = new SubscriptionClient(connectionString, topicName, subscriptionName, peekLock ? ReceiveMode.PeekLock : ReceiveMode.ReceiveAndDelete);
         _peekLock = peekLock;
      }

      /// <summary>
      /// Calls .DeadLetter explicitly
      /// </summary>
      public async Task DeadLetterAsync(QueueMessage message, string reason, string errorDescription, CancellationToken cancellationToken)
      {
         if (!_peekLock) return;

         if (!_messageIdToBrokeredMessage.TryRemove(message.Id, out Message bm)) return;

         await _client.DeadLetterAsync(bm.MessageId);
      }

      private QueueMessage ProcessAndConvert(Message bm)
      {
         QueueMessage qm = Converter.ToQueueMessage(bm);
         if(_peekLock) _messageIdToBrokeredMessage[qm.Id] = bm;
         return qm;
      }

      /// <summary>
      /// Call at the end when done with the message.
      /// </summary>
      /// <param name="message"></param>
      public async Task ConfirmMessageAsync(QueueMessage message, CancellationToken cancellationToken)
      {
         if(!_peekLock) return;

         //delete the message and get the deleted element, very nice method!
         if (!_messageIdToBrokeredMessage.TryRemove(message.Id, out Message bm)) return;

         await _client.CompleteAsync(bm.SystemProperties.LockToken);
      }

      /// <summary>
      /// Starts message pump with AutoComplete = false, 1 minute session renewal and 1 concurrent call.
      /// </summary>
      /// <param name="onMessage"></param>
      public Task StartMessagePumpAsync(Func<IEnumerable<QueueMessage>, Task> onMessage, int maxBatchSize, CancellationToken cancellationToken)
      {
         if (onMessage == null) throw new ArgumentNullException(nameof(onMessage));

         var options = new MessageHandlerOptions(ExceptionReceiverHandler)
         {
            AutoComplete = false,
            MaxAutoRenewDuration = TimeSpan.FromMinutes(1),
            MaxConcurrentCalls = 1
         };

         _client.PrefetchCount = maxBatchSize;

         _client.RegisterMessageHandler(
            async (message, token) =>
            {
               QueueMessage qm = Converter.ToQueueMessage(message);
               _messageIdToBrokeredMessage[qm.Id] = message;
               await onMessage(Enumerable.Repeat(qm, 1));
            },
            options);

         return Task.FromResult(true);
      }

      private Task ExceptionReceiverHandler(ExceptionReceivedEventArgs args)
      {
         return Task.FromResult(true);
      }

      /// <summary>
      /// Stops message pump if started
      /// </summary>
      public void Dispose()
      {
         _client.CloseAsync().Wait();  //this also stops the message pump
      }

      public async Task<ITransaction> OpenTransactionAsync()
      {
         return EmptyTransaction.Instance;
      }
   }
}

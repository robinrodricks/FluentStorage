using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Storage.Net.Messaging
{
   class InMemoryMessagePublisherReceiver : PollingMessageReceiver, IMessagePublisher
   {
      private readonly ConcurrentQueue<QueueMessage> _queue = new ConcurrentQueue<QueueMessage>();

      public Task PutMessagesAsync(IEnumerable<QueueMessage> messages, CancellationToken cancellationToken = default)
      {
         if (messages == null) return Task.FromResult(true);

         foreach(QueueMessage qm in messages)
         {
            _queue.Enqueue(qm);
         }

         return Task.FromResult(true);
      }

      protected override Task<IReadOnlyCollection<QueueMessage>> ReceiveMessagesAsync(int maxBatchSize)
      {
         var result = new List<QueueMessage>();

         while(_queue.TryDequeue(out QueueMessage message) && result.Count < maxBatchSize)
         {
            result.Add(message);
         }

         return Task.FromResult((IReadOnlyCollection<QueueMessage>)result);
      }

      public override Task ConfirmMessageAsync(QueueMessage message, CancellationToken cancellationToken = default)
      {
         return Task.FromResult(true);
      }
   }
}
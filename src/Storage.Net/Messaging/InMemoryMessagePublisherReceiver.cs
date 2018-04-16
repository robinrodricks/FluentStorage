using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Storage.Net.Messaging
{
   class InMemoryMessagePublisherReceiver : IMessagePublisher, IMessageReceiver
   {
      private readonly ConcurrentQueue<QueueMessage> _queue = new ConcurrentQueue<QueueMessage>();
      private readonly CancellationTokenSource _cts = new CancellationTokenSource();
      private Task _pollingTask;
      private bool _disposed;

      public Task ConfirmMessageAsync(QueueMessage message, CancellationToken cancellationToken = default)
      {
         return Task.FromResult(true);
      }

      public Task DeadLetterAsync(QueueMessage message, string reason, string errorDescription, CancellationToken cancellationToken = default)
      {
         throw new NotSupportedException();
      }

      public Task<ITransaction> OpenTransactionAsync()
      {
         return Task.FromResult(EmptyTransaction.Instance);
      }

      public Task PutMessagesAsync(IEnumerable<QueueMessage> messages, CancellationToken cancellationToken = default)
      {
         if (messages == null) return Task.FromResult(true);

         foreach(QueueMessage qm in messages)
         {
            _queue.Enqueue(qm);
         }

         return Task.FromResult(true);
      }

      public async Task StartMessagePumpAsync(Func<IEnumerable<QueueMessage>, Task> onMessageAsync, int maxBatchSize = 1, CancellationToken cancellationToken = default)
      {
         if (onMessageAsync == null) throw new ArgumentNullException(nameof(onMessageAsync));
         if (_pollingTask != null) throw new ArgumentException("polling already started", nameof(onMessageAsync));

         _pollingTask = PollTasks(onMessageAsync, maxBatchSize, cancellationToken);
      }

      private async Task PollTasks(Func<IEnumerable<QueueMessage>, Task> callback, int maxBatchSize, CancellationToken cancellationToken)
      {
         if (cancellationToken.IsCancellationRequested || _cts.IsCancellationRequested) return;

         IEnumerable<QueueMessage> messages = await ReceiveMessagesAsync(maxBatchSize);
         while (messages != null)
         {
            await callback(messages);
            messages = await ReceiveMessagesAsync(maxBatchSize);
         }

         await Task.Delay(TimeSpan.FromSeconds(1), cancellationToken).ContinueWith(async (t) =>
         {
            await PollTasks(callback, maxBatchSize, cancellationToken);
         });
      }

      private Task<IEnumerable<QueueMessage>> ReceiveMessagesAsync(int maxBatchSize)
      {
         var result = new List<QueueMessage>();

         while(_queue.TryDequeue(out QueueMessage message) && result.Count < maxBatchSize)
         {
            result.Add(message);
         }

         return result.Count == 0
            ? Task.FromResult((IEnumerable<QueueMessage>)null)
            : Task.FromResult((IEnumerable<QueueMessage>)result);
      }

      public void Dispose()
      {
         if (!_disposed)
         {
            _disposed = true;
            _cts.Cancel();
         }
      }
   }
}
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using NetBox.Extensions;

namespace Storage.Net.Messaging
{
   public abstract class PollingMessageReceiver : IMessageReceiver
   {
      public virtual Task ConfirmMessageAsync(QueueMessage message, CancellationToken cancellationToken = default)
      {
         throw new NotSupportedException();
      }

      public virtual Task DeadLetterAsync(QueueMessage message, string reason, string errorDescription, CancellationToken cancellationToken = default)
      {
         throw new NotSupportedException();
      }

      public virtual void Dispose()
      {
      }

      public virtual Task<ITransaction> OpenTransactionAsync()
      {
         return Task.FromResult(EmptyTransaction.Instance);
      }

      public async Task StartMessagePumpAsync(Func<IEnumerable<QueueMessage>, Task> onMessageAsync, int maxBatchSize = 1, CancellationToken cancellationToken = default)
      {
         if (onMessageAsync == null) throw new ArgumentNullException(nameof(onMessageAsync));

         PollTasks(onMessageAsync, maxBatchSize, cancellationToken).Forget();
      }

      private async Task PollTasks(Func<IEnumerable<QueueMessage>, Task> callback, int maxBatchSize, CancellationToken cancellationToken)
      {
         IReadOnlyCollection<QueueMessage> messages = await ReceiveMessagesAsync(maxBatchSize);
         while (messages != null && messages.Count > 0)
         {
            await callback(messages);

            messages = await ReceiveMessagesAsync(maxBatchSize);
         }

         await Task.Delay(TimeSpan.FromSeconds(1), cancellationToken).ContinueWith(async (t) =>
         {
            await PollTasks(callback, maxBatchSize, cancellationToken);
         });
      }

      protected abstract Task<IReadOnlyCollection<QueueMessage>> ReceiveMessagesAsync(int maxBatchSize);
   }
}

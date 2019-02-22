using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Storage.Net.Messaging.Large
{
   class LargeMessageContentMessageReceiver : IMessageReceiver
   {
      private readonly IMessageReceiver _parentReceiver;

      public LargeMessageContentMessageReceiver(IMessageReceiver parentReceiver, bool storageOpen)
      {
         _parentReceiver = parentReceiver;
      }

      public async Task ConfirmMessagesAsync(IReadOnlyCollection<QueueMessage> messages, CancellationToken cancellationToken = default)
      {
         await _parentReceiver.ConfirmMessagesAsync(messages, cancellationToken);

         //todo: remove blobs
      }

      public async Task DeadLetterAsync(QueueMessage message, string reason, string errorDescription, CancellationToken cancellationToken = default)
      {
         await _parentReceiver.DeadLetterAsync(message, reason, errorDescription, cancellationToken);

         //todo: remove blobs
      }

      public void Dispose()
      {
         _parentReceiver.Dispose();
      }

      public Task<int> GetMessageCountAsync() => _parentReceiver.GetMessageCountAsync();

      public Task<ITransaction> OpenTransactionAsync() => _parentReceiver.OpenTransactionAsync();

      public Task StartMessagePumpAsync(Func<IReadOnlyCollection<QueueMessage>, Task> onMessageAsync, int maxBatchSize = 1, CancellationToken cancellationToken = default)
      {
         //todo: redirect pumping

         return _parentReceiver.StartMessagePumpAsync(
            onMessageAsync,
            maxBatchSize, cancellationToken);
      }

      private async Task DownloadingMessagePumpAsync(IReadOnlyCollection<QueueMessage> messages,
         Func<IReadOnlyCollection<QueueMessage>, Task> onMessageAsync)
      {
         //todo: download and forward
      }
   }
}

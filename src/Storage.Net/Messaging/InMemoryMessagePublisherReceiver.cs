using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Storage.Net.Messaging
{
   class InMemoryMessagePublisherReceiver : IMessagePublisher, IMessageReceiver
   {
      public Task ConfirmMessageAsync(QueueMessage message, CancellationToken cancellationToken = default)
      {
         throw new NotImplementedException();
      }

      public Task DeadLetterAsync(QueueMessage message, string reason, string errorDescription, CancellationToken cancellationToken = default)
      {
         throw new NotImplementedException();
      }

      public Task<ITransaction> OpenTransactionAsync()
      {
         throw new NotImplementedException();
      }

      public Task PutMessagesAsync(IEnumerable<QueueMessage> messages, CancellationToken cancellationToken = default)
      {
         throw new NotImplementedException();
      }

      public Task StartMessagePumpAsync(Func<IEnumerable<QueueMessage>, Task> onMessageAsync, int maxBatchSize = 1, CancellationToken cancellationToken = default)
      {
         throw new NotImplementedException();
      }

      public void Dispose()
      {
         throw new NotImplementedException();
      }
   }
}
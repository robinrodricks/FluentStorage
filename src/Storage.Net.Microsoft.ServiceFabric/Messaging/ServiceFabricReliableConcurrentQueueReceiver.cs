using Microsoft.ServiceFabric.Data;
using Microsoft.ServiceFabric.Data.Collections;
using Storage.Net.Messaging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace Storage.Net.Microsoft.ServiceFabric.Messaging
{
   class ServiceFabricReliableConcurrentQueueReceiver : IMessageReceiver
   {
      private IReliableStateManager _stateManager;
      private readonly string _queueName;
      private readonly TimeSpan _scanInterval;
      private bool _disposed;

      public ServiceFabricReliableConcurrentQueueReceiver(IReliableStateManager stateManager, string queueName, TimeSpan scanInterval)
      {
         _stateManager = stateManager ?? throw new ArgumentNullException(nameof(stateManager));
         _queueName = queueName ?? throw new ArgumentNullException(nameof(queueName));

         if (scanInterval < TimeSpan.FromSeconds(1)) throw new ArgumentException("scan interval must be at least 1 second", nameof(scanInterval));

         _scanInterval = scanInterval;
      }

      public async Task StartMessagePumpAsync(Func<IEnumerable<QueueMessage>, Task> onMessage, int maxBatchSize, CancellationToken cancellationToken)
      {
         Task.Run(() => ReceiveMessagesAsync(onMessage, maxBatchSize, cancellationToken));
      }

      public Task ConfirmMessageAsync(QueueMessage message, CancellationToken cancellationToken)
      {
         return Task.FromResult(true);
      }

      public Task DeadLetterAsync(QueueMessage message, string reason, string errorDescription, CancellationToken cancellationToken)
      {
         return Task.FromResult(true);
      }

      private async Task ReceiveMessagesAsync(Func<IEnumerable<QueueMessage>, Task> onMessage, int maxBatchSize, CancellationToken cancellationToken)
      {
         var messages = new List<QueueMessage>();

         while (!cancellationToken.IsCancellationRequested && !_disposed)
         {
            try
            {
               using (var tx = new ServiceFabricTransaction(_stateManager, null))
               {
                  IReliableConcurrentQueue<byte[]> collection = await GetCollectionAsync();

                  while (messages.Count < maxBatchSize)
                  {
                     ConditionalValue<byte[]> message = await collection.TryDequeueAsync(tx.Tx, cancellationToken);
                     if (message.HasValue)
                     {
                        QueueMessage qm = QueueMessage.FromByteArray(message.Value);

                        messages.Add(qm);
                     }
                     else
                     {
                        break;
                     }
                  }

                  //make the call before committing the transaction
                  if (messages.Count > 0)
                  {
                     await onMessage(messages);
                     messages.Clear();
                  }

                  await tx.CommitAsync();
               }
            }
            catch(Exception ex)
            {
               Trace.Fail($"failed to listen to messages on queue '{_queueName}'", ex.ToString());
            }

            await Task.Delay(_scanInterval);
         }
      }

      public void Dispose()
      {
         _disposed = true;
      }

      public async Task<ITransaction> OpenTransactionAsync()
      {
         return EmptyTransaction.Instance;
      }

      private Task<IReliableConcurrentQueue<byte[]>> GetCollectionAsync()
      {
         return _stateManager.GetOrAddAsync<IReliableConcurrentQueue<byte[]>>(_queueName);
      }
   }
}

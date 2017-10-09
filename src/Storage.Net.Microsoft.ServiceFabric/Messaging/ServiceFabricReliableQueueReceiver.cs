using Microsoft.ServiceFabric.Data;
using Microsoft.ServiceFabric.Data.Collections;
using Storage.Net.Messaging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Storage.Net.Microsoft.ServiceFabric.Messaging
{
   class ServiceFabricReliableQueueReceiver : IMessageReceiver
   {
      private IReliableStateManager _stateManager;
      private readonly string _queueName;
      private IReliableQueue<byte[]> _collection;

      public ServiceFabricReliableQueueReceiver(IReliableStateManager stateManager, string queueName)
      {
         _stateManager = stateManager ?? throw new ArgumentNullException(nameof(stateManager));
         _queueName = queueName ?? throw new ArgumentNullException(nameof(queueName));
      }

      public async Task StartMessagePumpAsync(Func<IEnumerable<QueueMessage>, Task> onMessage, int maxBatchSize)
      {
         IReliableQueue<byte[]> collection = await GetCollectionAsync();

         throw new NotImplementedException();
      }

      public Task ConfirmMessageAsync(QueueMessage message)
      {
         throw new NotSupportedException();
      }

      public Task DeadLetterAsync(QueueMessage message, string reason, string errorDescription)
      {
         throw new NotSupportedException();
      }

      private async Task<IEnumerable<QueueMessage>> ReceiveMessagesAsync(int count)
      {
         IReliableQueue<byte[]> collection = await _stateManager.GetOrAddAsync<IReliableQueue<byte[]>>(_queueName);
         var result = new List<QueueMessage>();

         using (var tx = new ServiceFabricTransaction(_stateManager, null))
         {
            while (result.Count < count)
            {
               ConditionalValue<byte[]> message = await collection.TryDequeueAsync(tx.Tx);
               if (message.HasValue)
               {
                  QueueMessage qm = QueueMessage.FromByteArray(message.Value);

                  result.Add(qm);
               }
               else
               {
                  break;
               }
            }

            await tx.CommitAsync();
         }

         return result.Count == 0 ? null : result;
      }

      public void Dispose()
      {
      }

      public async Task<ITransaction> OpenTransactionAsync()
      {
         return EmptyTransaction.Instance;
      }

      private async Task<IReliableQueue<byte[]>> GetCollectionAsync()
      {
         if(_collection == null)
         {
            _collection = await _stateManager.GetOrAddAsync<IReliableQueue<byte[]>>(_queueName);
         }

         return _collection;
      }
   }
}

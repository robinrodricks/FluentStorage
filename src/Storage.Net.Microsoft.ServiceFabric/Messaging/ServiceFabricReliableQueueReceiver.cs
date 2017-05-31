using Microsoft.ServiceFabric.Data;
using Microsoft.ServiceFabric.Data.Collections;
using Storage.Net.Messaging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Storage.Net.Microsoft.ServiceFabric.Messaging
{
   class ServiceFabricReliableQueueReceiver : AsyncMessageReceiver
   {
      private IReliableStateManager _stateManager;
      private readonly string _queueName;

      public ServiceFabricReliableQueueReceiver(IReliableStateManager stateManager, string queueName)
      {
         _stateManager = stateManager ?? throw new ArgumentNullException(nameof(stateManager));
         _queueName = queueName ?? throw new ArgumentNullException(nameof(queueName));
      }

      public override async Task<IEnumerable<QueueMessage>> ReceiveMessagesAsync(int count)
      {
         var collection = await _stateManager.GetOrAddAsync<IReliableQueue<byte[]>>(_queueName);
         var result = new List<QueueMessage>();

         using (var tx = new FabricTransactionManager<string>(_stateManager, null))
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
   }
}

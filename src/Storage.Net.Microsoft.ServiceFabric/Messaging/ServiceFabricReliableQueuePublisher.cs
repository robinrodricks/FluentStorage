using Microsoft.ServiceFabric.Data;
using Microsoft.ServiceFabric.Data.Collections;
using Storage.Net.Messaging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Storage.Net.Microsoft.ServiceFabric.Messaging
{
   class ServiceFabricReliableQueuePublisher : IMessagePublisher
   {
      private IReliableStateManager _stateManager;
      private readonly string _queueName;

      public ServiceFabricReliableQueuePublisher(IReliableStateManager stateManager, string queueName)
      {
         _stateManager = stateManager ?? throw new ArgumentNullException(nameof(stateManager));
         _queueName = queueName ?? throw new ArgumentNullException(nameof(queueName));
      }

      public async Task PutMessagesAsync(IEnumerable<QueueMessage> messages)
      {
         IReliableQueue<byte[]> collection = await _stateManager.GetOrAddAsync<IReliableQueue<byte[]>>(_queueName);

         using (var tx = new ServiceFabricTransaction(_stateManager, null))
         {
            foreach (QueueMessage message in messages)
            {
               byte[] data = message.ToByteArray();
               await collection.EnqueueAsync(tx.Tx, data);
            }

            await tx.CommitAsync();
         }
      }

      public void Dispose()
      {

      }
   }
}

using System;
using System.Collections.Generic;
using System.Fabric;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.ServiceFabric.Data.Collections;
using Microsoft.ServiceFabric.Services.Communication.Runtime;
using Microsoft.ServiceFabric.Services.Runtime;
using Storage.Net;
using Storage.Net.Blob;
using Storage.Net.Messaging;
using SFT = Microsoft.ServiceFabric.Data.ITransaction;

namespace Stateful1
{
   /// <summary>
   /// An instance of this class is created for each service replica by the Service Fabric runtime.
   /// </summary>
   internal sealed class Stateful1 : StatefulService
   {
      private readonly BlobStorage _blobs;
      private readonly IMessagePublisher _publisher;
      private readonly IMessageReceiver _receiver;

      public Stateful1(StatefulServiceContext context)
          : base(context)
      {
         _blobs = new BlobStorage(StorageFactory.Blobs.AzureServiceFabricReliableStorage(this.StateManager, "c1"));
         _publisher = StorageFactory.Messages.AzureServiceFabricReliableQueuePublisher(this.StateManager);
         _receiver = StorageFactory.Messages.AzureServiceFabricReliableQueueReceiver(this.StateManager, TimeSpan.FromSeconds(1));
      }

      /// <summary>
      /// Optional override to create listeners (e.g., HTTP, Service Remoting, WCF, etc.) for this service replica to handle client or user requests.
      /// </summary>
      /// <remarks>
      /// For more information on service communication, see https://aka.ms/servicefabricservicecommunication
      /// </remarks>
      /// <returns>A collection of listeners.</returns>
      protected override IEnumerable<ServiceReplicaListener> CreateServiceReplicaListeners()
      {
         return new ServiceReplicaListener[0];
      }

      /// <summary>
      /// This is the main entry point for your service replica.
      /// This method executes when this replica of your service becomes primary and has write status.
      /// </summary>
      /// <param name="cancellationToken">Canceled when Service Fabric needs to shut down this service replica.</param>
      protected override async Task RunAsync(CancellationToken cancellationToken)
      {
         try
         {
            await _receiver.StartMessagePumpAsync(OnNewMessages, 1, cancellationToken);

            //IEnumerable<QueueMessage> qm = await _receiver.ReceiveMessagesAsync(100);

            await _publisher.PutMessagesAsync(new[] { QueueMessage.FromText("content at " + DateTime.UtcNow) });

            //qm = await _receiver.ReceiveMessagesAsync(100);

            //separate writes
            await _blobs.WriteTextAsync("one", "test text 1");
            await _blobs.WriteTextAsync("two", "test text 2");

            //with transaction object
            using (ITransaction tx = await _blobs.OpenTransactionAsync())
            {
               await _blobs.WriteTextAsync("three", "test text 1");
               await _blobs.WriteTextAsync("four", "test text 2");

               await tx.CommitAsync();
            }

            IEnumerable<BlobId> keys = await _blobs.ListAsync(null);

            string textBack = await _blobs.ReadTextAsync("one");
            textBack = await _blobs.ReadTextAsync("two");
         }
         catch(Exception ex)
         {
            throw;
         }

         IReliableDictionary<string, long> myDictionary = await StateManager.GetOrAddAsync<IReliableDictionary<string, long>>("myDictionary");

         while (true)
         {
            cancellationToken.ThrowIfCancellationRequested();

            using (SFT tx = this.StateManager.CreateTransaction())
            {
               Microsoft.ServiceFabric.Data.ConditionalValue<long> result = await myDictionary.TryGetValueAsync(tx, "Counter");

               ServiceEventSource.Current.ServiceMessage(this.Context, "Current Counter Value: {0}",
                   result.HasValue ? result.Value.ToString() : "Value does not exist.");

               await myDictionary.AddOrUpdateAsync(tx, "Counter", 0, (key, value) => ++value);

               // If an exception is thrown before calling CommitAsync, the transaction aborts, all changes are 
               // discarded, and nothing is saved to the secondary replicas.
               await tx.CommitAsync();
            }

            await Task.Delay(TimeSpan.FromSeconds(1), cancellationToken);
         }
      }

      private async Task OnNewMessages(IEnumerable<QueueMessage> messages)
      {
         ServiceEventSource.Current.ServiceMessage(this.Context, "received message(s)");
      }
   }
}

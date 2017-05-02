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

namespace Stateful1
{
   /// <summary>
   /// An instance of this class is created for each service replica by the Service Fabric runtime.
   /// </summary>
   internal sealed class Stateful1 : StatefulService
   {
      private readonly IBlobStorage _blobs;
      private readonly IMessagePublisher _publisher;
      private readonly IMessageReceiver _receiver;

      public Stateful1(StatefulServiceContext context)
          : base(context)
      {
         _blobs = StorageFactory.Blobs.AzureServiceFabricReliableStorage(this.StateManager, "c1");
         _publisher = StorageFactory.Messages.AzureServiceFabricReliableQueuePublisher(this.StateManager);
         _receiver = StorageFactory.Messages.AzureServiceFabricReliableQueueReceiver(this.StateManager);
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
            IEnumerable<QueueMessage> qm = await _receiver.ReceiveMessagesAsync(100);

            await _publisher.PutMessagesAsync(new[] { QueueMessage.FromText("content at " + DateTime.UtcNow) });

            qm = await _receiver.ReceiveMessagesAsync(100);

            await _blobs.UploadTextAsync("one", "test text 1");
            await _blobs.UploadTextAsync("two", "test text 2");

            IEnumerable<string> keys = await _blobs.ListAsync(null);

            string textBack = await _blobs.DownloadTextAsync("one");
            textBack = await _blobs.DownloadTextAsync("two");
         }
         catch(Exception ex)
         {
            throw;
         }

         var myDictionary = await this.StateManager.GetOrAddAsync<IReliableDictionary<string, long>>("myDictionary");

         while (true)
         {
            cancellationToken.ThrowIfCancellationRequested();

            using (var tx = this.StateManager.CreateTransaction())
            {
               var result = await myDictionary.TryGetValueAsync(tx, "Counter");

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
   }
}

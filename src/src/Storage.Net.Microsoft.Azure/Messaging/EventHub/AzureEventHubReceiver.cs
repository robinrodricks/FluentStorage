using Storage.Net.Messaging;
using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.Azure.EventHubs;
using System.Collections.Generics;
using Storage.Net.Blob;
using System.Threading;

namespace Storage.Net.Microsoft.Azure.Messaging.EventHub
{
   /// <summary>
   /// Microsoft Azure Event Hub receiver
   /// </summary>
   public class AzureEventHubReceiver : AsyncMessageReceiver
   {
      private readonly EventHubClient _hubClient;
      private readonly HashSet<string> _partitionIds;
      private readonly string _consumerGroupName;
      private readonly EventHubStateAdapter _state;
      private readonly CancellationTokenSource _tokenSource = new CancellationTokenSource();
      private const int MaxMessageCount = 10;
      private static readonly TimeSpan WaitTime = TimeSpan.FromMinutes(1);
      private readonly List<PartitionReceiver> _partitonReceivers = new List<PartitionReceiver>();
      private bool _isReady;

      /// <summary>
      /// Creates an instance of EventHub receiver
      /// </summary>
      /// <param name="connectionString">Event hub connection string</param>
      /// <param name="hubPath">Entity path</param>
      /// <param name="partitionIds">When specified, listens only on specified partition(s), otherwise all partitions will be used</param>
      /// <param name="consumerGroupName">When not specified uses default consumer group</param>
      /// <param name="stateStorage">When specified uses this storage for persisting the state i.e. will be able to continue from
      /// where it left next time it runs</param>
      public AzureEventHubReceiver(string connectionString, string hubPath,
         IEnumerable<string> partitionIds = null,
         string consumerGroupName = null,
         IBlobStorage stateStorage = null)
      {
         if (connectionString == null)
            throw new ArgumentNullException(nameof(connectionString));

         if (hubPath == null)
            throw new ArgumentNullException(nameof(hubPath));

         _partitionIds = partitionIds == null ? null : new HashSet<string>(partitionIds);

         var builder = new EventHubsConnectionStringBuilder(connectionString) { EntityPath = hubPath };
         _hubClient = EventHubClient.CreateFromConnectionString(builder.ToString());
         if (partitionIds != null) _partitionIds.AddRange(_partitionIds);
         _consumerGroupName = consumerGroupName;
         _state = new EventHubStateAdapter(stateStorage);
      }

      /// <summary>
      /// Gets the IDs of event hub partitions
      /// </summary>
      /// <returns></returns>
      public async Task<IEnumerable<string>> GetPartitionIds()
      {
         EventHubRuntimeInformation info = await _hubClient.GetRuntimeInformationAsync();

         return info.PartitionIds;
      }

      private async Task CheckReady()
      {
         if (_isReady) return;

         await CreateReceivers();

         _isReady = true;
      }

      private async Task CreateReceivers()
      {
         EventHubRuntimeInformation runtimeInfo = await _hubClient.GetRuntimeInformationAsync();

         foreach (string partitionId in runtimeInfo.PartitionIds)
         {
            if (_partitionIds == null || _partitionIds.Contains(partitionId))
            {
               PartitionReceiver receiver = _hubClient.CreateReceiver(
                  _consumerGroupName ?? PartitionReceiver.DefaultConsumerGroupName,
                  partitionId,
                  (await _state.GetPartitionOffset(partitionId)),
                  false);

               _partitonReceivers.Add(receiver);
            }
         }
      }


      /// <summary>
      /// See interface
      /// </summary>
      public override Task ConfirmMessageAsync(QueueMessage message)
      {
         //nothing to confirm
         return Task.FromResult(true);
      }

      /// <summary>
      /// See interface
      /// </summary>
      public override void ConfirmMessage(QueueMessage message)
      {
         //nothing to confirm
      }

      /// <summary>
      /// See interface
      /// </summary>
      public override Task DeadLetterAsync(QueueMessage message, string reason, string errorDescription)
      {
         //no dead letter queue in EH
         return Task.FromResult(true);
      }

      /// <summary>
      /// See interface
      /// </summary>
      public override void DeadLetter(QueueMessage message, string reason, string errorDescription)
      {
         //no dead letter queue in EH
      }

      /// <summary>
      /// See interface
      /// </summary>
      public override Task<IEnumerable<QueueMessage>> ReceiveMessagesAsync(int count)
      {
         return Task.FromResult<IEnumerable<QueueMessage>>(null);
      }

      /// <summary>
      /// See interface
      /// </summary>
      public override async Task StartMessagePumpAsync(Func<QueueMessage, Task> onMessageAsync)
      {
         await CheckReady();

         foreach(PartitionReceiver receiver in _partitonReceivers)
         {
            Task pump = ReceiverPump(receiver, onMessageAsync);
         }
      }

      private async Task ReceiverPump(PartitionReceiver receiver, Func<QueueMessage, Task> onMessage)
      {
         while (true)
         {
            IEnumerable<EventData> events = await receiver.ReceiveAsync(MaxMessageCount, WaitTime);

            if(events != null)
            {
               QueueMessage lastMessage = null;

               foreach(EventData ed in events)
               {
                  QueueMessage qm = Converter.ToQueueMessage(ed, receiver.PartitionId);

                  await onMessage(qm);

                  lastMessage = qm;
               }

               //save state
               if(lastMessage != null)
               {
                  const string sequenceNumberPropertyName = "x-opt-sequence-number";
                  const string offsetPropertyName = "x-opt-offset";

                  if(lastMessage.Properties.TryGetValue(offsetPropertyName, out string offset))
                  {
                     lastMessage.Properties.TryGetValue(sequenceNumberPropertyName, out string sequenceNumber);

                     await _state.SetPartitionState(receiver.PartitionId, offset, sequenceNumber);
                  }
               }
            }

            if (_tokenSource.IsCancellationRequested)
            {
               await receiver.CloseAsync();
               return;
            }
         }
      }

      /// <summary>
      /// Cancels message pump and closes the client
      /// </summary>
      public override void Dispose()
      {
         _tokenSource.Cancel();
         base.Dispose();
      }
   }
}

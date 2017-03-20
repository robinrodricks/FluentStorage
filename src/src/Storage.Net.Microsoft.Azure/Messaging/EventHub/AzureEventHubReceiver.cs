using Storage.Net.Messaging;
using System;

namespace Storage.Net.Microsoft.Azure.Messaging.EventHub
{
   //todo: default implementation requires blob storage for checkpoints and leases, i'm not even sure what that is
   //but i don't want an extra dependency

   class AzureEventHubReceiver : AsyncMessageReceiver
   {
      private readonly string _connectionString;
      private readonly string _hubPath;

      public AzureEventHubReceiver(string connectionString, string hubPath)
      {
         _connectionString = connectionString;
         _hubPath = hubPath;
      }

      public override void StartMessagePump(Action<QueueMessage> onMessage)
      {
         //var host = new EventProcessorHost(_hubPath,
         //   PartitionReceiver.DefaultConsumerGroupName,
         //   _connectionString)

         //PartitionReceiver.DefaultConsumerGroupName


         throw new NotImplementedException();
      }
   }
}

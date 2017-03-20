using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using Storage.Net.Messaging;
using Microsoft.Azure.EventHubs;
using System;

namespace Storage.Net.Microsoft.Azure.Messaging.EventHub
{
   /// <summary>
   /// Publishes messages to Azure Event Hub
   /// </summary>
   class AzureEventHubPublisher : AsyncMessagePublisher
   {
      private EventHubClient _client;

      public AzureEventHubPublisher(string connectionString, string path)
      {
         if (connectionString == null) throw new ArgumentNullException(nameof(connectionString));
         if (path == null) throw new ArgumentNullException(nameof(path));

         var csb = new EventHubsConnectionStringBuilder(connectionString)
         {
            EntityPath = path
         };

         _client = EventHubClient.CreateFromConnectionString(csb.ToString());

      }

      public override async Task PutMessagesAsync(IEnumerable<QueueMessage> messages)
      {
         if (messages == null) return;

         await _client.SendAsync(messages.Select(Converter.ToEventData));
      }

      public override void Dispose()
      {
         _client.CloseAsync().Wait();

         base.Dispose();
      }
   }
}

using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using Storage.Net.Messaging;
using Microsoft.Azure.EventHubs;
using System;
using System.Threading;

namespace Storage.Net.Microsoft.Azure.EventHub
{
   /// <summary>
   /// Publishes messages to Azure Event Hub
   /// </summary>
   class AzureEventHubMessenger : IMessenger
   {
      private readonly EventHubClient _client;

      /// <summary>
      /// Creates an instance of event hub publisher by full connection string
      /// </summary>
      /// <param name="connectionString">Full connection string</param>
      public AzureEventHubMessenger(string connectionString)
      {
         _client = EventHubClient.CreateFromConnectionString(connectionString);
      }

      /// <summary>
      /// Create with connection string and entity path
      /// </summary>
      /// <param name="connectionString">Connection string</param>
      /// <param name="path">Entity path</param>
      /// <returns></returns>
      public static AzureEventHubMessenger Create(string connectionString, string path)
      {
         if (connectionString == null) throw new ArgumentNullException(nameof(connectionString));
         if (path == null) throw new ArgumentNullException(nameof(path));

         var csb = new EventHubsConnectionStringBuilder(connectionString)
         {
            EntityPath = path
         };

         return new AzureEventHubMessenger(csb.ToString());
      }

      /// <summary>
      /// See interface
      /// </summary>
      public async Task PutMessagesAsync(IReadOnlyCollection<QueueMessage> messages, CancellationToken cancellationToken = default)
      {
         if (messages == null) return;

         await _client.SendAsync(messages.Select(Converter.ToEventData));
      }

      /// <summary>
      /// Closes the receiver
      /// </summary>
      public void Dispose()
      {
         _client.CloseAsync().Wait();
      }

      #region [ IMessenger ]

      public Task CreateChannelsAsync(IEnumerable<string> channelNames, CancellationToken cancellationToken = default) => throw new NotImplementedException();
      public Task<IReadOnlyCollection<string>> ListChannelsAsync(CancellationToken cancellationToken = default) => throw new NotImplementedException();
      public Task DeleteChannelsAsync(IEnumerable<string> channelNames, CancellationToken cancellationToken = default) => throw new NotImplementedException();
      public Task<long> GetMessageCountAsync(string channelName, CancellationToken cancellationToken = default) => throw new NotImplementedException();

      public async Task SendAsync(string channelName, IEnumerable<QueueMessage> messages, CancellationToken cancellationToken = default)
      {
         if(messages == null)
            return;

         await _client.SendAsync(messages.Select(Converter.ToEventData)).ConfigureAwait(false);
      }

      public Task<IReadOnlyCollection<QueueMessage>> ReceiveAsync(string channelName, int count = 100, TimeSpan? visibility = null, CancellationToken cancellationToken = default) => throw new NotImplementedException();
      public Task<IReadOnlyCollection<QueueMessage>> PeekAsync(string channelName, int count = 100, CancellationToken cancellationToken = default) => throw new NotImplementedException();

      #endregion
   }
}

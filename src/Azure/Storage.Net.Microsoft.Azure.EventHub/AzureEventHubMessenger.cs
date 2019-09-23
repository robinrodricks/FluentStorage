using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using Storage.Net.Messaging;
using Microsoft.Azure.EventHubs;
using System;
using System.Threading;
using NetBox.Extensions;

namespace Storage.Net.Microsoft.Azure.EventHub
{
   /// <summary>
   /// Publishes messages to Azure Event Hub
   /// </summary>
   class AzureEventHubMessenger : IMessenger
   {
      private readonly Dictionary<string, EventHubClient> _entityNameToClient = new Dictionary<string, EventHubClient>();
      private readonly string _connectionString;
      private readonly string _entityName;

      /// <summary>
      /// Creates an instance of event hub publisher by full connection string
      /// </summary>
      /// <param name="connectionString">Full connection string</param>
      /// <param name="entityName"></param>
      public AzureEventHubMessenger(string connectionString, string entityName)
      {
         _connectionString = connectionString ?? throw new ArgumentNullException(nameof(connectionString));
         _entityName = entityName;
      }

      private EventHubClient GetClient(string channelName)
      {
         if(_entityNameToClient.TryGetValue(channelName, out EventHubClient client))
            return client;

         var csb = new EventHubsConnectionStringBuilder(_connectionString)
         {
            EntityPath = channelName
         };

         return EventHubClient.CreateFromConnectionString(csb.ToString());
      }

      private static Task ThrowManagementNotSupportedException()
      {
         throw new NotSupportedException("EventHub management operations are not supported by this library as they require elevated permissions");
      }

      #region [ IMessenger ]

      public Task CreateChannelsAsync(IEnumerable<string> channelNames, CancellationToken cancellationToken = default)
      {
         return ThrowManagementNotSupportedException();
      }

      public Task<IReadOnlyCollection<string>> ListChannelsAsync(CancellationToken cancellationToken = default)
      {
         return Task.FromResult<IReadOnlyCollection<string>>(new[] { _entityName });
      }

      public Task DeleteChannelsAsync(IEnumerable<string> channelNames, CancellationToken cancellationToken = default)
      {
         if(channelNames is null)
            throw new ArgumentNullException(nameof(channelNames));

         return ThrowManagementNotSupportedException();
      }
      public Task<long> GetMessageCountAsync(string channelName, CancellationToken cancellationToken = default)
      {
         if(channelName is null)
            throw new ArgumentNullException(nameof(channelName));

         ThrowManagementNotSupportedException();

         return Task.FromResult(0L);
      }

      public async Task SendAsync(string channelName, IEnumerable<QueueMessage> messages, CancellationToken cancellationToken = default)
      {
         if(channelName is null)
            throw new ArgumentNullException(nameof(channelName));

         if(messages is null)
            throw new ArgumentNullException(nameof(messages));

         if(channelName != _entityName)
            throw new ArgumentException($"You can only send messages to '{_entityName}' channel", nameof(channelName));

         EventHubClient client = GetClient(channelName);

         await client.SendAsync(messages.Select(Converter.ToEventData)).ConfigureAwait(false);
      }

      public Task<IReadOnlyCollection<QueueMessage>> ReceiveAsync(string channelName, int count = 100, TimeSpan? visibility = null, CancellationToken cancellationToken = default) => throw new NotImplementedException();
      public Task<IReadOnlyCollection<QueueMessage>> PeekAsync(string channelName, int count = 100, CancellationToken cancellationToken = default) => throw new NotImplementedException();

      /// <summary>
      /// Closes the receiver
      /// </summary>
      public void Dispose()
      {
         foreach(KeyValuePair<string, EventHubClient> client in _entityNameToClient)
         {
            client.Value.CloseAsync().Forget();
         }
      }

      public Task DeleteAsync(string channelName, IEnumerable<QueueMessage> messages, CancellationToken cancellationToken = default) => throw new NotImplementedException();

      #endregion
   }
}

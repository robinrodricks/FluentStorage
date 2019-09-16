using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.Storage;
using Microsoft.Azure.Storage.Auth;
using Microsoft.Azure.Storage.Queue;
using Storage.Net.Messaging;

namespace Storage.Net.Microsoft.Azure.Storage.Messaging
{
   class AzureStorageQueueMessenger : IMessenger
   {
      private readonly CloudQueueClient _client;
      private readonly ConcurrentDictionary<string, CloudQueue> _channelNameToQueue =
         new ConcurrentDictionary<string, CloudQueue>();

      public AzureStorageQueueMessenger(
         string accountName, string storageKey)
      {
         if(accountName is null)
            throw new ArgumentNullException(nameof(accountName));
         if(storageKey is null)
            throw new ArgumentNullException(nameof(storageKey));

         var account = new CloudStorageAccount(new StorageCredentials(accountName, storageKey), true);
         _client = account.CreateCloudQueueClient();
      }

      private async Task<CloudQueue> GetQueueAsync(string channelName, bool createIfNotExists = false)
      {
         if(_channelNameToQueue.TryGetValue(channelName, out CloudQueue queue))
            return queue;

         queue = _client.GetQueueReference(channelName);

         if(createIfNotExists)
         {
            await queue.CreateIfNotExistsAsync().ConfigureAwait(false);
         }

         _channelNameToQueue[channelName] = queue;
         return queue;
      }

      #region [ IMessenger ]

      public async Task CreateChannelsAsync(IEnumerable<string> channelNames, CancellationToken cancellationToken = default)
      {
         await Task.WhenAll(channelNames.Select(cn => GetQueueAsync(cn, true))).ConfigureAwait(false);
      }


      public Task<long> GetMessageCountAsync(string channelName, CancellationToken cancellationToken = default) => throw new NotImplementedException();

      public async Task<IReadOnlyCollection<string>> ListChannelsAsync(CancellationToken cancellationToken = default)
      {
         var queueNames = new List<string>();

         QueueContinuationToken token = null;

         do
         {
            QueueResultSegment page = await _client.ListQueuesSegmentedAsync(token).ConfigureAwait(false);

            queueNames.AddRange(page.Results.Select(q => q.Name));

            token = page.ContinuationToken;

         }
         while(token != null);

         return queueNames;
      }

      public async Task DeleteChannelsAsync(IEnumerable<string> channelNames, CancellationToken cancellationToken = default)
      {
         if(channelNames is null)
            throw new ArgumentNullException(nameof(channelNames));

         foreach(string queueName in channelNames)
         {
            CloudQueue queue = _client.GetQueueReference(queueName);
            await queue.DeleteIfExistsAsync(cancellationToken).ConfigureAwait(false);
         }
      }


      public Task<IReadOnlyCollection<QueueMessage>> PeekAsync(string channelName, int count = 100, CancellationToken cancellationToken = default) => throw new NotImplementedException();
      public Task<IReadOnlyCollection<QueueMessage>> ReceiveAsync(string channelName, int count = 100, TimeSpan? visibility = null, CancellationToken cancellationToken = default) => throw new NotImplementedException();

      public async Task SendAsync(string channelName, IEnumerable<QueueMessage> messages, CancellationToken cancellationToken = default)
      {
         CloudQueue queue = await GetQueueAsync(channelName).ConfigureAwait(false);

         await Task.WhenAll(messages.Select(async m =>
         {
            CloudQueueMessage nativeMessage = Converter.ToCloudQueueMessage(m);
            await queue.AddMessageAsync(nativeMessage, cancellationToken).ConfigureAwait(false);
            m.Id = Converter.CreateId(nativeMessage);
         })).ConfigureAwait(false);
      }

      public void Dispose()
      {

      }


      #endregion

   }
}

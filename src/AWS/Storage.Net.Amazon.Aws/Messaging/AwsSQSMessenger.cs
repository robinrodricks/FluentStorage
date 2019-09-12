﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Amazon;
using Amazon.Runtime;
using Amazon.SQS;
using Amazon.SQS.Model;
using Storage.Net.Messaging;
using NetBox.Extensions;
using System.Collections.Concurrent;

namespace Storage.Net.Amazon.Aws.Messaging
{
   class AwsSQSMessenger : IMessenger
   {
      private readonly AmazonSQSClient _client;
      private const int MaxEntriesPerRequest = 10; //SQS limit
      private readonly ConcurrentDictionary<string, string> _queueNameToUri = new ConcurrentDictionary<string, string>();
      private readonly string _serviceUrl;

      /// <summary>
      /// 
      /// </summary>
      /// <param name="accessKeyId"></param>
      /// <param name="secretAccessKey"></param>
      /// <param name="serviceUrl">Serivce URL, for instance http://sqs.us-west-2.amazonaws.com"</param>
      /// <param name="regionEndpoint">Optional regional endpoint</param>
      public AwsSQSMessenger(string accessKeyId, string secretAccessKey, string serviceUrl, RegionEndpoint regionEndpoint)
      {
         var config = new AmazonSQSConfig
         {
            ServiceURL = serviceUrl,
            RegionEndpoint = regionEndpoint ?? RegionEndpoint.USEast1
         };

         _client = new AmazonSQSClient(new BasicAWSCredentials(accessKeyId, secretAccessKey), config);
         _serviceUrl = serviceUrl;
      }

      private string GetQueueUri(string queueName)
      {
         return _queueNameToUri.GetOrAdd(queueName, qn => new Uri(new Uri(_serviceUrl), queueName).ToString());
      }

      #region [ IMessenger ]

      public async Task<IReadOnlyCollection<string>> ListChannelsAsync(CancellationToken cancellationToken = default)
      {
         ListQueuesResponse queues = await _client.ListQueuesAsync(new ListQueuesRequest { }).ConfigureAwait(false);

         return queues.QueueUrls.Select(u => u.Substring(u.LastIndexOf("/") + 1)).ToList();
      }

      public async Task DeleteChannelsAsync(IEnumerable<string> channelNames, CancellationToken cancellationToken = default)
      {
         if(channelNames is null)
            throw new ArgumentNullException(nameof(channelNames));

         foreach(string queueName in channelNames)
         {
            await _client.DeleteQueueAsync(GetQueueUri(queueName), cancellationToken).ConfigureAwait(false);
         }
      }

      public Task<long> GetMessageCountAsync(string channelName, CancellationToken cancellationToken = default) => throw new NotImplementedException();

      public async Task SendAsync(string channelName, IEnumerable<QueueMessage> messages, CancellationToken cancellationToken = default)
      {
         if(channelName is null)
            throw new ArgumentNullException(nameof(channelName));
         if(messages is null)
            throw new ArgumentNullException(nameof(messages));

         string queueUri = GetQueueUri(channelName);

         // SQS request size is limited
         foreach(IEnumerable<QueueMessage> chunk in messages.Chunk(MaxEntriesPerRequest))
         {
            var request = new SendMessageBatchRequest(
               queueUri,
               chunk.Select(Converter.ToSQSMessage).ToList());

            try
            {
               await _client.SendMessageBatchAsync(request, cancellationToken);
            }
            catch(AmazonSQSException ex) when(ex.ErrorCode == "AWS.SimpleQueueService.NonExistentQueue")
            {
               //try to create the queue and send again
               try
               {
                  await _client.CreateQueueAsync(channelName, cancellationToken);
               }
               catch(Exception exi)
               {
                  throw new InvalidOperationException(
                     $"the queue '{channelName}' didn't exist, so we tried to create it. Unfortunately the operation has failed, probably due to permission issues.", exi);
               }

               await _client.SendMessageBatchAsync(request, cancellationToken);
            }
         }
      }

      public Task<IReadOnlyCollection<QueueMessage>> ReceiveAsync(string channelName, int count = 100, TimeSpan? visibility = null, CancellationToken cancellationToken = default) => throw new NotImplementedException();
      public Task<IReadOnlyCollection<QueueMessage>> PeekAsync(string channelName, int count = 100, CancellationToken cancellationToken = default) => throw new NotImplementedException();

      public void Dispose()
      {

      }

      #endregion
   }
}

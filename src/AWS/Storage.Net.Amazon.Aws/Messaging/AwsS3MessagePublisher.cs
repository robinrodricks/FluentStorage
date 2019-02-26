using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Amazon;
using Amazon.SQS;
using Amazon.SQS.Model;
using Storage.Net.Messaging;

namespace Storage.Net.Amazon.Aws.Messaging
{
   class AwsS3MessagePublisher : IMessagePublisher
   {
      private readonly AmazonSQSClient _client;
      private readonly string _queueName;

      /// <summary>
      /// 
      /// </summary>
      /// <param name="serviceUrl">Serivce URL, for instance http://sqs.us-west-2.amazonaws.com"</param>
      /// <param name="regionEndpoint">Optional regional endpoint</param>
      public AwsS3MessagePublisher(string serviceUrl, string queueName, RegionEndpoint regionEndpoint)
      {
         var config = new AmazonSQSConfig
         {
            ServiceURL = serviceUrl,
            RegionEndpoint = regionEndpoint ?? RegionEndpoint.USEast1
         };

         _client = new AmazonSQSClient(config);
         _queueName = queueName;
      }

      public async Task PutMessagesAsync(IEnumerable<QueueMessage> messages, CancellationToken cancellationToken = default)
      {
         var sqs = messages.Select(ToSQSMessage).ToList();

         SendMessageBatchResponse r = await _client.SendMessageBatchAsync(_queueName, sqs, cancellationToken);
      }

      private static SendMessageBatchRequestEntry ToSQSMessage(QueueMessage message)
      {
         var r = new SendMessageBatchRequestEntry(Guid.NewGuid().ToString(), message.StringContent);

         //todo: message attributes

         return r;
      }

      public void Dispose()
      {

      }


   }
}

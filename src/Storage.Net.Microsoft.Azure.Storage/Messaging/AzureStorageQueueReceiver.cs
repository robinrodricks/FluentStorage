using System;
using Storage.Net.Messaging;
using Microsoft.WindowsAzure.Storage.Queue;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Auth;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Storage.Net.Microsoft.Azure.Storage.Messaging
{
   /// <summary>
   /// Queue receiver based on Azure Storage Queues
   /// </summary>
   class AzureStorageQueueReceiver : IMessageReceiver
   {
      private readonly CloudQueueClient _client;
      private readonly string _queueName;
      private readonly CloudQueue _queue;
      private CloudQueue _deadLetterQueue;
      private readonly TimeSpan _messageVisibilityTimeout;
      private readonly TimeSpan _messagePumpPollingTimeout;
      private bool _disposed;

      /// <summary>
      /// Creates an instance of Azure Storage Queue receiver 
      /// </summary>
      /// <param name="accountName">Azure Storage account name</param>
      /// <param name="storageKey">Azure Storage key</param>
      /// <param name="queueName">Queue name</param>
      /// <param name="messageVisibilityTimeout">
      /// Timeout value passed in GetMessage call in the Storage Queue. The value indicates how long the message
      /// will be hidden before it reappears in the queue. Therefore you must call <see cref="ConfirmMessageAsync(QueueMessage, CancellationToken)"/>
      /// to complete and delete it.
      /// </param>
      public AzureStorageQueueReceiver(string accountName, string storageKey, string queueName,
         TimeSpan messageVisibilityTimeout) :
         this(accountName, storageKey, queueName, messageVisibilityTimeout, TimeSpan.FromMinutes(1))
      {
         
      }

      /// <summary>
      /// Creates an instance of Azure Storage Queue receiver 
      /// </summary>
      /// <param name="accountName">Azure Storage account name</param>
      /// <param name="storageKey">Azure Storage key</param>
      /// <param name="queueName">Queue name</param>
      /// <param name="messageVisibilityTimeout">
      /// Timeout value passed in GetMessage call in the Storage Queue. The value indicates how long the message
      /// will be hidden before it reappears in the queue. Therefore you must call <see cref="ConfirmMessageAsync(QueueMessage, CancellationToken)"/>
      /// to complete and delete it.
      /// </param>
      /// <param name="messagePumpPollingTimeout">
      /// Used in conjunction with <see cref="StartMessagePumpAsync(Func{IEnumerable{QueueMessage}, Task}, int, CancellationToken)"/> and indicates how often message pump will ping for new
      /// messages in the queue.
      /// </param>
      public AzureStorageQueueReceiver(string accountName, string storageKey, string queueName,
         TimeSpan messageVisibilityTimeout, TimeSpan messagePumpPollingTimeout)
      {
         var account = new CloudStorageAccount(new StorageCredentials(accountName, storageKey), true);
         _client = account.CreateCloudQueueClient();
         _queueName = queueName;
         _queue = _client.GetQueueReference(queueName);
         _queue.CreateIfNotExistsAsync().Wait();
         _messageVisibilityTimeout = messageVisibilityTimeout;
         _messagePumpPollingTimeout = messagePumpPollingTimeout;
      }

      private async Task<CloudQueue> GetDeadLetterQueue()
      {
         if (_deadLetterQueue == null)
         {
            _deadLetterQueue = _client.GetQueueReference(_queueName + "-deadletter");
            await _deadLetterQueue.CreateIfNotExistsAsync();
         }

         return _deadLetterQueue;
      }

      /// <summary>
      /// Deletes the message from the queue
      /// </summary>
      /// <param name="message"></param>
      /// <param name="cancellationToken"></param>
      public async Task ConfirmMessageAsync(QueueMessage message, CancellationToken cancellationToken)
      {
         Converter.SplitId(message.Id, out string id, out string popReceipt);
         if (popReceipt == null) throw new ArgumentException("cannot delete message by short id", id);

         await _queue.DeleteMessageAsync(id, popReceipt);
      }

      /// <summary>
      /// Moves message to a dead letter queue which has the same name as original queue prefixed with "-deadletter". This is done because 
      /// Azure Storage queues do not support deadlettering directly.
      /// </summary>
      public async Task DeadLetterAsync(QueueMessage message, string reason, string errorDescription, CancellationToken cancellationToken)
      {
         var dead = (QueueMessage)message.Clone();
         dead.Properties["deadLetterReason"] = reason;
         dead.Properties["deadLetterError"] = errorDescription;

         CloudQueue deadLetterQueue = await GetDeadLetterQueue();

         await deadLetterQueue.AddMessageAsync(Converter.ToCloudQueueMessage(message));

         await ConfirmMessageAsync(message, cancellationToken);
      }

      /// <summary>
      /// Due to the fact storage queues don't support notifications this method starts an internal thread to poll for messages.
      /// </summary>
      public async Task StartMessagePumpAsync(Func<IEnumerable<QueueMessage>, Task> onMessage, int maxBatchSize, CancellationToken cancellationToken)
      {
         if (onMessage == null) throw new ArgumentNullException(nameof(onMessage));

         PollTasksAsync(onMessage, maxBatchSize, cancellationToken);
      }

      private async Task PollTasksAsync(Func<IEnumerable<QueueMessage>, Task> callback, int maxBatchSize, CancellationToken cancellationToken)
      {
         if (cancellationToken.IsCancellationRequested) return;

         IEnumerable<QueueMessage> messages = await ReceiveMessagesAsync(maxBatchSize);
         while(messages != null)
         {
            await callback(messages);
            messages = await ReceiveMessagesAsync(maxBatchSize);
         }

         await Task.Delay(_messagePumpPollingTimeout, cancellationToken).ContinueWith(async (t) =>
         {
            await PollTasksAsync(callback, maxBatchSize, cancellationToken);
         });
      }

      /// <summary>
      /// Stops message pump if any.
      /// </summary>
      public void Dispose()
      {
      }

      /// <summary>
      /// Calls .GetMessages on storage queue
      /// </summary>
      private async Task<IEnumerable<QueueMessage>> ReceiveMessagesAsync(int count)
      {
         IEnumerable<CloudQueueMessage> batch = await _queue.GetMessagesAsync(count, _messageVisibilityTimeout, null, null);
         if(batch == null) return null;
         return batch.Select(Converter.ToQueueMessage).ToList();
      }

      public Task<ITransaction> OpenTransactionAsync()
      {
         return Task.FromResult(EmptyTransaction.Instance);
      }
   }
}

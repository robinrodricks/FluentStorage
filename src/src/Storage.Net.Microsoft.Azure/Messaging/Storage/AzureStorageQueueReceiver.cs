using System;
using Storage.Net.Messaging;
using Microsoft.WindowsAzure.Storage.Queue;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Auth;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Storage.Net.Microsoft.Azure.Messaging.Storage
{
   /// <summary>
   /// Queue receiver based on Azure Storage Queues
   /// </summary>
   public class AzureStorageQueueReceiver : IMessageReceiver
   {
      private const int PollingBatchSize = 5;
      private readonly CloudQueueClient _client;
      private readonly string _queueName;
      private readonly CloudQueue _queue;
      private CloudQueue _deadLetterQueue;
      private readonly TimeSpan _messageVisibilityTimeout;
      private readonly TimeSpan _messagePumpPollingTimeout;
      private Thread _pollingThread;
      private bool _disposed;
      private Action<QueueMessage> _onMessageAction;
      private readonly object _createLock = new object();

      /// <summary>
      /// Creates an instance of Azure Storage Queue receiver 
      /// </summary>
      /// <param name="accountName">Azure Storage account name</param>
      /// <param name="storageKey">Azure Storage key</param>
      /// <param name="queueName">Queue name</param>
      /// <param name="messageVisibilityTimeout">
      /// Timeout value passed in GetMessage call in the Storage Queue. The value indicates how long the message
      /// will be hidden before it reappears in the queue. Therefore you must call <see cref="ConfirmMessage(QueueMessage)"/>
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
      /// will be hidden before it reappears in the queue. Therefore you must call <see cref="ConfirmMessage(QueueMessage)"/>
      /// to complete and delete it.
      /// </param>
      /// <param name="messagePumpPollingTimeout">
      /// Used in conjunction with <see cref="StartMessagePump(Action{QueueMessage})"/> and indicates how often message pump will ping for new
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
      public void ConfirmMessage(QueueMessage message)
      {
         ConfirmMessageAsync(message).Wait();
      }


      /// <summary>
      /// Deletes the message from the queue
      /// </summary>
      /// <param name="message"></param>
      private async Task ConfirmMessageAsync(QueueMessage message)
      {
         string id, popReceipt;
         Converter.SplitId(message.Id, out id, out popReceipt);
         if(popReceipt == null) throw new ArgumentException("cannot delete message by short id", id);
         await _queue.DeleteMessageAsync(id, popReceipt);
      }

      /// <summary>
      /// Moves message to a dead letter queue which has the same name as original queue prefixed with "-deadletter". This is done because 
      /// Azure Storage queues do not support deadlettering directly.
      /// </summary>
      public void DeadLetter(QueueMessage message, string reason, string errorDescription)
      {
         DeadLetterAsync(message, reason, errorDescription).Wait();
      }

      /// <summary>
      /// Moves message to a dead letter queue which has the same name as original queue prefixed with "-deadletter". This is done because 
      /// Azure Storage queues do not support deadlettering directly.
      /// </summary>
      public async Task DeadLetterAsync(QueueMessage message, string reason, string errorDescription)
      {
         var dead = (QueueMessage)message.Clone();
         dead.Properties["deadLetterReason"] = reason;
         dead.Properties["deadLetterError"] = errorDescription;

         CloudQueue deadLetterQueue = await GetDeadLetterQueue();

         await deadLetterQueue.AddMessageAsync(Converter.ToCloudQueueMessage(message));

         await ConfirmMessageAsync(message);
      }

      /// <summary>
      /// Due to the fact storage queues don't support notifications this method starts an internal thread to poll for messages.
      /// </summary>
      public void StartMessagePump(Action<QueueMessage> onMessage)
      {
         if (onMessage == null) throw new ArgumentNullException(nameof(onMessage));
         if (_pollingThread != null) throw new ArgumentException("polling already started", nameof(onMessage));

         _onMessageAction = onMessage;
         _pollingThread = new Thread(PollingThread) { IsBackground = true };
         _pollingThread.Start();
      }

      private void PollingThread()
      {
         while (!_disposed)
         {
            //remember if there were messages in current loop and fetch more immediately instead of waiting for another poll
            bool hadSome = true;

            while (hadSome)
            {
               hadSome = false;

               IEnumerable<QueueMessage> messages = ReceiveMessages(PollingBatchSize);
               if(messages != null)
               {
                  foreach(QueueMessage qm in messages)
                  {
                     hadSome = true;
                     _onMessageAction(qm);
                  }
               }
            }
            

            Thread.Sleep(_messagePumpPollingTimeout);

            //todo: think of smart polling, regular intervals are too expensive
         }
      }

      /// <summary>
      /// Stops message pump if any.
      /// </summary>
      public void Dispose()
      {
         if (!_disposed)
         {
            _disposed = true;
            _pollingThread = null;
         }
      }

      /// <summary>
      /// Calls .GetMessage on storage queue
      /// </summary>
      public QueueMessage ReceiveMessage()
      {
         return ReceiveMessageAsync().Result;
      }


      /// <summary>
      /// Calls .GetMessage on storage queue
      /// </summary>
      private async Task<QueueMessage> ReceiveMessageAsync()
      {
         CloudQueueMessage message = await _queue.GetMessageAsync(_messageVisibilityTimeout, null, null);
         if(message == null) return null;

         return Converter.ToQueueMessage(message);
      }

      /// <summary>
      /// Calls .GetMessages on storage queue
      /// </summary>
      public IEnumerable<QueueMessage> ReceiveMessages(int count)
      {
         return ReceiveMessagesAsync(count).Result;
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
   }
}

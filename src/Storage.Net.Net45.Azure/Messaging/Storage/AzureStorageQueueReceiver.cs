using System;
using Storage.Net.Messaging;
using Microsoft.WindowsAzure.Storage.Queue;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Auth;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace Storage.Net.Azure.Messaging.Storage
{
   /// <summary>
   /// Queue receiver based on Azure Storage Queues
   /// </summary>
   public class AzureStorageQueueReceiver : IMessageReceiver
   {
      private const int PollingBatchSize = 5;
      private readonly CloudQueue _queue;
      private readonly TimeSpan _messageVisibilityTimeout;
      private readonly TimeSpan _messagePumpPollingTimeout;
      private Thread _pollingThread;
      private bool _disposed;
      private Action<QueueMessage> _onMessageAction;

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
         var client = account.CreateCloudQueueClient();
         _queue = client.GetQueueReference(queueName);
         _queue.CreateIfNotExists();
         _messageVisibilityTimeout = messageVisibilityTimeout;
         _messagePumpPollingTimeout = messagePumpPollingTimeout;
      }

      /// <summary>
      /// Deletes the message from the queue
      /// </summary>
      /// <param name="message"></param>
      public void ConfirmMessage(QueueMessage message)
      {
         string id, popReceipt;
         Converter.SplitId(message.Id, out id, out popReceipt);
         if(popReceipt == null) throw new ArgumentException("cannot delete message by short id", id);
         _queue.DeleteMessage(id, popReceipt);
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
         CloudQueueMessage message = _queue.GetMessage(_messageVisibilityTimeout);
         if(message == null) return null;

         return Converter.ToQueueMessage(message);
      }

      /// <summary>
      /// Calls .GetMessages on storage queue
      /// </summary>
      public IEnumerable<QueueMessage> ReceiveMessages(int count)
      {
         IEnumerable<CloudQueueMessage> batch = _queue.GetMessages(count, _messageVisibilityTimeout);
         if(batch == null) return null;
         return batch.Select(Converter.ToQueueMessage).ToList();
      }
   }
}

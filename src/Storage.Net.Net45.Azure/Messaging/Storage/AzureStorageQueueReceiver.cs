using System;
using Storage.Net.Messaging;
using Microsoft.WindowsAzure.Storage.Queue;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Auth;
using System.Collections.Generic;
using System.Linq;

namespace Storage.Net.Azure.Messaging.Storage
{
   /// <summary>
   /// Queue receiver based on Azure Storage Queues
   /// </summary>
   public class AzureStorageQueueReceiver : IMessageReceiver
   {
      private readonly CloudQueue _queue;
      private readonly TimeSpan _messageVisibilityTimeout;

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
      public AzureStorageQueueReceiver(string accountName, string storageKey, string queueName, TimeSpan messageVisibilityTimeout)
      {
         var account = new CloudStorageAccount(new StorageCredentials(accountName, storageKey), true);
         var client = account.CreateCloudQueueClient();
         _queue = client.GetQueueReference(queueName);
         _queue.CreateIfNotExists();
         _messageVisibilityTimeout = messageVisibilityTimeout;
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
      /// Doesn't do anything
      /// </summary>
      public void Dispose()
      {
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

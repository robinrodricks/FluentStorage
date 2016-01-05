using System;
using Storage.Net.Messaging;
using Microsoft.WindowsAzure.Storage.Queue;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Auth;

namespace Storage.Net.Azure.Queue.Storage
{
   /// <summary>
   /// Queue receiver based on Azure Storage Queues
   /// </summary>
   public class AzureStorageQueueReceiver : IMessageReceiver
   {
      private readonly CloudQueue _queue;

      /// <summary>
      /// Creates an instance of Azure Storage Queue receiver 
      /// </summary>
      /// <param name="accountName">Azure Storage account name</param>
      /// <param name="storageKey">Azure Storage key</param>
      /// <param name="queueName">Queue name</param>
      /// <param name="pingInverval">
      /// Ping interval. Due to the fact that Storage Queues have to be periodically
      /// pinged for new messages you have to choose this interval wisely. Each call is billed separately.
      /// </param>
      public AzureStorageQueueReceiver(string accountName, string storageKey, string queueName,
         TimeSpan pingInverval)
      {
         var account = new CloudStorageAccount(new StorageCredentials(accountName, storageKey), true);
         var client = account.CreateCloudQueueClient();
         _queue = client.GetQueueReference(queueName);
         _queue.CreateIfNotExists();
      }

      /// <summary>
      /// Fired when a new message is received.
      /// </summary>
      public event EventHandler<QueueMessage> OnNewMessage;

      /// <summary>
      /// Deletes the message from the queue
      /// </summary>
      /// <param name="message"></param>
      public void ConfirmMessage(QueueMessage message)
      {
         throw new NotImplementedException();
      }

      /// <summary>
      /// Stops the pinging thread
      /// </summary>
      public void Dispose()
      {
      }
      /*
      public QueueMessage GetMessage(TimeSpan? visibilityTimeout)
      {
         CloudQueueMessage message = _queue.GetMessage(visibilityTimeout);
         if(message == null) return null;

         return new QueueMessage(CreateId(message), message.AsString);
      }

      public QueueMessage PeekMesssage()
      {
         CloudQueueMessage message = _queue.PeekMessage();
         if(message == null) return null;

         return new QueueMessage(CreateId(message), message.AsString);
      }

      public void DeleteMessage(string id)
      {
         string id2, pop;
         SplitId(id, out id2, out pop);
         if(pop == null) throw new ArgumentException(@"cannot delete message by short id, use GetMessage to get the message with full id first", id);

         _queue.DeleteMessage(id2, pop);
      }

      public void Clear()
      {
         _queue.Clear();
      }

      private string CreateId(CloudQueueMessage message)
      {
         if(string.IsNullOrEmpty(message.PopReceipt)) return message.Id;

         return message.Id + ":" + message.PopReceipt;
      }

      private void SplitId(string compositeId, out string id, out string popReceipt)
      {
         string[] parts = compositeId.Split(new[] { ':' }, 2, StringSplitOptions.RemoveEmptyEntries);
         id = parts[0];
         popReceipt = parts.Length > 1 ? parts[1] : null;
      }*/

   }
}

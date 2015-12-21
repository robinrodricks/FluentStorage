using System;
using Storage.Net.Messaging;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Auth;
using Microsoft.WindowsAzure.Storage.Queue;

namespace Storage.Net.Azure.Queue
{
   public class AzureMessageQueue : IMessageQueue
   {
      private readonly CloudQueue _queue;

      public AzureMessageQueue(string accountName, string storageKey, string queueName)
      {
         if (accountName == null) throw new ArgumentNullException(nameof(accountName));
         if (storageKey == null) throw new ArgumentNullException(nameof(storageKey));
         if (queueName == null) throw new ArgumentNullException(nameof(queueName));

         var account = new CloudStorageAccount(new StorageCredentials(accountName, storageKey), true);
         var client = account.CreateCloudQueueClient();
         _queue = client.GetQueueReference(queueName);
         _queue.CreateIfNotExists();
      }

      public void PutMessage(string content)
      {
         var message = new CloudQueueMessage(content);

         _queue.AddMessage(message);
      }

      public QueueMessage GetMessage(TimeSpan? visibilityTimeout)
      {
         CloudQueueMessage message = _queue.GetMessage(visibilityTimeout);
         if (message == null) return null;

         return new QueueMessage(CreateId(message), message.AsString);
      }

      public QueueMessage PeekMesssage()
      {
         CloudQueueMessage message = _queue.PeekMessage();
         if (message == null) return null;

         return new QueueMessage(CreateId(message), message.AsString);
      }

      public void DeleteMessage(string id)
      {
         string id2, pop;
         SplitId(id, out id2, out pop);
         if (pop == null) throw new ArgumentException(@"cannot delete message by short id, use GetMessage to get the message with full id first", id);

         _queue.DeleteMessage(id2, pop);
      }

      public void Clear()
      {
         _queue.Clear();
      }

      private string CreateId(CloudQueueMessage message)
      {
         if (string.IsNullOrEmpty(message.PopReceipt)) return message.Id;

         return message.Id + ":" + message.PopReceipt;
      }

      private void SplitId(string compositeId, out string id, out string popReceipt)
      {
         string[] parts = compositeId.Split(new[] { ':' }, 2, StringSplitOptions.RemoveEmptyEntries);
         id = parts[0];
         popReceipt = parts.Length > 1 ? parts[1] : null;
      }
   }
}

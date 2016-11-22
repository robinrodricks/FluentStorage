using System;
using Storage.Net.Messaging;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Auth;
using Microsoft.WindowsAzure.Storage.Queue;
using System.Collections.Generic;

namespace Storage.Net.Microsoft.Azure.Messaging.Storage
{
   /// <summary>
   /// Azure Storage queue publisher
   /// </summary>
   public class AzureStorageQueuePublisher : IMessagePublisher
   {
      private readonly CloudQueue _queue;

      /// <summary>
      /// Creates an instance of Azure Storage Queue by account details and the queue name
      /// </summary>
      /// <param name="accountName">Storage account name</param>
      /// <param name="storageKey">Storage key (primary or secondary)</param>
      /// <param name="queueName">Name of the queue. If queue doesn't exist it will be created</param>
      public AzureStorageQueuePublisher(string accountName, string storageKey, string queueName)
      {
         if(accountName == null) throw new ArgumentNullException(nameof(accountName));
         if(storageKey == null) throw new ArgumentNullException(nameof(storageKey));
         if(queueName == null) throw new ArgumentNullException(nameof(queueName));

         var account = new CloudStorageAccount(new StorageCredentials(accountName, storageKey), true);
         var client = account.CreateCloudQueueClient();
         _queue = client.GetQueueReference(queueName);
         _queue.CreateIfNotExists();
      }

      /// <summary>
      /// Pushes a new message to storage queue
      /// </summary>
      public void PutMessage(QueueMessage message)
      {
         if(message == null) return;
         CloudQueueMessage nativeMessage = Converter.ToCloudQueueMessage(message);
         _queue.AddMessage(nativeMessage);
      }

      /// <summary>
      /// Pushes new messages to storage queue. Due to the fact storage queues don't support batched pushes
      /// this method makes a call per message.
      /// </summary>
      public void PutMessages(IEnumerable<QueueMessage> messages)
      {
         if(messages == null) return;
         foreach(QueueMessage message in messages)
         {
            PutMessage(message);
         }
      }

      /// <summary>
      /// Nothing to dispose
      /// </summary>
      public void Dispose()
      {
      }
   }
}

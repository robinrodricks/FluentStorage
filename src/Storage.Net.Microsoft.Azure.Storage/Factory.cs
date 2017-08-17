using System;
using Storage.Net.Blob;
using Storage.Net.Messaging;
using Storage.Net.Microsoft.Azure.Storage.Blob;
using Storage.Net.Microsoft.Azure.Storage.Messaging;
using Storage.Net.Microsoft.Azure.Storage.Table;
using Storage.Net.Table;
using System.Net;

namespace Storage.Net
{
   /// <summary>
   /// Factory class that implement factory methods for Microsoft Azure implememtations
   /// </summary>
   public static class Factory
   {
      /// <summary>
      /// Creates an instance of Azure Table Storage using account name and key.
      /// </summary>
      /// <param name="factory">Factory reference</param>
      /// <param name="accountName">Account name</param>
      /// <param name="storageKey">Account key</param>
      /// <returns></returns>
      public static ITableStorage AzureTableStorage(this ITableStorageFactory factory,
         string accountName,
         string storageKey)
      {
         return new AzureTableStorage(accountName, storageKey);
      }

      /// <summary>
      /// Creates an instance of Azure Table Storage using account name and key.
      /// </summary>
      /// <param name="factory">Factory reference</param>
      /// <param name="credential">Credential structure cotnaining account name in username and account key in password.</param>
      /// <returns></returns>
      public static ITableStorage AzureTableStorage(this ITableStorageFactory factory,
         NetworkCredential credential)
      {
         return new AzureTableStorage(credential.UserName, credential.Password);
      }

      /// <summary>
      /// Creates a blob storage implementation based on Microsoft Azure Blob Storage using account name and key.
      /// </summary>
      /// <param name="factory">Reference to factory</param>
      /// <param name="accountName">Storage Account name</param>
      /// <param name="key">Storage Account key</param>
      /// <param name="containerName">Container name in the blob storage. If the container doesn't exist it will be automatically
      /// created for you.</param>
      /// <returns>Generic blob storage interface</returns>
      public static IBlobStorageProvider AzureBlobStorage(this IBlobStorageFactory factory,
         string accountName,
         string key,
         string containerName)
      {
         return new AzureBlobStorageProvider(accountName, key, containerName);
      }

      /// <summary>
      /// Creates a blob storage implementation based on Microsoft Azure Blob Storage using account name and key.
      /// </summary>
      /// <param name="factory">Reference to factory</param>
      /// <param name="credential">Credential structure cotnaining account name in username and account key in password.</param>
      /// <param name="containerName">Container name in the blob storage. If the container doesn't exist it will be automatically
      /// created for you.</param>
      /// <returns>Generic blob storage interface</returns>
      public static IBlobStorageProvider AzureBlobStorage(this IBlobStorageFactory factory,
         NetworkCredential credential,
         string containerName)
      {
         return new AzureBlobStorageProvider(credential, containerName);
      }

      /// <summary>
      /// Creates a blob storage implementation
      /// </summary>
      /// <param name="factory">Reference to factory</param>
      /// <param name="connectionString">Storage account connection string</param>
      /// <param name="containerName">Container name in the blob storage. If the container doesn't exist it will be automatically
      /// create for you.</param>
      /// <returns>Generic blob storage  interface</returns>
      public static IBlobStorageProvider AzureBlobStorage(this IBlobStorageFactory factory,
         string connectionString,
         string containerName)
      {
         return new AzureBlobStorageProvider(connectionString, containerName);
      }

      /// <summary>
      /// Creates an instance of a publisher to Azure Storage Queues
      /// </summary>
      /// <param name="factory">Factory reference</param>
      /// <param name="accountName">Account name</param>
      /// <param name="storageKey">Storage key</param>
      /// <param name="queueName">Queue name</param>
      /// <returns>Generic message publisher interface</returns>
      public static IMessagePublisher AzureStorageQueuePublisher(this IMessagingFactory factory,
         string accountName,
         string storageKey,
         string queueName)
      {
         return new AzureStorageQueuePublisher(accountName, storageKey, queueName);
      }

      /// <summary>
      /// Creates an instance of a receiver from Azure Storage Queues
      /// </summary>
      /// <param name="factory">Factory reference</param>
      /// <param name="accountName">Account name</param>
      /// <param name="storageKey">Storage key</param>
      /// <param name="queueName">Queue name</param>
      /// <param name="messageVisibilityTimeout">Message visibility timeout</param>
      /// <returns>Generic message receiver interface</returns>
      public static IMessageReceiver AzureStorageQueueReceiver(this IMessagingFactory factory,
         string accountName,
         string storageKey,
         string queueName,
         TimeSpan messageVisibilityTimeout)
      {
         return new AzureStorageQueueReceiver(accountName, storageKey, queueName, messageVisibilityTimeout);
      }
      /// <summary>
      /// Creates an instance of a receiver from Azure Storage Queues
      /// </summary>
      /// <param name="factory">Factory reference</param>
      /// <param name="accountName">Account name</param>
      /// <param name="storageKey">Storage key</param>
      /// <param name="queueName">Queue name</param>
      /// <param name="messageVisibilityTimeout">Message visibility timeout</param>
      /// <param name="messagePollingInterval">Storage Queues do not support listening therefore internally we poll for new messages. This parameters
      /// indicates how often this happens</param>
      /// <returns>Generic message receiver interface</returns>
      public static IMessageReceiver AzureStorageQueueReceiver(this IMessagingFactory factory,
         string accountName,
         string storageKey,
         string queueName,
         TimeSpan messageVisibilityTimeout,
         TimeSpan messagePollingInterval)
      {
         return new AzureStorageQueueReceiver(accountName, storageKey, queueName, messageVisibilityTimeout, messagePollingInterval);
      }

   }
}

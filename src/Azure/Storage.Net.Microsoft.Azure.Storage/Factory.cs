using System;
using Storage.Net.Blobs;
using Storage.Net.Messaging;
using Storage.Net.Microsoft.Azure.Storage.Messaging;
using Storage.Net.KeyValue;
using System.Net;
using Storage.Net.Microsoft.Azure.Storage;
using Microsoft.Azure.Storage.Blob;
using Storage.Net.Microsoft.Azure.Storage.KeyValue;
using Storage.Net.ConnectionString;
using Storage.Net.Microsoft.Azure.Storage.Blobs;

namespace Storage.Net
{
   /// <summary>
   /// Factory class that implement factory methods for Microsoft Azure implememtations
   /// </summary>
   public static class Factory
   {

      /// <summary>
      /// Register Azure module.
      /// </summary>
      public static IModulesFactory UseAzureStorage(this IModulesFactory factory)
      {
         return factory.Use(new Module());
      }

      /// <summary>
      /// Creates a blob storage implementation based on Microsoft Azure Files.
      /// </summary>
      /// <param name="factory">Reference to factory</param>
      /// <param name="accountName">Storage Account name</param>
      /// <param name="key">Storage Account key</param>
      /// <returns>Generic blob storage interface</returns>
      public static IBlobStorage AzureFiles(this IBlobStorageFactory factory,
         string accountName,
         string key)
      {
         return AzureFilesBlobStorage.CreateFromAccountNameAndKey(accountName, key);
      }

      /// <summary>
      /// Creates an instance of Azure Table Storage using account name and key.
      /// </summary>
      /// <param name="factory">Factory reference</param>
      /// <param name="accountName">Account name</param>
      /// <param name="storageKey">Account key</param>
      /// <returns></returns>
      public static IKeyValueStorage AzureTableStorage(this IKeyValueStorageFactory factory,
         string accountName,
         string storageKey)
      {
         return new AzureTableStorageKeyValueStorage(accountName, storageKey);
      }

      /// <summary>
      /// Creates an instance of Azure Table Storage using account name and key.
      /// </summary>
      /// <param name="factory">Factory reference</param>
      /// <param name="credential">Credential structure cotnaining account name in username and account key in password.</param>
      /// <returns></returns>
      public static IKeyValueStorage AzureTableStorage(this IKeyValueStorageFactory factory,
         NetworkCredential credential)
      {
         return new AzureTableStorageKeyValueStorage(credential.UserName, credential.Password);
      }

      /// <summary>
      /// Creates an instance of a publisher to Azure Storage Queues
      /// </summary>
      /// <param name="factory">Factory reference</param>
      /// <param name="accountName">Account name</param>
      /// <param name="storageKey">Storage key</param>
      /// <returns>Generic message publisher interface</returns>
      public static IMessenger AzureStorageQueue(this IMessagingFactory factory,
         string accountName,
         string storageKey)
      {
         return new AzureStorageQueueMessenger(accountName, storageKey);
      }

      /// <summary>
      /// Creates an instance of Azure Table Storage using development storage.
      /// </summary>
      /// <param name="factory">Factory reference</param>
      /// <returns></returns>
      public static IKeyValueStorage AzureTableDevelopmentStorage(this IKeyValueStorageFactory factory)
      {
         return new AzureTableStorageKeyValueStorage();
      }

      #region [ Connection Strings ]

      /// <summary>
      /// Create connection string for Azure File storage with account name and key
      /// </summary>
      public static StorageConnectionString ForAzureFileStorage(this IConnectionStringFactory factory,
         string accountName,
         string accountKey)
      {
         var cs = new StorageConnectionString(KnownPrefix.AzureFilesStorage + "://");
         cs.Parameters[KnownParameter.AccountName] = accountName;
         cs.Parameters[KnownParameter.KeyOrPassword] = accountKey;
         return cs;
      }

      #endregion
   }
}

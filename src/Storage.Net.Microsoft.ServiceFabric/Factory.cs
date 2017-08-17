using Microsoft.ServiceFabric.Data;
using Storage.Net.Blob;
using Storage.Net.Messaging;
using Storage.Net.Microsoft.ServiceFabric.Blob;
using Storage.Net.Microsoft.ServiceFabric.Messaging;
using System;

namespace Storage.Net
{
   public static class Factory
   {
      private const string DefaultQueueName = "$default$";

      public static IBlobStorageProvider AzureServiceFabricReliableStorage(this IBlobStorageFactory factory,
         IReliableStateManager stateManager,
         string collectionName)
      {
         return new ServiceFabricReliableDictionaryBlobStorage(stateManager, collectionName);
      }

      public static IMessagePublisher AzureServiceFabricReliableQueuePublisher(
         this IMessagingFactory factory,
         IReliableStateManager stateManager,
         string queueName = null)
      {
         return new ServiceFabricReliableQueuePublisher(stateManager, queueName ?? DefaultQueueName);
      }

      public static IMessageReceiver AzureServiceFabricReliableQueueReceiver(
         this IMessagingFactory factory,
         IReliableStateManager stateManager,
         string queueName = null)
      {
         return new ServiceFabricReliableQueueReceiver(stateManager, queueName ?? DefaultQueueName);
      }

   }
}

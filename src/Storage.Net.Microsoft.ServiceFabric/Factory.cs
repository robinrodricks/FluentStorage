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
         return new ServiceFabricReliableDictionaryBlobStorageProvider(stateManager, collectionName);
      }

      public static IMessagePublisher AzureServiceFabricReliableQueuePublisher(
         this IMessagingFactory factory,
         IReliableStateManager stateManager,
         string queueName = null)
      {
         return new ServiceFabricReliableQueuePublisher(stateManager, queueName ?? DefaultQueueName);
      }

      /// <summary>
      /// Create a receiver on top of Service Fabric Reliable Queue.
      /// </summary>
      /// <param name="factory"></param>
      /// <param name="stateManager"></param>
      /// <param name="scanInterval">Due to the fact that queues are scanned, set this value to a scan interval. 1 second is minimum.</param>
      /// <param name="queueName">Set queue name, otherwise a default queue name is used.</param>
      /// <returns></returns>
      public static IMessageReceiver AzureServiceFabricReliableQueueReceiver(
         this IMessagingFactory factory,
         IReliableStateManager stateManager,
         TimeSpan scanInterval,
         string queueName = null)
      {
         return new ServiceFabricReliableQueueReceiver(stateManager, queueName ?? DefaultQueueName, scanInterval);
      }

   }
}

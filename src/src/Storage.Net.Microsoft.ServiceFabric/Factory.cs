using Microsoft.ServiceFabric.Data;
using Storage.Net.Blob;
using Storage.Net.Microsoft.ServiceFabric.Blob;
using System;

namespace Storage.Net
{
   public static class Factory
   {
      public static IBlobStorage AzureServiceFabricReliableStorage(this IBlobStorageFactory factory,
         IReliableStateManager stateManager,
         string collectionName)
      {
         return new ServiceFabricReliableDictionaryBlobStorage(stateManager, collectionName);
      }
   }
}

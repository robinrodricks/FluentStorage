using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure.EventHubs.Processor;
using Storage.Net.Messaging;

namespace Storage.Net.Microsoft.Azure.EventHub
{
   class EventHubMessageProcessor
   {
      public EventHubMessageProcessor(IMessageProcessor messageProcessor)
      {
         //_host = new EventProcessorHost(hubPath, consumerGroupName, connectionString, storageConnectionString, leaseContainerName);
      }

      public Task StartAsync()
      {
         return Task.CompletedTask;
      }
   }
}

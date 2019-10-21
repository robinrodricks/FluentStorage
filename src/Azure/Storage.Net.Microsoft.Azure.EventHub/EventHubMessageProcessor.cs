using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure.EventHubs;
using Microsoft.Azure.EventHubs.Processor;
using Storage.Net.Messaging;

namespace Storage.Net.Microsoft.Azure.EventHub
{
   class EventHubMessageProcessor
   {
      public EventHubMessageProcessor(string connectionString, IMessageProcessor messageProcessor)
      {
         var csb = new EventHubsConnectionStringBuilder(connectionString);

         string path = csb.EntityPath;
         csb.EntityPath = null;
         string cs = csb.ToString();

         var sa = new StateAdapter();

         var host = new EventProcessorHost(
            Guid.NewGuid().ToString(),
            path,
            null,
            cs,
            sa,
            sa);

         //host.RegisterEventProcessorFactoryAsync
      }

      public Task StartAsync()
      {
         return Task.CompletedTask;
      }
   }
}

using Microsoft.ServiceBus.Messaging;
using System;
using QueueMessage = Storage.Net.Messaging.QueueMessage;

namespace Storage.Net.Azure.Queue.ServiceBus
{
   static class ServiceBusConverter
   {
      public static BrokeredMessage ToBrokeredMessage(QueueMessage message)
      {
         var result = new BrokeredMessage(message.Content.ToMemoryStream());
         if(message.Properties != null && message.Properties.Count > 0)
         {
            foreach(var prop in message.Properties)
            {
               result.Properties.Add(prop.Key, prop.Value);
            }
         }
         return result;
      }

      public static QueueMessage ToQueueMessage(BrokeredMessage message)
      {
         return null;
      }
   }
}

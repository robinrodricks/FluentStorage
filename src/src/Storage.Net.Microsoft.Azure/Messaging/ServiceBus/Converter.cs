using Microsoft.ServiceBus.Messaging;
using System;
using System.Collections.Generic;
using System.IO;
using QueueMessage = Storage.Net.Messaging.QueueMessage;

namespace Storage.Net.Microsoft.Azure.Messaging.ServiceBus
{
   static class Converter
   {
      public static BrokeredMessage ToBrokeredMessage(QueueMessage message)
      {
         var result = new BrokeredMessage(message.Content == null ? null : new MemoryStream(message.Content));
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
         byte[] body;
         using (Stream stream = message.GetBody<Stream>())
         {
            using (var ms = new MemoryStream())
            {
               stream.CopyTo(ms);
               body = ms.ToArray();
            }
         }

         var result = new QueueMessage(message.MessageId, body);
         if(message.Properties != null && message.Properties.Count > 0)
         {
            foreach(KeyValuePair<string, object> pair in message.Properties)
            {
               result.Properties[pair.Key] = pair.Value == null ? null : pair.Value.ToString();
            }
         }

         result.DequeueCount = message.DeliveryCount;
         return result;
      }
   }
}

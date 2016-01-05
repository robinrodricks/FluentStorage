using Microsoft.ServiceBus.Messaging;
using System;
using System.Collections.Generic;
using System.IO;
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
         string body;
         using(var reader = new StreamReader(message.GetBody<Stream>()))
         {
            body = reader.ReadToEnd();
         }

         var result = new QueueMessage(message.MessageId, body);
         if(message.Properties != null && message.Properties.Count > 0)
         {
            result.Properties = new Dictionary<string, string>();
            foreach(KeyValuePair<string, object> pair in message.Properties)
            {
               result.Properties[pair.Key] = pair.Value == null ? pair.Value.ToString() : null;
            }
         }
         return result;
      }
   }
}

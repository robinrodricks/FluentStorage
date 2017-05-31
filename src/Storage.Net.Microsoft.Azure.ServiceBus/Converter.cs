using Microsoft.Azure.ServiceBus;
using System.Collections.Generic;
using QueueMessage = Storage.Net.Messaging.QueueMessage;

namespace Storage.Net.Microsoft.Azure.ServiceBus
{
   static class Converter
   {
      public static Message ToMessage(QueueMessage message)
      {
         var result = new Message(message.Content);
         if(message.Properties != null && message.Properties.Count > 0)
         {
            foreach(var prop in message.Properties)
            {
               result.UserProperties.Add(prop.Key, prop.Value);
            }
         }
         return result;
      }

      public static QueueMessage ToQueueMessage(Message message)
      {
         var result = new QueueMessage(message.MessageId, message.Body);
         if(message.UserProperties != null && message.UserProperties.Count > 0)
         {
            foreach(KeyValuePair<string, object> pair in message.UserProperties)
            {
               result.Properties[pair.Key] = pair.Value == null ? null : pair.Value.ToString();
            }
         }

         return result;
      }
   }
}

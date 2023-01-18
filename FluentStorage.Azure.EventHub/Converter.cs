using Microsoft.Azure.EventHubs;
using NetBox.Extensions;
using FluentStorage.Messaging;
using System;
using System.Collections.Generic;
using System.Linq;

namespace FluentStorage.Azure.EventHub
{
   static class Converter
   {
      public static EventData ToEventData(QueueMessage message)
      {
         if(message == null)
            throw new ArgumentNullException(nameof(message));

         var ev = new EventData(message.Content);
         if(message.Properties.Count > 0)
         {
            ev.Properties.AddRange(message.Properties.ToDictionary(kv => kv.Key, kv => (object)kv.Value));
         }
         return ev;
      }

      public static QueueMessage ToQueueMessage(EventData ed, string partitionId)
      {
         if(ed == null)
            return null;

         var r = new QueueMessage(ed.Body.Array);
         r.Properties.AddRange(ed.Properties.ToDictionary(kv => kv.Key, kv => kv.Value?.ToString()));
         if(partitionId != null)
         {
            r.Properties.Add("x-eventhub-partitionid", partitionId);
         }
         return r;
      }
   }
}

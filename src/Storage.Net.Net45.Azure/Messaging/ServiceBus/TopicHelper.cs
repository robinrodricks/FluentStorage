using Microsoft.ServiceBus;
using Microsoft.ServiceBus.Messaging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Storage.Net.Azure.Messaging.ServiceBus
{
   static class TopicHelper
   {
      public static void PrepareTopic(NamespaceManager nsMgr, string topicName)
      {
         if(!nsMgr.TopicExists(topicName))
         {
            var td = new TopicDescription(topicName);
            //todo: more options on TD

            nsMgr.CreateTopic(td);
         }
      }
   }
}

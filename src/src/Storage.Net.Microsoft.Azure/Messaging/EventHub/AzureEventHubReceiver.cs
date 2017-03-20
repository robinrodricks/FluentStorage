using Storage.Net.Messaging;
using System;

namespace Storage.Net.Microsoft.Azure.Messaging.EventHub
{
   class AzureEventHubReceiver : AsyncMessageReceiver
   {
      public override void StartMessagePump(Action<QueueMessage> onMessage)
      {
         throw new NotImplementedException();
      }
   }
}

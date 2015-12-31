using Storage.Net.Messaging;
using System;

namespace Storage.Net.Azure.Queue
{
   class AzureServiceBusTopicQueue : IMessageQueue
   {
      public void Clear()
      {
         throw new NotImplementedException();
      }

      public void DeleteMessage(string id)
      {
         throw new NotImplementedException();
      }

      public QueueMessage GetMessage(TimeSpan? visibilityTimeout = default(TimeSpan?))
      {
         throw new NotImplementedException();
      }

      public QueueMessage PeekMesssage()
      {
         throw new NotImplementedException();
      }

      public void PutMessage(string content)
      {
         throw new NotImplementedException();
      }
   }
}

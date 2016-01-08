using Storage.Net.Messaging;
using System;

namespace Storage.Net.Azure.Queue.ServiceBus
{
   /// <summary>
   /// Subscribes to messages in a Service Bus Topic
   /// </summary>
   class AzureServiceBusTopicReceiver : IMessageReceiver
   {
      public AzureServiceBusTopicReceiver(string connectionString, string topicName)
      {

      }

      public void ConfirmMessage(QueueMessage message)
      {
         throw new NotImplementedException();
      }

      public void Dispose()
      {
         throw new NotImplementedException();
      }

      public QueueMessage ReceiveMessage()
      {
         throw new NotImplementedException();
      }
   }
}

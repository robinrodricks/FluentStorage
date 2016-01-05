using Storage.Net.Messaging;
using System;

namespace Storage.Net.Azure.Queue.ServiceBus
{
   /// <summary>
   /// Implements message receiver on Azure Service Bus Queues
   /// </summary>
   public class AzureServiceBusQueueReceiver : IMessageReceiver
   {


      /// <summary>
      /// Fired when a new message appears in the queue
      /// </summary>
      public event EventHandler<QueueMessage> OnNewMessage;

      /// <summary>
      /// Call at the end when done with the message.
      /// </summary>
      /// <param name="message"></param>
      public void ConfirmMessage(QueueMessage message)
      {
         throw new NotImplementedException();
      }
   }
}

using Microsoft.ServiceBus;
using Microsoft.ServiceBus.Messaging;
using Storage.Net.Messaging;
using System;

namespace Storage.Net.Azure.Queue.ServiceBus
{
   /// <summary>
   /// Implements message receiver on Azure Service Bus Queues
   /// </summary>
   public class AzureServiceBusQueueReceiver : IMessageReceiver
   {
      private static readonly TimeSpan AutoRenewTimeout = TimeSpan.FromMinutes(1);

      private readonly QueueClient _client;
      private readonly bool _peekLock;

      /// <summary>
      /// Creates 
      /// </summary>
      /// <param name="connectionString"></param>
      /// <param name="queueName"></param>
      /// <param name="peekLock"></param>
      public AzureServiceBusQueueReceiver(string connectionString, string queueName, bool peekLock = true)
      {
         _client = QueueClient.CreateFromConnectionString(connectionString, queueName,
            peekLock ? ReceiveMode.PeekLock : ReceiveMode.ReceiveAndDelete);

         _peekLock = peekLock;

         SubscribeQueue();
      }

      private void SubscribeQueue()
      {
         var options = new OnMessageOptions();
         options.AutoComplete = false;
         options.AutoRenewTimeout = AutoRenewTimeout;

         _client.OnMessage(OnBrokeredMessage, options);
      }

      private void OnBrokeredMessage(BrokeredMessage message)
      {
         QueueMessage result = ServiceBusConverter.ToQueueMessage(message);

         message.Complete();

         if(OnNewMessage != null) OnNewMessage(this, result);
      }

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

      public void Dispose()
      {
         //_client.OnMessage -= OnBrokeredMessage;
      }
   }
}

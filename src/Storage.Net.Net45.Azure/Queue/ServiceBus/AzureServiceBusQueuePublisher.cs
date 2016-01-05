using System;
using Microsoft.ServiceBus;
using Microsoft.ServiceBus.Messaging;
using Storage.Net.Messaging;

namespace Storage.Net.Azure.Queue.ServiceBus
{
   /// <summary>
   /// Implements Azure Service Bus Queue
   /// </summary>
   public class AzureServiceBusQueuePublisher : IMessagePublisher
   {
      private readonly NamespaceManager _ns;
      private readonly string _queueName;
      private readonly QueueClient _client;

      /// <summary>
      /// Creates a new instance of Azure Service Bus Queue by connection string and queue name
      /// </summary>
      /// <param name="connectionString">Service Bus connection string</param>
      /// <param name="queueName">Queue name in Service Bus. If queue doesn't exist it will be created for you.</param>
      public AzureServiceBusQueuePublisher(string connectionString, string queueName)
      {
         _ns = NamespaceManager.CreateFromConnectionString(connectionString);
         _queueName = queueName;

         PrepareQueue();

         _client = QueueClient.CreateFromConnectionString(connectionString, _queueName);
      }

      private void PrepareQueue()
      {
         if(!_ns.QueueExists(_queueName))
         {
            var qd = new QueueDescription(_queueName);
            //todo: set extra parameters in qd here

            _ns.CreateQueue(qd);
         }
      }

      /// <summary>
      /// Puts message to the queue with default options
      /// </summary>
      /// <param name="message"></param>
      public void PutMessage(QueueMessage message)
      {
         BrokeredMessage bm = Converter.ToBrokeredMessage(message);
         _client.Send(bm);
      }

      public void Dispose()
      {
      }
   }
}

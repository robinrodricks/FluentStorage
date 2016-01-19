using System.Collections.Generic;
using System.Linq;
using Microsoft.ServiceBus;
using Microsoft.ServiceBus.Messaging;
using Storage.Net.Messaging;

namespace Storage.Net.Azure.Messaging.ServiceBus
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
      public void PutMessage(QueueMessage message)
      {
         if(message == null) return;
         BrokeredMessage bm = Converter.ToBrokeredMessage(message);
         _client.Send(bm);
      }

      /// <summary>
      /// Puts message to the queue with default options
      /// </summary>
      public void PutMessages(IEnumerable<QueueMessage> messages)
      {
         if(messages == null) return;
         IEnumerable<BrokeredMessage> bms = messages.Select(Converter.ToBrokeredMessage);
         _client.SendBatch(bms);
      }

      /// <summary>
      /// Nothing to dispose
      /// </summary>
      public void Dispose()
      {
      }
   }
}

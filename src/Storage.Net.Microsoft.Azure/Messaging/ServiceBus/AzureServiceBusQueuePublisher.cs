#if NETFULL
using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.ServiceBus;
using Microsoft.ServiceBus.Messaging;
using Storage.Net.Messaging;
using System.Threading.Tasks;

namespace Storage.Net.Microsoft.Azure.Messaging.ServiceBus
{
   /// <summary>
   /// Implements Azure Service Bus Queue
   /// </summary>
   public class AzureServiceBusQueuePublisher : AsyncMessagePublisher
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
         try
         {
            _ns = NamespaceManager.CreateFromConnectionString(connectionString);
         }
         catch(Exception ex)
         {
            throw new ArgumentException("cannot use connection string: " + connectionString, nameof(connectionString), ex);
         }
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
      public override async Task PutMessagesAsync(IEnumerable<QueueMessage> messages)
      {
         if (messages == null) return;
         IEnumerable<BrokeredMessage> bms = messages.Select(Converter.ToBrokeredMessage);
         await _client.SendBatchAsync(bms);
      }
   }
}
#endif
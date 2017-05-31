using Storage.Net.Messaging;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Azure.ServiceBus;

namespace Storage.Net.Microsoft.Azure.ServiceBus
{
   /// <summary>
   /// Represents queues as Azure Service Bus Topics. Note that you must have at least one subscription
   /// for messages not to be lost. Subscriptions represent <see cref="AzureServiceBusTopicReceiver"/>
   /// in this library
   /// </summary>
   public class AzureServiceBusTopicPublisher : AsyncMessagePublisher
   {
      private TopicClient _client;

      /// <summary>
      /// Creates an instance of Azure Service Bus Topic publisher
      /// </summary>
      /// <param name="connectionString">Service Bus connection string</param>
      /// <param name="topicName">Name of the Service Bus topic</param>
      public AzureServiceBusTopicPublisher(string connectionString, string topicName)
      {
         _client = new TopicClient(connectionString, topicName);
      }

      /// <summary>
      /// Sends messages out
      /// </summary>
      public override async Task PutMessagesAsync(IEnumerable<QueueMessage> messages)
      {
         if (messages == null) return;
         IList<Message> sbmsg = messages.Select(Converter.ToMessage).ToArray();
         await _client.SendAsync(sbmsg);
      }
   }
}
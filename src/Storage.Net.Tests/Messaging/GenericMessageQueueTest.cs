using Xunit;
using Storage.Net.Messaging;
using System;
using LogMagic;
using System.Collections.Generic;
using System.Threading.Tasks;
using NetBox;
using System.Linq;

namespace Storage.Net.Tests.Integration.Messaging
{
   #region [ Test Variations ]

   public class AzureStorageQueueMessageQueueTest : GenericMessageQueueTest
   {
      public AzureStorageQueueMessageQueueTest() : base("azure-storage-queue") { }
   }

   public class AzureServiceBusQueueMessageQeueueTest : GenericMessageQueueTest
   {
      public AzureServiceBusQueueMessageQeueueTest() : base("azure-servicebus-queue") { }
   }

   public class AzureServiceBusTopicMessageQeueueTest : GenericMessageQueueTest
   {
      public AzureServiceBusTopicMessageQeueueTest() : base("azure-servicebus-topic") { }
   }

   public class AzureEventHubMessageQeueueTest : GenericMessageQueueTest
   {
      public AzureEventHubMessageQeueueTest() : base("azure-eventhub") { }
   }

   #endregion

   public abstract class GenericMessageQueueTest : AbstractTestFixture
   {
      private readonly ILog _log = L.G();
      private readonly string _name;
      private IMessagePublisher _publisher;
      private IMessageReceiver _receiver;
      private readonly List<QueueMessage> _receivedMessages = new List<QueueMessage>();

      protected GenericMessageQueueTest(string name)
      {
         _name = name;

         switch(_name)
         {
            case "azure-storage-queue":
               _publisher = StorageFactory.Messages.AzureStorageQueuePublisher(
                  TestSettings.Instance.AzureStorageName,
                  TestSettings.Instance.AzureStorageKey,
                  TestSettings.Instance.ServiceBusQueueName);
               _receiver = StorageFactory.Messages.AzureStorageQueueReceiver(
                  TestSettings.Instance.AzureStorageName,
                  TestSettings.Instance.AzureStorageKey,
                  TestSettings.Instance.ServiceBusQueueName,
                  TimeSpan.FromMinutes(1),
                  TimeSpan.FromMilliseconds(500));
               break;
            case "azure-servicebus-queue":
               _receiver = StorageFactory.Messages.AzureServiceBusQueueReceiver(
                  TestSettings.Instance.ServiceBusConnectionString,
                  "testqueue",
                  true);
               _publisher = StorageFactory.Messages.AzureServiceBusQueuePublisher(
                  TestSettings.Instance.ServiceBusConnectionString,
                  "testqueue");
               break;
            case "azure-servicebus-topic":
               _receiver = StorageFactory.Messages.AzureServiceBusTopicReceiver(
                  TestSettings.Instance.ServiceBusConnectionString,
                  "testtopic",
                  "testsub",
                  true);
               _publisher = StorageFactory.Messages.AzureServiceBusTopicPublisher(
                  TestSettings.Instance.ServiceBusConnectionString,
                  "testtopic");
               break;
            case "azure-eventhub":
               _receiver = StorageFactory.Messages.AzureEventHubReceiver(
                  TestSettings.Instance.EventHubConnectionString,
                  TestSettings.Instance.EventHubPath,
                  null,
                  null,
                  StorageFactory.Blobs.AzureBlobStorage(
                     TestSettings.Instance.AzureStorageName,
                     TestSettings.Instance.AzureStorageKey,
                     "integration-hub"));
               _publisher = StorageFactory.Messages.AzureEventHubPublisher(
                  TestSettings.Instance.EventHubConnectionString,
                  TestSettings.Instance.EventHubPath);
               break;
         }

         //start the pump
         _receiver.StartMessagePumpAsync(ReceiverPump);
      }

      private async Task ReceiverPump(QueueMessage message)
      {
         _receivedMessages.Add(message);
         await _receiver.ConfirmMessageAsync(message);
      }

      private async Task<QueueMessage> WaitMessage()
      {
         //wait for receiver to stabilize

         int prevCount;

         do
         {
            prevCount = _receivedMessages.Count;
            await Task.Delay(TimeSpan.FromSeconds(1));
         }
         while (prevCount != _receivedMessages.Count);

         return _receivedMessages.LastOrDefault();
      }

      public override void Dispose()
      {
         if(_publisher != null) _publisher.Dispose();
         if(_receiver != null) _receiver.Dispose();

         base.Dispose();
      }

      [Fact]
      public async Task SendMessage_OneMessage_DoesntCrash()
      {
         var qm = QueueMessage.FromText("test");
         await _publisher.PutMessagesAsync(new[] { qm });
      }

      [Fact]
      public async Task SendMessage_SendAFew_Receives()
      {
         for(int i = 0; i < 2; i++)
         {
            await _publisher.PutMessagesAsync(new[] { new QueueMessage("test content at " + DateTime.UtcNow) });
         }

         //there is a delay between messages sent and received on subscription, so sleep for a bit
         await WaitMessage();
         Assert.True(_receivedMessages.Count > 0);
      }

      [Fact]
      public async Task SendMessage_ExtraProperties_DoesntCrash()
      {
         var msg = new QueueMessage("prop content at " + DateTime.UtcNow);
         msg.Properties["one"] = "one value";
         msg.Properties["two"] = "two value";
         await _publisher.PutMessagesAsync(new[] { msg });
      }

      [Fact]
      public async Task SendMessage_SimpleOne_Received()
      {
         string content = Generator.RandomString;

         await _publisher.PutMessagesAsync(new[] { new QueueMessage(content) });

         QueueMessage received = await WaitMessage();

         Assert.NotNull(received);
         Assert.Equal(content, received.StringContent);
      }

      [Fact]
      public async Task SendMessage_WithProperties_Received()
      {
         string content = Generator.RandomString;

         var msg = new QueueMessage(content);
         msg.Properties["one"] = "v1";

         await _publisher.PutMessagesAsync(new[] { msg });

         QueueMessage received = await WaitMessage();

         Assert.NotNull(received);
         Assert.Equal(content, received.StringContent);
         Assert.Equal("v1", received.Properties["one"]);
      }

      [Fact]
      public async Task CleanQueue_SendMessage_ReceiveAndConfirm()
      {
         string content = Generator.RandomString;
         var msg = new QueueMessage(content);
         await _publisher.PutMessagesAsync(new[] { msg });

         QueueMessage rmsg = await WaitMessage();
         Assert.NotNull(rmsg);
      }

      [Fact]
      public async Task MessagePump_AddFewMessages_CanReceiveOneAndPumpClearsThemAll()
      {
         for (int i = 0; i < 10; i++)
         {
            var qm = new QueueMessage(nameof(MessagePump_AddFewMessages_CanReceiveOneAndPumpClearsThemAll) + "#" + i);
            await _publisher.PutMessagesAsync(new[] { qm });
         }

         await WaitMessage();

         Assert.True(_receivedMessages.Count >= 9, _receivedMessages.Count.ToString());
      }
   }
}

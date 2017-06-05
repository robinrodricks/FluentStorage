using Xunit;
using Storage.Net.Messaging;
using System;
using LogMagic;
using System.Collections.Generic;
using System.Threading.Tasks;

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
      private int _messagesPumped;
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
                  TimeSpan.FromMinutes(1));
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
      }

      public override void Dispose()
      {
         if(_publisher != null) _publisher.Dispose();
         if(_receiver != null) _receiver.Dispose();

         base.Dispose();
      }

      [Fact]
      public void SendMessage_OneMessage_DoesntCrash()
      {
         //var messages = Enumerable.Range(0, 1000).Select(i => new QueueMessage("content " + i));

         //_publisher.PutMessages(messages);

         var qm = QueueMessage.FromText("test");
         _publisher.PutMessage(qm);
      }

      /*[Fact]
      public void SendMessage_SendAFew_ReceivesAsBatch()
      {
         for(int i = 0; i < 2; i++)
         {
            _publisher.PutMessage(new QueueMessage("test content at " + DateTime.UtcNow));
         }

         //there is a delay between messages sent and received on subscription, so sleep for a bit

         List<QueueMessage> batch = null;

         for (int i = 0; i < 50; i++)
         {
            batch = _receiver.ReceiveMessages(10).ToList();
            if (batch != null && batch.Count > 0) break;
            Thread.Sleep(TimeSpan.FromSeconds(1));
         }

         Assert.NotNull(batch);
         Assert.True(batch.Count > 0);
      }*/

      [Fact]
      public void SendMessage_ExtraProperties_DoesntCrash()
      {
         var msg = new QueueMessage("prop content at " + DateTime.UtcNow);
         msg.Properties["one"] = "one value";
         msg.Properties["two"] = "two value";
         _publisher.PutMessage(msg);
      }

      /*[Fact]
      public void SendMessage_SimpleOne_Received()
      {
         string content = Generator.RandomString;

         _publisher.PutMessage(new QueueMessage(content));

         QueueMessage received = null;

         for (int i = 0; i < 5; i++)
         {
            received = _receiver.ReceiveMessage();
            if (received != null) break;
            Thread.Sleep(TimeSpan.FromSeconds(2));
         }

         Assert.NotNull(received);
         Assert.Equal(content, received.StringContent);
      }*/

      /*[Fact]
      public void SendMessage_WithProperties_Received()
      {
         string content = Generator.RandomString;

         var msg = new QueueMessage(content);
         msg.Properties["one"] = "v1";

         _publisher.PutMessage(msg);

         QueueMessage received = null;

         for (int i = 0; i < 10; i++)
         {
            received = _receiver.ReceiveMessage();
            if (received != null) break;
            Thread.Sleep(TimeSpan.FromSeconds(2));
         }

         Assert.NotNull(received);
         Assert.Equal(content, received.StringContent);
         Assert.Equal("v1", received.Properties["one"]);
      }*/

      /*[Fact]
      public void CleanQueue_SendMessage_ReceiveAndConfirm()
      {
         string content = Generator.RandomString;
         var msg = new QueueMessage(content);
         _publisher.PutMessage(msg);

         QueueMessage rmsg = _receiver.ReceiveMessage();
         Assert.NotNull(rmsg);

         _receiver.ConfirmMessage(rmsg);
         _receiver.ConfirmMessage(new QueueMessage(rmsg.Id, string.Empty));
      }*/

      /*[Fact]
      public void MessagePump_AddFewMessages_CanReceiveOneAndPumpClearsThemAll()
      {
         for (int i = 0; i < 10; i++)
         {
            var qm = new QueueMessage(nameof(MessagePump_AddFewMessages_CanReceiveOneAndPumpClearsThemAll) + "#" + i);
            _publisher.PutMessage(qm);
         }

         _receiver.StartMessagePump(OnMessage);

         Thread.Sleep(TimeSpan.FromHours(11));

         Assert.True(_messagesPumped >= 9);
      }*/

      private void OnMessage(QueueMessage qm)
      {
         _messagesPumped++;
      }
   }
}

using Aloneguid.Support;
using Config.Net;
using Xunit;
using Storage.Net.Azure.Messaging.ServiceBus;
using Storage.Net.Azure.Messaging.Storage;
using Storage.Net.Messaging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using LogMagic;

namespace Storage.Net.Tests.Integration.Messaging
{
   #region [ Test Variations ]

   public class AzureStorageQueueMessageQueueTest : GenericMessageQueueTest
   {
      public AzureStorageQueueMessageQueueTest() : base("azure-storage-queue") { }
   }

   public class AzureServiceBusTopicMessageQeueueTest : GenericMessageQueueTest
   {
      public AzureServiceBusTopicMessageQeueueTest() : base("azure-servicebus-topic") { }
   }

   public class AzureServiceBusQueueMessageQeueueTest : GenericMessageQueueTest
   {
      public AzureServiceBusQueueMessageQeueueTest() : base("azure-servicebus-queue") { }
   }

   public class InMemoryMessageQueueTest : GenericMessageQueueTest
   {
      public InMemoryMessageQueueTest() : base("inmemory") { }
   }

   /*public class DiskMessageQeueuTest : GenericMessageQueueTest
   {
      public DiskMessageQeueuTest() : base("disk") { }
   }*/

   #endregion

   public abstract class GenericMessageQueueTest : AbstractTestFixture
   {
      private readonly ILog _log = L.G();
      private readonly string _name;
      private IMessagePublisher _publisher;
      private IMessageReceiver _receiver;
      private int _messagesPumped;

      protected GenericMessageQueueTest(string name)
      {
         _name = name;

         switch(_name)
         {
            case "azure-storage-queue":
               _publisher = new AzureStorageQueuePublisher(
                  TestSettings.AzureStorageName,
                  TestSettings.AzureStorageKey,
                  TestSettings.ServiceBusQueueName);
               _receiver = new AzureStorageQueueReceiver(
                  TestSettings.AzureStorageName,
                  TestSettings.AzureStorageKey,
                  TestSettings.ServiceBusQueueName,
                  TimeSpan.FromMinutes(1));
               break;
            case "azure-servicebus-topic":
               _receiver = new AzureServiceBusTopicReceiver(
                  TestSettings.ServiceBusConnectionString,
                  TestSettings.ServiceBusTopicName,
                  "AllMessages",
                  null,
                  true);
               _publisher = new AzureServiceBusTopicPublisher(
                  TestSettings.ServiceBusConnectionString,
                  TestSettings.ServiceBusTopicName);
               break;
            case "azure-servicebus-queue":
               _receiver = new AzureServiceBusQueueReceiver(
                  Cfg.Read(TestSettings.ServiceBusConnectionString),
                  "testqueue",
                  true);
               _publisher = new AzureServiceBusQueuePublisher(
                  Cfg.Read(TestSettings.ServiceBusConnectionString),
                  "testqueue");
               break;
            case "inmemory":
               var inmem = new InMemoryMessagePublisherReceiver();
               _publisher = inmem;
               _receiver = inmem;
               break;
            /*case "disk":
               var disk = new DiskMessagePublisherReceiver(TestDir);
               _publisher = disk;
               _receiver = disk;
               break;*/
         }

         //delete any messages already in queue
         QueueMessage qm;
         while((qm = _receiver.ReceiveMessage()) != null)
         {
            _receiver.ConfirmMessage(qm);
         }

         _messagesPumped = 0;
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
         _publisher.PutMessage(new QueueMessage("test content at " + DateTime.UtcNow));
      }

      [Fact]
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
      }

      [Fact]
      public void SendMessage_ExtraProperties_DoesntCrash()
      {
         var msg = new QueueMessage("prop content at " + DateTime.UtcNow);
         msg.Properties["one"] = "one value";
         msg.Properties["two"] = "two value";
         _publisher.PutMessage(msg);
      }

      [Fact]
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
      }

      [Fact]
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
      }

      [Fact]
      public void MessagePump_AddFewMessages_CanReceiveOneAndPumpClearsThemAll()
      {
         for (int i = 0; i < 10; i++)
         {
            var qm = new QueueMessage(nameof(MessagePump_AddFewMessages_CanReceiveOneAndPumpClearsThemAll) + "#" + i);
            _publisher.PutMessage(qm);
         }

         Assert.NotNull(_receiver.ReceiveMessage());

         _receiver.StartMessagePump(OnMessage);

         Thread.Sleep(TimeSpan.FromSeconds(10));

         Assert.True(_messagesPumped >= 9);
      }

      private void OnMessage(QueueMessage qm)
      {
         _messagesPumped++;
      }
   }
}

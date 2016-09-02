using Aloneguid.Support;
using Config.Net;
using NUnit.Framework;
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
   [TestFixture("azure-storage-queue")]
   [TestFixture("azure-servicebus-topic")]
   [TestFixture("azure-servicebus-queue")]
   [TestFixture("inmemory")]
   //[TestFixture("disk")]
   public class GenericMessageQueueTest : AbstractTestFixture
   {
      private readonly ILog _log = L.G();
      private readonly string _name;
      private IMessagePublisher _publisher;
      private IMessageReceiver _receiver;
      private int _messagesPumped;

      public GenericMessageQueueTest(string name)
      {
         _name = name;
      }

      [SetUp]
      public void SetUp()
      {
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
               _publisher = new AzureServiceBusTopicPublisher(
                  TestSettings.ServiceBusConnectionString,
                  TestSettings.ServiceBusTopicName);
               _receiver = new AzureServiceBusTopicReceiver(
                  TestSettings.ServiceBusConnectionString,
                  TestSettings.ServiceBusTopicName,
                  "AllMessages",
                  null,
                  true);
               break;
            case "azure-servicebus-queue":
               _publisher = new AzureServiceBusQueuePublisher(
                  Cfg.Read(TestSettings.ServiceBusConnectionString),
                  "testqueue");
               _receiver = new AzureServiceBusQueueReceiver(
                  Cfg.Read(TestSettings.ServiceBusConnectionString),
                  "testqueue",
                  true);
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

      [TearDown]
      public void TearDown()
      {
         if(_publisher != null) _publisher.Dispose();
         if(_receiver != null) _receiver.Dispose();
      }

      [Test]
      public void SendMessage_OneMessage_DoesntCrash()
      {
         _publisher.PutMessage(new QueueMessage("test content at " + DateTime.UtcNow));
      }

      [Test]
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

         Assert.IsNotNull(batch);
         Assert.Greater(batch.Count, 0);
      }

      [Test]
      public void SendMessage_ExtraProperties_DoesntCrash()
      {
         var msg = new QueueMessage("prop content at " + DateTime.UtcNow);
         msg.Properties["one"] = "one value";
         msg.Properties["two"] = "two value";
         _publisher.PutMessage(msg);
      }

      [Test]
      public void SendMessage_SimpleOne_Received()
      {
         string content = Generator.RandomString;

         _publisher.PutMessage(new QueueMessage(content));

         QueueMessage received = null;

         for (int i = 0; i < 10; i++)
         {
            received = _receiver.ReceiveMessage();
            if (received != null) break;
            Thread.Sleep(TimeSpan.FromSeconds(2));
         }

         Assert.IsNotNull(received);
         Assert.AreEqual(content, received.StringContent);
      }

      [Test]
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

         Assert.IsNotNull(received);
         Assert.AreEqual(content, received.Content);
         Assert.AreEqual("v1", received.Properties["one"]);
      }

      [Test]
      public void MessagePump_AddFewMessages_CanReceiveOneAndPumpClearsThemAll()
      {
         for (int i = 0; i < 10; i++)
         {
            var qm = new QueueMessage(nameof(MessagePump_AddFewMessages_CanReceiveOneAndPumpClearsThemAll) + "#" + i);
            _publisher.PutMessage(qm);
         }

         Assert.IsNotNull(_receiver.ReceiveMessage());

         _receiver.StartMessagePump(OnMessage);

         Thread.Sleep(TimeSpan.FromSeconds(10));

         Assert.GreaterOrEqual(_messagesPumped, 9);
      }

      private void OnMessage(QueueMessage qm)
      {
         _messagesPumped++;
      }
   }
}

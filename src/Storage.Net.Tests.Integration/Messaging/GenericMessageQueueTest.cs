using Aloneguid.Support;
using Config.Net;
using NUnit.Framework;
using Storage.Net.Azure.Queue.ServiceBus;
using Storage.Net.Azure.Queue.Storage;
using Storage.Net.Messaging;
using System;
using System.Threading;

namespace Storage.Net.Tests.Integration.Messaging
{
   //[TestFixture("azure-storage-queue")]
   //[TestFixture("azure-servicebus-topic")]
   [TestFixture("azure-servicebus-queue")]
   public class GenericMessageQueueTest : AbstractTestFixture
   {
      private string _name;
      private IMessagePublisher _publisher;
      private IMessageReceiver _receiver;
      private QueueMessage _lastMessage;

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
                  Cfg.Read(TestSettings.AzureStorageName),
                  Cfg.Read(TestSettings.AzureStorageKey),
                  "testqueue");
               _receiver = new AzureStorageQueueReceiver(
                  Cfg.Read(TestSettings.AzureStorageName),
                  Cfg.Read(TestSettings.AzureStorageKey),
                  "testqueue", TimeSpan.FromSeconds(1), TimeSpan.FromMinutes(1));
               break;
            /*case "azure-servicebus-topic":
               _publisher = new AzureServiceBusTopicPublisher(Cfg.Read(TestSettings.ServiceBusConnectionString), "testtopic");
               _receiver = null;
               break;*/
            case "azure-servicebus-queue":
               _publisher = new AzureServiceBusQueuePublisher(Cfg.Read(TestSettings.ServiceBusConnectionString), "testqueue");
               _receiver = new AzureServiceBusQueueReceiver(Cfg.Read(TestSettings.ServiceBusConnectionString), "testqueue", true);
               break;
         }

         _receiver.OnNewMessage += OnMessage;
      }

      [TearDown]
      public void TearDown()
      {
         _receiver.OnNewMessage -= OnMessage;

         _publisher.Dispose();
         _receiver.Dispose();
      }

      private void OnMessage(object sender, QueueMessage message)
      {
         _lastMessage = message;

         _receiver.ConfirmMessage(message);
      }

      [Test]
      public void SendMessage_OneMessage_DoesntCrash()
      {
         _publisher.PutMessage(new QueueMessage("test content at " + DateTime.UtcNow));

         if(_receiver != null) Thread.Sleep(TimeSpan.FromMinutes(1));
         //_queue.PeekMesssage
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

         Thread.Sleep(TimeSpan.FromSeconds(10));

         Assert.AreEqual(content, _lastMessage.Content);
      }

      [Test]
      public void SendMessage_WithProperties_Received()
      {
         string content = Generator.RandomString;

         var msg = new QueueMessage(content);
         msg.Properties["one"] = "v1";

         _publisher.PutMessage(msg);

         Thread.Sleep(TimeSpan.FromSeconds(30));

         Assert.AreEqual(content, _lastMessage.Content);
         Assert.AreEqual("v1", _lastMessage.Properties["one"]);
      }

   }
}

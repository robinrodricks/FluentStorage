using Config.Net;
using NUnit.Framework;
using Storage.Net.Azure.Queue.ServiceBus;
using Storage.Net.Azure.Queue.Storage;
using Storage.Net.Messaging;
using System;
using System.Threading;

namespace Storage.Net.Tests.Integration.Messaging
{
   [TestFixture("azure-storage-queue")]
   [TestFixture("azure-servicebus-topic")]
   [TestFixture("azure-servicebus-queue")]
   public class GenericMessageQueueTest : AbstractTestFixture
   {
      private string _name;
      private IMessagePublisher _publisher;
      private IMessageReceiver _receiver;

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
               _publisher = new AzureStorageMessageQueue(
                  Cfg.Read(TestSettings.AzureStorageName),
                  Cfg.Read(TestSettings.AzureStorageKey),
                  "testqueue");
               _receiver = null;
               break;
            case "azure-servicebus-topic":
               _publisher = new AzureServiceBusTopicPublisher(Cfg.Read(TestSettings.ServiceBusConnectionString), "testtopic");
               _receiver = null;
               break;
            case "azure-servicebus-queue":
               _publisher = new AzureServiceBusQueuePublisher(Cfg.Read(TestSettings.ServiceBusConnectionString), "testqueue");
               _receiver = new AzureServiceBusQueueReceiver(Cfg.Read(TestSettings.ServiceBusConnectionString), "testqueue", true);
               break;
         }
      }

      [Test]
      public void SendMessage_OneMessage_DoesntCrash()
      {
         _publisher.PutMessage(new QueueMessage("test content at " + DateTime.UtcNow));

         if(_receiver != null) Thread.Sleep(TimeSpan.FromMinutes(1));
         //_queue.PeekMesssage
      }

   }
}

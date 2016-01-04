using Config.Net;
using NUnit.Framework;
using Storage.Net.Azure.Queue;
using Storage.Net.Messaging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Storage.Net.Tests.Integration.Messaging
{
   [TestFixture("azure-storage-queue")]
   [TestFixture("azure-servicebus-topics")]
   public class GenericMessageQueueTest : AbstractTestFixture
   {
      private string _name;
      private IMessageQueue _queue;

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
               _queue = new AzureMessageQueue(
                  Cfg.Read(TestSettings.AzureStorageName),
                  Cfg.Read(TestSettings.AzureStorageKey),
                  "testqueue");
               break;
            case "azure-servicebus-topics":
               _queue = new AzureServiceBusTopicQueue(Cfg.Read(TestSettings.ServiceBusConnectionString), "testtopic");
               break;
         }
      }

      [Test]
      public void SendMessage_OneMessage_DoesntCrash()
      {
         _queue.PutMessage(new QueueMessage("test content at " + DateTime.UtcNow));

         //_queue.PeekMesssage
      }

   }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.Storage.Queue;
using NUnit.Framework;
using Storage.Net.Azure.Messaging.Storage;
using Storage.Net.Messaging;

namespace Storage.Net.Tests.Azure.Messaging.Storage
{
   [TestFixture]
   public class ConverterTest
   {
      [Test]
      public void ToCloudQueueMessage_WithProps_ConvertsTwoWay()
      {
         var qm = new QueueMessage("a string");
         qm.Properties["one"] = "two";

         CloudQueueMessage cm = Converter.ToCloudQueueMessage(qm);

         QueueMessage qm2 = Converter.ToQueueMessage(cm);

         Assert.AreEqual(qm.Properties.Count, qm2.Properties.Count);
         Assert.AreEqual(qm.Properties["one"], qm2.Properties["one"]);
         Assert.AreEqual(qm.StringContent, qm2.StringContent);
      }
   }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Storage.Net.Microsoft.Azure.Storage.Blob;
using Xunit;

namespace Storage.Net.Tests.Blobs
{
   public class AzureBlobMonitorTest
   {
      [Fact]
      public async Task Smoke()
      {
         var provider = new AzureBlobStorageProvider(
            "asosecstorage",
            "xIKbkHN2cruLDDatCHmJF7+aGEktDwdODoqBNt2AQHDicAJlnOuVJnDQTYIzfAxwLFMBp9W6AkYwNYBIgvB+5Q==",
            "$logs");

         var monitor = new AzureBlobMonitor(provider);

         await monitor.DoWork();
      }
   }
}

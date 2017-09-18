using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Storage.Net.Microsoft.Azure.Storage.Blob;
using Storage.Net.Tests;

namespace Storage.Net.MonitorConsole
{
   class Program
   {
      static async Task Main(string[] args)
      {
         try
         {
            var provider = new AzureBlobStorageProvider(
               TestSettings.Instance.AzureStorageName,
               TestSettings.Instance.AzureStorageKey,
               "$logs",
               false);

            var monitor = new AzureBlobMonitor(provider);

            await monitor.DoWork();
         }
         catch(Exception ex)
         {
            Console.WriteLine(ex);
         }

         Console.ReadLine();
      }
   }
}

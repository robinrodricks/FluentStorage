using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.Storage.Blob;
using Storage.Net.Blob;

namespace Storage.Net.Microsoft.Azure.Storage.Blob
{
   /// <summary>
   /// Monitors for changes in Azure Blob Storage
   /// </summary>
   public class AzureBlobMonitor
   {
      private readonly AzureBlobStorageProvider _provider;
      private readonly DateTime _latestDay;
      private long _offset;
      private string _lastFileName;

      public AzureBlobMonitor(AzureBlobStorageProvider provider)
      {
         _provider = provider;
         _latestDay = DateTime.UtcNow;
      }

      public async Task DoWork()
      {
         while (true)
         {
            string fileName = await ListFiles();

            if(fileName != _lastFileName)
            {
               _offset = 0;
               _lastFileName = fileName;
            }

            await Tail(fileName);

            await Task.Delay(TimeSpan.FromSeconds(1));
         }
      }

      private async Task<string> ListFiles()
      {
         string folderPath = $"blob/{_latestDay.Year}/{_latestDay.Month:D2}/{_latestDay.Day:D2}";

         List<BlobId> ids = (await _provider.ListAsync(
            new ListOptions { FolderPath = folderPath, Recurse = true },
            CancellationToken.None))
            .ToList();

         return ids.Last().FullPath;
      }

      private async Task Tail(string fileName)
      {
         CloudBlockBlob cbb = _provider.NativeBlobContainer.GetBlockBlobReference(fileName);

         await cbb.FetchAttributesAsync();

         long length = cbb.Properties.Length - _offset;

         if (length > 0)
         {
            byte[] buffer = new byte[length];

            await cbb.DownloadRangeToByteArrayAsync(buffer, 0, _offset, buffer.Length);

            _offset = cbb.Properties.Length;

            List<AzureBlobEvent> events = ToBlobEvents(buffer);

            foreach(AzureBlobEvent e in events)
            {
               Console.WriteLine(e);
            }
         }
      }

      private List<AzureBlobEvent> ToBlobEvents(byte[] data)
      {
         var result = new List<AzureBlobEvent>();
         string str = Encoding.UTF8.GetString(data);

         string[] lines = str.Split(new char[] { '\n' }, StringSplitOptions.RemoveEmptyEntries).Select(l => l.Trim('\r')).ToArray();

         foreach(string line in lines)
         {
            string[] parts = line.Split(';');

            var ev = new AzureBlobEvent(parts);

            result.Add(ev);

         }

         return result;
      }
   }
}

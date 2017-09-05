using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.Management.DataLake.Store;
using Microsoft.Azure.Management.DataLake.Store.Models;
using Storage.Net.Blob;

namespace Storage.Net.Microsoft.Azure.DataLake.Store.Blob
{
   class DirectoryBrowser
   {
      private readonly DataLakeStoreFileSystemManagementClient _client;
      private readonly string _accountName;

      public DirectoryBrowser(DataLakeStoreFileSystemManagementClient client, string accountName)
      {
         _client = client;
         _accountName = accountName;
      }

      public async Task<IEnumerable<BlobId>> Browse(ListOptions options, CancellationToken token)
      {
         string path = StoragePath.Normalize(options.FolderPath);
         var result = new List<BlobId>();

         await Browse(path, options, result, token);

         return result;
      }

      private async Task Browse(string path, ListOptions options, ICollection<BlobId> container, CancellationToken token)
      {
         FileStatusesResult statuses;

         try
         {
            statuses = await _client.FileSystem.ListFileStatusAsync(_accountName, path,
               null, null, null, null,
               token);
         }
         //skip files with forbidden access
         catch (AdlsErrorException ex) when (ex.Response.StatusCode == HttpStatusCode.Forbidden)
         {
            statuses = null;
         }

         if (statuses == null) return;

         List<BlobId> batch =
            statuses.FileStatuses.FileStatus
               .Select(p => ToBlobId(path, p))
               .Where(options.IsMatch)
               .ToList();

         if (options.Add(container, batch)) return;

         if(options.Recurse)
         {
            int folderCount = batch.Count(b => b.Kind == BlobItemKind.Folder);
            if(folderCount > 0)
            {
               await Task.WhenAll(batch.Select(bid => Browse(
                  StoragePath.Combine(path, bid.Id),
                  options,
                  container,
                  token
                  )));
            }
         }
      }

      private static BlobId ToBlobId(string path, FileStatusProperties properties)
      {
         if (properties.Type == FileType.FILE)
            return new BlobId(path, properties.PathSuffix, BlobItemKind.File);
         else
            return new BlobId(path, properties.PathSuffix, BlobItemKind.Folder);
      }
   }
}

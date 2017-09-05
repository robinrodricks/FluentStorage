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

         await Browse(path, options.Prefix, options.Recurse, result, token);

         if (options.Prefix == null) return result;

         return result.Where(i => i.Id.StartsWith(options.Prefix));
      }

      private async Task Browse(string path, string prefix, bool recurse, ICollection<BlobId> container, CancellationToken token)
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
               .Where(b => prefix == null || b.Id.StartsWith(prefix))
               .ToList();

         container.AddRange(batch);

         if(recurse)
         {
            int folderCount = batch.Count(b => b.Kind == BlobItemKind.Folder);
            if(folderCount > 0)
            {
               await Task.WhenAll(batch.Select(bid => Browse(
                  StoragePath.Combine(path, bid.Id),
                  prefix,
                  recurse,
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

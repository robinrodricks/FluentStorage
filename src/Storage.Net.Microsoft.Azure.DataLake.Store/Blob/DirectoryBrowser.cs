using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.DataLake.Store;
using Microsoft.Azure.Management.DataLake.Store;
using Microsoft.Azure.Management.DataLake.Store.Models;
using Storage.Net.Blob;

namespace Storage.Net.Microsoft.Azure.DataLake.Store.Blob
{
   class DirectoryBrowser
   {
      private readonly AdlsClient _client;

      public DirectoryBrowser(AdlsClient client)
      {
         _client = client ?? throw new ArgumentNullException(nameof(client));
      }

      public async Task<IReadOnlyCollection<BlobId>> Browse(ListOptions options, CancellationToken token)
      {
         string path = StoragePath.Normalize(options.FolderPath);
         var result = new List<BlobId>();

         await Browse(path, options, result, token);

         return result;
      }

      private async Task Browse(string path, ListOptions options, ICollection<BlobId> container, CancellationToken token)
      {
         List<DirectoryEntry> dirEntries;

         try
         {
            dirEntries = _client.EnumerateDirectory(path, UserGroupRepresentation.ObjectID, token).ToList();
         }
         //skip files with forbidden access
         catch (AdlsException ex) when (ex.HttpStatus == HttpStatusCode.Forbidden || ex.HttpStatus == HttpStatusCode.NotFound)
         {
            dirEntries = null;
         }

         if (dirEntries == null) return;

         List<BlobId> batch =
            dirEntries
               .Select(p => ToBlobId(path, p, options.IncludeMetaWhenKnown))
               .Where(options.IsMatch)
               .ToList();

         if (options.Add(container, batch)) return;

         if(options.Recurse)
         {
            List<BlobId> folders = batch.Where(b => b.Kind == BlobItemKind.Folder).ToList();

            if(folders.Count > 0)
            {
               await Task.WhenAll(
                  folders.Select(bid => Browse(
                     StoragePath.Combine(path, bid.Id),
                     options,
                     container,
                     token
                  )));
            }
         }
      }

      private static BlobId ToBlobId(string path, DirectoryEntry entry, bool includeMeta)
      {
         BlobMeta meta = includeMeta ? new BlobMeta(entry.Length, null, entry.LastModifiedTime) : null;

         if (entry.Type == DirectoryEntryType.FILE)
            return new BlobId(path, entry.Name, BlobItemKind.File) { Meta = meta };
         else
            return new BlobId(path, entry.Name, BlobItemKind.Folder) { Meta = meta };
      }
   }
}

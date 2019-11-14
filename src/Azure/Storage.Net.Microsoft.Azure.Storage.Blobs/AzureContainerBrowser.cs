using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using NetBox.Async;
using Storage.Net.Blobs;

namespace Storage.Net.Microsoft.Azure.Storage.Blobs
{
   class AzureContainerBrowser : IDisposable
   {
      private readonly BlobContainerClient _client;
      private readonly bool _prependContainerName;
      private readonly AsyncLimiter _asyncLimiter;

      public AzureContainerBrowser(BlobContainerClient client, bool prependContainerName, int maxTasks)
      {
         _client = client ?? throw new ArgumentNullException(nameof(client));
         _prependContainerName = prependContainerName;
         _asyncLimiter = new AsyncLimiter(maxTasks);
      }

      public async Task<IReadOnlyCollection<Blob>> ListFolderAsync(ListOptions options, CancellationToken cancellationToken)
      {
         await foreach(BlobHierarchyItem item in _client.GetBlobsByHierarchyAsync(prefix: options.FolderPath).ConfigureAwait(false))
         {
            string name = item.Prefix;
         }

         //_client.GetBlobsByHierarchyAsync()

         return Enumerable.Empty<Blob>().ToList();
      }

      public void Dispose()
      {
         _asyncLimiter.Dispose();
      }
   }
}

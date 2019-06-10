using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.Storage.Blob;
using Storage.Net.Blobs;

namespace Storage.Net.Microsoft.Azure.Storage.Blobs
{
   class AzureBlobDirectoryBrowser
   {
      private readonly CloudBlobContainer _container;
      private readonly SemaphoreSlim _throttler;

      public AzureBlobDirectoryBrowser(CloudBlobContainer container, int maxTasks)
      {
         _container = container;
         _throttler = new SemaphoreSlim(maxTasks);
      }

      public async Task<IReadOnlyCollection<Blob>> ListFolderAsync(ListOptions options, CancellationToken cancellationToken)
      {
         var result = new List<Blob>();

         await ListFolderAsync(result, options.FolderPath, options, cancellationToken).ConfigureAwait(false);

         return result;
      }

      private async Task ListFolderAsync(List<Blob> container, string path, ListOptions options, CancellationToken cancellationToken)
      {
         CloudBlobDirectory dir = GetCloudBlobDirectory(path);

         BlobContinuationToken token = null;

         var batch = new List<Blob>();

         await _throttler.WaitAsync();
         try
         {
            do
            {
               BlobResultSegment segment = await dir.ListBlobsSegmentedAsync(
                  false, BlobListingDetails.None, null, token, null, null, cancellationToken).ConfigureAwait(false);

               token = segment.ContinuationToken;

               foreach (IListBlobItem blob in segment.Results)
               {
                  Blob id = await ToBlobIdAsync(blob, options.IncludeAttributes).ConfigureAwait(false);

                  if (options.IsMatch(id) && (options.BrowseFilter == null || options.BrowseFilter(id)))
                  {
                     batch.Add(id);
                  }
               }

            }
            while (token != null && ((options.MaxResults == null) || (container.Count + batch.Count < options.MaxResults.Value)));
         }
         finally
         {
            _throttler.Release();
         }

         batch = batch.Where(options.IsMatch).ToList();
         if (options.Add(container, batch)) return;

         if (options.Recurse)
         {
            var folderIds = batch.Where(r => r.Kind == BlobItemKind.Folder).ToList();

            await Task.WhenAll(
               folderIds.Select(folderId => ListFolderAsync(
                  container,
                  StoragePath.Combine(path, folderId.Id),
                  options,
                  cancellationToken))).ConfigureAwait(false);
         }
      }

      private CloudBlobDirectory GetCloudBlobDirectory(string path)
      {
         path = path == null ? string.Empty : path.Trim(StoragePath.PathSeparator);

         CloudBlobDirectory dir = _container.GetDirectoryReference(path);

         return dir;
      }

      private async Task<Blob> ToBlobIdAsync(IListBlobItem blob, bool attachMetadata)
      {
         Blob id;

         if (blob is CloudBlockBlob blockBlob)
         {
            string fullName = StoragePath.Combine(_container.Name, blockBlob.Name);

            id = new Blob(fullName, BlobItemKind.File);
         }
         else if (blob is CloudAppendBlob appendBlob)
         {
            string fullName = StoragePath.Combine(_container.Name, appendBlob.Name);

            id = new Blob(fullName, BlobItemKind.File);
         }
         else if (blob is CloudBlobDirectory dirBlob)
         {
            string fullName = StoragePath.Combine(_container.Name, dirBlob.Prefix);

            id = new Blob(fullName, BlobItemKind.Folder);
         }
         else
         {
            throw new InvalidOperationException($"unknown item type {blob.GetType()}");
         }

         //attach metadata if we can
         if(attachMetadata && blob is CloudBlob cloudBlob)
         {
            await cloudBlob.FetchAttributesAsync().ConfigureAwait(false);
            AzureUniversalBlobStorageProvider.AttachBlobMeta(id, cloudBlob);
         }

         return id;

      }

   }
}

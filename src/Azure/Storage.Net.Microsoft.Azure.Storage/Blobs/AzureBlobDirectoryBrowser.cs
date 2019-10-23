
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.Storage.Blob;
using Storage.Net.Blobs;

namespace Storage.Net.Microsoft.Azure.Storage.Blobs
{
   class AzureBlobDirectoryBrowser : IDisposable
   {
      private readonly CloudBlobContainer _container;
      private readonly bool _prependContainerName;
      private readonly SemaphoreSlim _throttler;

      public AzureBlobDirectoryBrowser(CloudBlobContainer container, bool prependContainerName, int maxTasks)
      {
         _container = container;
         _prependContainerName = prependContainerName;
         _throttler = new SemaphoreSlim(maxTasks);
      }

      public async Task<IReadOnlyCollection<Blob>> ListFolderAsync(ListOptions options, CancellationToken cancellationToken)
      {
         var result = new List<Blob>();

         await ListFolderAsync(result, options.FolderPath, options, cancellationToken).ConfigureAwait(false);

         return result;
      }

      public async Task RecursiveDeleteAsync(CloudBlobDirectory directory, CancellationToken cancellationToken)
      {
         BlobResultSegment segment = await directory.ListBlobsSegmentedAsync(null, cancellationToken).ConfigureAwait(false);
         foreach(IListBlobItem item in segment.Results)
         {
            if(item is CloudBlobDirectory iDir)
            {
               await RecursiveDeleteAsync(iDir, cancellationToken).ConfigureAwait(false);
            }
            else if(item is CloudBlockBlob iBlob)
            {
               await iBlob.DeleteIfExistsAsync(
                  DeleteSnapshotsOption.IncludeSnapshots, null, null, null, cancellationToken).ConfigureAwait(false);
            }
            else if(item is CloudAppendBlob iAppendBlob)
            {
               await iAppendBlob.DeleteIfExistsAsync().ConfigureAwait(false);
            }
         }
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
                  false,
                  //automatically include metadata in the response
                  options.IncludeAttributes ? BlobListingDetails.Metadata : BlobListingDetails.None,
                  null, token, null, null, cancellationToken).ConfigureAwait(false);

               token = segment.ContinuationToken;

               foreach (IListBlobItem listItem in segment.Results)
               {
                  Blob blob = ToBlob(listItem);

                  if (options.IsMatch(blob) && (options.BrowseFilter == null || options.BrowseFilter(blob)))
                  {
                     batch.Add(blob);
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
                  StoragePath.Combine(path, folderId.Name),
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

      private string GetFullName(string name)
      {
         return _prependContainerName
            ? StoragePath.Combine(_container.Name, name)
            : name;
      }

      private Blob ToBlob(IListBlobItem nativeBlob)
      {
         Blob blob;

         if (nativeBlob is CloudBlockBlob blockBlob)
         {
            string fullName = GetFullName(blockBlob.Name);

            blob = new Blob(fullName, BlobItemKind.File);
         }
         else if (nativeBlob is CloudAppendBlob appendBlob)
         {
            string fullName = GetFullName(appendBlob.Name);

            blob = new Blob(fullName, BlobItemKind.File);
         }
         else if (nativeBlob is CloudBlobDirectory dirBlob)
         {
            string fullName = GetFullName(dirBlob.Prefix);

            blob = new Blob(fullName, BlobItemKind.Folder);
         }
         else
         {
            throw new InvalidOperationException($"unknown item type {nativeBlob.GetType()}");
         }

         //attach metadata if we can
         if(nativeBlob is CloudBlob cloudBlob)
         {
            //no need to fetch attributes, parent request includes the details
            //await cloudBlob.FetchAttributesAsync().ConfigureAwait(false);
            AzureUniversalBlobStorageProvider.AttachBlobMeta(blob, cloudBlob);
         }

         return blob;
      }

      public void Dispose()
      {
         if(_throttler != null)
         {
            _throttler.Dispose();
         }
      }
   }
}

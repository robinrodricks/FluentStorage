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
   class AzureBlobDirectoryBrowser
   {
      private readonly CloudBlobContainer _container;

      public AzureBlobDirectoryBrowser(CloudBlobContainer container)
      {
         _container = container;
      }

      public async Task<IEnumerable<BlobId>> ListFolder(ListOptions options, CancellationToken cancellationToken)
      {
         var result = new List<BlobId>();

         await ListFolder(result, options.FolderPath, options, cancellationToken);

         return result;
      }

      public async Task ListFolder(List<BlobId> container, string path, ListOptions options, CancellationToken cancellationToken)
      {
         CloudBlobDirectory dir = GetCloudBlobDirectory(path);

         BlobContinuationToken token = null;

         var batch = new List<BlobId>();

         do
         {
            BlobResultSegment segment = await dir.ListBlobsSegmentedAsync(
               false, BlobListingDetails.None, null, token, null, null, cancellationToken);

            token = segment.ContinuationToken;

            foreach (IListBlobItem blob in segment.Results)
            {
               BlobId id;

               if (blob is CloudBlockBlob blockBlob)
                  id = new BlobId(blockBlob.Name, BlobItemKind.File);
               else if (blob is CloudAppendBlob appendBlob)
                  id = new BlobId(appendBlob.Name, BlobItemKind.File);
               else if (blob is CloudBlobDirectory dirBlob)
                  id = new BlobId(dirBlob.Prefix, BlobItemKind.Folder);
               else
                  throw new InvalidOperationException($"unknown item type {blob.GetType()}");

               if (options.IsMatch(id))
               {
                  batch.Add(id);
               }
            }

         }
         while (token != null && ((options.MaxResults == null) || ( container.Count + batch.Count < options.MaxResults.Value)));

         batch = batch.Where(options.IsMatch).ToList();
         if (options.Add(container, batch)) return;

         if (options.Recurse)
         {
            List<BlobId> folderIds = batch.Where(r => r.Kind == BlobItemKind.Folder).ToList();
            foreach (BlobId folderId in folderIds)
            {
               await ListFolder(
                  container,
                  StoragePath.Combine(path, folderId.Id),
                  options,
                  cancellationToken);
            }
         }
      }

      private CloudBlobDirectory GetCloudBlobDirectory(string path)
      {
         path = path == null ? string.Empty : path.Trim(StoragePath.PathSeparator);

         CloudBlobDirectory dir = _container.GetDirectoryReference(path);

         return dir;
      }

   }
}

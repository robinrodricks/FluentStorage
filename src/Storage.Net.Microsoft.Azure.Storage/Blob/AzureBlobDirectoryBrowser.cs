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

      public async Task<IEnumerable<BlobId>> ListFolder(string path, string prefix, bool recurse, CancellationToken cancellationToken)
      {
         CloudBlobDirectory dir = GetCloudBlobDirectory(path);

         var result = new List<BlobId>();

         BlobContinuationToken token = null;

         //blob path cannot start with '/'
         //string listPrefix = StoragePath.Combine(path, prefix).Trim(StoragePath.PathSeparator);
         //if (prefix != null) listPrefix = StoragePath.Combine(listPrefix, prefix);

         do
         {
            BlobResultSegment segment = await dir.ListBlobsSegmentedAsync(
               false, BlobListingDetails.None, null, token, null, null, cancellationToken);

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

               if(prefix == null ||  id.Id.StartsWith(prefix))
                  result.Add(id);
            }

         }
         while (token != null);

         if (recurse)
         {
            List<BlobId> folderIds = result.Where(r => r.Kind == BlobItemKind.Folder).ToList();
            foreach (BlobId folderId in folderIds)
            {
               IEnumerable<BlobId> children = await ListFolder(
                  StoragePath.Combine(path, folderId.Id),
                  prefix,
                  recurse,
                  cancellationToken);

               result.AddRange(children);
            }
         }

         return result;
      }

      private CloudBlobDirectory GetCloudBlobDirectory(string path)
      {
         path = path == null ? string.Empty : path.Trim(StoragePath.PathSeparator);

         CloudBlobDirectory dir = _container.GetDirectoryReference(path);

         return dir;
      }

   }
}

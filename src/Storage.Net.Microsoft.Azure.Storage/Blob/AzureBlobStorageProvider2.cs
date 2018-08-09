using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Auth;
using Microsoft.WindowsAzure.Storage.Blob;
using Storage.Net.Blob;

namespace Storage.Net.Microsoft.Azure.Storage.Blob
{
   class AzureBlobStorageProvider2 : IBlobStorage
   {
      private readonly CloudBlobClient _client;
      private readonly Dictionary<string, CloudBlobContainer> _containerNameToContainer = new Dictionary<string, CloudBlobContainer>();

      public AzureBlobStorageProvider2(string accountName, string key)
      {
         var account = new CloudStorageAccount(
            new StorageCredentials(accountName, key),
            true);

         _client = account.CreateCloudBlobClient();
      }

      public Task DeleteAsync(IEnumerable<string> ids, CancellationToken cancellationToken = default)
      {
         throw new NotImplementedException();
      }

      public void Dispose()
      {
         
      }

      public Task<IReadOnlyCollection<bool>> ExistsAsync(IEnumerable<string> ids, CancellationToken cancellationToken = default)
      {
         throw new NotImplementedException();
      }

      public Task<IEnumerable<BlobMeta>> GetMetaAsync(IEnumerable<string> ids, CancellationToken cancellationToken = default)
      {
         throw new NotImplementedException();
      }

      public Task<IReadOnlyCollection<BlobId>> ListAsync(ListOptions options, CancellationToken cancellationToken = default)
      {
         throw new NotImplementedException();
      }

      public Task<Stream> OpenReadAsync(string id, CancellationToken cancellationToken = default)
      {
         throw new NotImplementedException();
      }

      public Task<ITransaction> OpenTransactionAsync()
      {
         throw new NotImplementedException();
      }

      public async Task<Stream> OpenWriteAsync(string id, bool append, CancellationToken cancellationToken)
      {
         (CloudBlobContainer container, string path) = await GetPartsAsync(id);

         if (append)
         {
            CloudAppendBlob cab = container.GetAppendBlobReference(path);

            return await cab.OpenWriteAsync(!append);
         }
         else
         {
            CloudBlockBlob cab = container.GetBlockBlobReference(path);

            return await cab.OpenWriteAsync();

         }
      }

      public async Task WriteAsync(string id, Stream sourceStream, bool append, CancellationToken cancellationToken)
      {
         GenericValidation.CheckSourceStream(sourceStream);

         (CloudBlobContainer container, string path) = await GetPartsAsync(id);

         if (append)
         {
            CloudAppendBlob cab = container.GetAppendBlobReference(StoragePath.Normalize(id, false));
            if (!(await cab.ExistsAsync())) await cab.CreateOrReplaceAsync();

            await cab.AppendFromStreamAsync(sourceStream);

         }
         else
         {
            CloudBlockBlob blob = container.GetBlockBlobReference(StoragePath.Normalize(id, false));

            await blob.UploadFromStreamAsync(sourceStream);
         }
      }

      #region [ Path forking ]

      private async Task<(CloudBlobContainer, string)> GetPartsAsync(string path)
      {
         GenericValidation.CheckBlobId(path);

         path = StoragePath.Normalize(path);
         if (path == null) throw new ArgumentNullException(nameof(path));
         int idx = path.IndexOf(StoragePath.PathSeparator);
         if (idx == -1) throw new ArgumentException("blob path must contain container name", nameof(path));

         string containerName = path.Substring(0, idx);
         string relativePath = path.Substring(idx + 1);

         if (!_containerNameToContainer.TryGetValue(containerName, out CloudBlobContainer container))
         {
            container = _client.GetContainerReference(containerName);
            await container.CreateIfNotExistsAsync();
            _containerNameToContainer[containerName] = container;
         }

         return (container, relativePath);
      }

      private string GetRelativePath(string path)
      {
         int idx = path.IndexOf(StoragePath.PathSeparator);
         if (idx == -1) throw new ArgumentException("blob path must contain container name", nameof(path));

         return path.Substring(idx);
      }

      #endregion
   }
}

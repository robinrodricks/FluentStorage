using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Storage.Net.Blobs;

namespace Storage.Net.Microsoft.Azure.Storage.Blobs
{
   //auth scenarios: https://github.com/Azure/azure-sdk-for-net/blob/master/sdk/storage/Azure.Storage.Blobs/samples/Sample02_Auth.cs


   class AzureBlobStorage : IAzureBlobStorage12
   {
      private readonly BlobServiceClient _client;
      private readonly string _containerName;
      private readonly Dictionary<string, BlobContainerClient> _containerNameToContainerClient =
         new Dictionary<string, BlobContainerClient>();

      public AzureBlobStorage(BlobServiceClient blobServiceClient)
      {
         _client = blobServiceClient;
      }

      private async Task<IReadOnlyCollection<Blob>> ListContainersAsync(CancellationToken cancellationToken)
      {
         var r = new List<Blob>();

         await foreach(BlobContainerItem container in _client.GetBlobContainersAsync(BlobContainerTraits.Metadata).ConfigureAwait(false))
         {
            r.Add(AzConvert.ToBlob(container));
         }

         return r;
      }

      #region [ Interface Methods ]

      public async Task<IReadOnlyCollection<Blob>> ListAsync(ListOptions options = null, CancellationToken cancellationToken = default)
      {
         if(options == null)
            options = new ListOptions();

         var containers = new List<Blob>();

         if(StoragePath.IsRootPath(options.FolderPath) && _containerName == null)
         {
            // list all of the containers
            containers.AddRange(await ListContainersAsync(cancellationToken));

         }

         return null;
      }


      public Task DeleteAsync(IEnumerable<string> fullPaths, CancellationToken cancellationToken = default) => throw new NotImplementedException();
      public void Dispose() { }
      public Task<IReadOnlyCollection<bool>> ExistsAsync(IEnumerable<string> fullPaths, CancellationToken cancellationToken = default) => throw new NotImplementedException();
      public Task<IReadOnlyCollection<Blob>> GetBlobsAsync(IEnumerable<string> fullPaths, CancellationToken cancellationToken = default) => throw new NotImplementedException();
      public Task<Stream> OpenReadAsync(string fullPath, CancellationToken cancellationToken = default) => throw new NotImplementedException();
      public Task<ITransaction> OpenTransactionAsync() => throw new NotImplementedException();
      public Task<Stream> OpenWriteAsync(string fullPath, bool append = false, CancellationToken cancellationToken = default) => throw new NotImplementedException();
      public Task SetBlobsAsync(IEnumerable<Blob> blobs, CancellationToken cancellationToken = default) => throw new NotImplementedException();

      #endregion

      private async Task<(BlobContainerClient, string)> GetPartsAsync(string fullPath, bool createContainer = true)
      {
         GenericValidation.CheckBlobFullPath(fullPath);

         fullPath = StoragePath.Normalize(fullPath);
         if(fullPath == null)
            throw new ArgumentNullException(nameof(fullPath));

         string containerName, relativePath;

         if(_containerName == null)
         {
            int idx = fullPath.IndexOf(StoragePath.PathSeparator);
            if(idx == -1)
            {
               containerName = fullPath;
               relativePath = string.Empty;
            }
            else
            {
               containerName = fullPath.Substring(0, idx);
               relativePath = fullPath.Substring(idx + 1);
            }
         }
         else
         {
            containerName = _containerName;
            relativePath = fullPath;
         }

         if(!_containerNameToContainerClient.TryGetValue(containerName, out BlobContainerClient container))
         {
            container = _client.GetBlobContainerClient(containerName);
            if(_containerName == null)
            {
               if(createContainer)
               {
                  await container.CreateIfNotExistsAsync().ConfigureAwait(false);
               }
               else
               {
                  return (null, null);
               }
            }

            _containerNameToContainerClient[containerName] = container;
         }

         return (container, relativePath);
      }

   }
}

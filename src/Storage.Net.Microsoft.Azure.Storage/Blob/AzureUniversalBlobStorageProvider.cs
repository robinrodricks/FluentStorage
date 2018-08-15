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
using NetBox.Extensions;
using Storage.Net.Blob;
using AzureStorageException = Microsoft.WindowsAzure.Storage.StorageException;

namespace Storage.Net.Microsoft.Azure.Storage.Blob
{
   class AzureUniversalBlobStorageProvider : IBlobStorage, IAzureBlobStorageNativeOperations
   {
      private readonly CloudBlobClient _client;
      private readonly Dictionary<string, CloudBlobContainer> _containerNameToContainer = new Dictionary<string, CloudBlobContainer>();
      private readonly CloudBlobContainer _fixedContainer;
      private readonly string _fixedContainerName;

      public CloudBlobClient NativeBlobClient => _client;

      public AzureUniversalBlobStorageProvider(string accountName, string key, string containerName = null)
      {
         var account = new CloudStorageAccount(
            new StorageCredentials(accountName, key),
            true);

         _client = account.CreateCloudBlobClient();
         _fixedContainerName = containerName;
      }

      private AzureUniversalBlobStorageProvider(Uri sasUri)
      {
         if (sasUri == null)
         {
            throw new ArgumentNullException(nameof(sasUri));
         }

         _fixedContainer = new CloudBlobContainer(sasUri);
         _client = _fixedContainer.ServiceClient;
      }

      public static AzureUniversalBlobStorageProvider CreateWithContainerSasUri(Uri sasUri)
      {
         return new AzureUniversalBlobStorageProvider(sasUri);
      }

      public async Task DeleteAsync(IEnumerable<string> ids, CancellationToken cancellationToken = default)
      {
         GenericValidation.CheckBlobId(ids);

         await Task.WhenAll(ids.Select(id => DeleteAsync(id, cancellationToken)));
      }

      private async Task DeleteAsync(string id, CancellationToken cancellationToken)
      {
         (CloudBlobContainer container, string path) = await GetPartsAsync(id, false);

         CloudBlockBlob blob = container.GetBlockBlobReference(StoragePath.Normalize(path, false));
         await blob.DeleteIfExistsAsync();
      }

      public void Dispose()
      {
         
      }

      public async Task<IReadOnlyCollection<bool>> ExistsAsync(IEnumerable<string> ids, CancellationToken cancellationToken = default)
      {
         var result = new List<bool>();

         foreach (string id in ids)
         {
            GenericValidation.CheckBlobId(id);

            (CloudBlobContainer container, string path) = await GetPartsAsync(id, false);
            if (container == null)
            {
               result.Add(false);
            }
            else
            {
               CloudBlockBlob blob = container.GetBlockBlobReference(StoragePath.Normalize(path, false));
               bool exists = await blob.ExistsAsync();
               result.Add(exists);
            }
         }

         return result;
      }

      public async Task<IEnumerable<BlobMeta>> GetMetaAsync(IEnumerable<string> ids, CancellationToken cancellationToken = default)
      {
         var result = new List<BlobMeta>();
         foreach (string id in ids)
         {
            GenericValidation.CheckBlobId(id);
         }

         return await Task.WhenAll(ids.Select(id => GetMetaAsync(id, cancellationToken)));
      }

      private async Task<BlobMeta> GetMetaAsync(string id, CancellationToken cancellationToken)
      {
         (CloudBlobContainer container, string path) = await GetPartsAsync(id, false);
         if (container == null) return null;

         CloudBlob blob = container.GetBlobReference(StoragePath.Normalize(path, false));
         if (!(await blob.ExistsAsync())) return null;

         await blob.FetchAttributesAsync();

         return GetblobMeta(blob);
      }

      internal static BlobMeta GetblobMeta(CloudBlob blob)
      {
         //ContentMD5 is base64-encoded hash, whereas we work with HEX encoded ones
         string md5 = blob.Properties.ContentMD5.Base64DecodeAsBytes().ToHexString();

         var meta = new BlobMeta(
            blob.Properties.Length,
            md5,
            blob.Properties.LastModified);

         return meta;
      }

      public async Task<IReadOnlyCollection<BlobId>> ListAsync(ListOptions options, CancellationToken cancellationToken = default)
      {
         if (options == null) options = new ListOptions();

         var result = new List<BlobId>();
         var containers = new List<CloudBlobContainer>();

         if(_fixedContainer != null)
         {
            containers.Add(_fixedContainer);
         }
         else if(options.FolderPath == null)
         {
            // list all of the containers
            containers.AddRange(await GetCloudBlobContainersAsync(cancellationToken));

            //represent containers as folders in the result
            result.AddRange(containers.Select(c => new BlobId(c.Name, BlobItemKind.Folder)));
         }
         else
         {
            (CloudBlobContainer container, string path) = await GetPartsAsync(options.FolderPath, false);
            if (container == null) return new List<BlobId>();
            options.FolderPath = path; //scan from subpath now
            containers.Add(container);

            //add container as search result
            result.Add(new BlobId(container.Name, BlobItemKind.Folder));
         }

         foreach(CloudBlobContainer container in containers)
         {
            var browser = new AzureBlobDirectoryBrowser(container);
            IReadOnlyCollection<BlobId> containerBlobs = await browser.ListFolderAsync(options, cancellationToken);
            if (containerBlobs.Count > 0)
            {
               if (_fixedContainer == null)
               {
                  result.AddRange(containerBlobs.Select(bid => new BlobId(StoragePath.Combine(container.Name, bid.FullPath), bid.Kind)));
               }
               else
               {
                  result.AddRange(containerBlobs);
               }
            }

            if (options.MaxResults != null && result.Count >= options.MaxResults.Value)
            {
               break;
            }
         }

         return result;
      }

      private async Task<List<CloudBlobContainer>> GetCloudBlobContainersAsync(CancellationToken cancellationToken)
      {
         var result = new List<CloudBlobContainer>();

         ContainerResultSegment firstPage = await _client.ListContainersSegmentedAsync(null);
         result.AddRange(firstPage.Results);

         //todo: list more containers

         return result;
      }

      public async Task<Stream> OpenReadAsync(string id, CancellationToken cancellationToken = default)
      {
         GenericValidation.CheckBlobId(id);

         (CloudBlobContainer container, string path) = await GetPartsAsync(id, false);

         if (container == null) return null;

         CloudBlockBlob blob = container.GetBlockBlobReference(StoragePath.Normalize(path, false));

         try
         {
            return await blob.OpenReadAsync();
         }
         catch (AzureStorageException ex)
         {
            if (AzureStorageValidation.IsDoesntExist(ex)) return null;

            if (!AzureStorageValidation.TryHandleStorageException(ex)) throw;
         }

         throw new Exception("must not be here");
      }

      public Task<ITransaction> OpenTransactionAsync()
      {
         return Task.FromResult(EmptyTransaction.Instance);
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
            CloudAppendBlob cab = container.GetAppendBlobReference(StoragePath.Normalize(path, false));
            if (!(await cab.ExistsAsync())) await cab.CreateOrReplaceAsync();

            await cab.AppendFromStreamAsync(sourceStream);

         }
         else
         {
            CloudBlockBlob blob = container.GetBlockBlobReference(StoragePath.Normalize(path, false));

            await blob.UploadFromStreamAsync(sourceStream);
         }
      }

      #region [ Path forking ]

      private async Task<(CloudBlobContainer, string)> GetPartsAsync(string path, bool createContainer = true)
      {
         GenericValidation.CheckBlobId(path);

         path = StoragePath.Normalize(path);
         if (path == null) throw new ArgumentNullException(nameof(path));

         if(_fixedContainer != null)
         {
            return (_fixedContainer, path);
         }

         if(_fixedContainerName != null)
         {
            CloudBlobContainer fxc = _client.GetContainerReference(_fixedContainerName);
            await fxc.CreateIfNotExistsAsync();
            return (fxc, path);
         }

         int idx = path.IndexOf(StoragePath.PathSeparator);
         if (idx == -1) throw new ArgumentException("blob path must contain container name", nameof(path));

         string containerName = path.Substring(0, idx);
         string relativePath = path.Substring(idx + 1);

         if (!_containerNameToContainer.TryGetValue(containerName, out CloudBlobContainer container))
         {
            if (!createContainer) return (null, null);

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

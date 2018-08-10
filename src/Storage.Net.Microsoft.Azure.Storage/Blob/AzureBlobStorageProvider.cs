using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using Storage.Net.Blob;
using Microsoft.WindowsAzure.Storage.Blob;
using AzureStorageException = Microsoft.WindowsAzure.Storage.StorageException;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Auth;
using System.Threading.Tasks;
using System.Threading;
using NetBox.Extensions;

namespace Storage.Net.Microsoft.Azure.Storage.Blob
{
   /// <summary>
   /// Azure Blob Storage
   /// </summary>
   public class AzureBlobStorageProvider : IBlobStorage
   {
      private readonly CloudBlobClient _client;
      private readonly CloudBlobContainer _blobContainer;

      /// <summary>
      /// Creates an instance from account name, private key and container name
      /// </summary>
      public AzureBlobStorageProvider(string accountName, string key, string containerName, bool createIfNotExists = true)
      {
         AzureStorageValidation.ValidateContainerName(containerName);

         var account = new CloudStorageAccount(
            new StorageCredentials(accountName, key),
            true);

         _client = account.CreateCloudBlobClient();

         _blobContainer = _client.GetContainerReference(containerName);

         if (createIfNotExists)
         {
            _blobContainer.CreateIfNotExistsAsync().Wait();
         }
      }

      /// <summary>
      /// Create an instance from a SAS URL and container name
      /// </summary>
      /// <param name="sasUrl"></param>
      public AzureBlobStorageProvider(Uri sasUrl)
      {
         if (sasUrl == null)
         {
            throw new ArgumentNullException(nameof(sasUrl));
         }

         _blobContainer = new CloudBlobContainer(sasUrl);
         _client = _blobContainer.ServiceClient;
      }

      /// <summary>
      /// Returns reference to the native Azure SD blob client.
      /// </summary>
      public CloudBlobClient NativeBlobClient => _client;


      /// <summary>
      /// Returns reference to the native Azure SDK blob container
      /// </summary>
      public CloudBlobContainer NativeBlobContainer => _blobContainer;

      /// <summary>
      /// Get native Azure blob absolute URI by blob ID
      /// </summary>
      /// <param name="id">Blob ID</param>
      /// <returns>URI of the blob or null if blob doesn't exist</returns>
      public async Task<Uri> GetNativeBlobUriAsync(string id)
      {
         GenericValidation.CheckBlobId(id);

         CloudBlockBlob blob = _blobContainer.GetBlockBlobReference(StoragePath.Normalize(id, false));

         bool exists = await blob.ExistsAsync();
         if (!exists) return null;

         return blob.Uri;
      }

      /// <summary>
      /// Gets native Azure Blob absolute URI by blob ID and shared access policy
      /// </summary>
      /// <param name="id">Blob ID</param>
      /// <param name="policy">SAS policy</param>
      /// <returns>Blob URI with SAS policy, or null if blob doesn't exist</returns>
      public async Task<Uri> GetNativeSasUri(string id, SharedAccessBlobPolicy policy)
      {
         GenericValidation.CheckBlobId(id);
         if (policy == null) throw new ArgumentNullException(nameof(policy));

         CloudBlockBlob blob = _blobContainer.GetBlockBlobReference(StoragePath.Normalize(id, false));

         bool exists = await blob.ExistsAsync();
         if (!exists) return null;

         string sas = blob.GetSharedAccessSignature(policy);

         return new Uri(blob.Uri.ToString() + sas);
      }

      /// <summary>
      /// Creates and instance from network credential and container name
      /// </summary>
      /// <param name="credential"></param>
      /// <param name="containerName"></param>
      public AzureBlobStorageProvider(NetworkCredential credential, string containerName) :
         this(credential.UserName, credential.Password, containerName)
      {

      }

      /// <summary>
      /// Creates an insance from connection string
      /// </summary>
      public AzureBlobStorageProvider(string connectionString, string containerName)
      {
         if (connectionString == null) throw new ArgumentNullException(nameof(connectionString));
         AzureStorageValidation.ValidateContainerName(containerName);

         if (!CloudStorageAccount.TryParse(connectionString, out CloudStorageAccount account))
         {
            throw new ArgumentException("could not parse provided connection string");
         }

         CloudBlobClient blobClient = account.CreateCloudBlobClient();

         _blobContainer = blobClient.GetContainerReference(containerName);
         _blobContainer.CreateIfNotExistsAsync().Wait();
      }

      /// <summary>
      /// Gets all the blob names, then filters by prefix optionally
      /// </summary>
      public async Task<IReadOnlyCollection<BlobId>> ListAsync(ListOptions options, CancellationToken cancellationToken)
      {
         if (options == null) options = new ListOptions();

         var browser = new AzureBlobDirectoryBrowser(_blobContainer);

         return await browser.ListFolderAsync(options, cancellationToken); 
      }

      public async Task WriteAsync(string id, Stream sourceStream, bool append, CancellationToken cancellationToken)
      {
         GenericValidation.CheckBlobId(id);
         GenericValidation.CheckSourceStream(sourceStream);

         if (append)
         {
            CloudAppendBlob cab = _blobContainer.GetAppendBlobReference(StoragePath.Normalize(id, false));
            if (!(await cab.ExistsAsync())) await cab.CreateOrReplaceAsync();

            await cab.AppendFromStreamAsync(sourceStream);

         }
         else
         {
            CloudBlockBlob blob = _blobContainer.GetBlockBlobReference(StoragePath.Normalize(id, false));

            await blob.UploadFromStreamAsync(sourceStream);
         }
      }

      public async Task<Stream> OpenWriteAsync(string id, bool append, CancellationToken cancellationToken)
      {
         GenericValidation.CheckBlobId(id);

         id = StoragePath.Normalize(id, false);

         if (append)
         {
            CloudAppendBlob cab = _blobContainer.GetAppendBlobReference(id);
   
            return await cab.OpenWriteAsync(!append);
         }
         else
         {
            CloudBlockBlob cab = _blobContainer.GetBlockBlobReference(id);

            return await cab.OpenWriteAsync();

         }
      }

      public async Task<Stream> OpenReadAsync(string id, CancellationToken cancellationToken)
      {
         GenericValidation.CheckBlobId(id);

         CloudBlockBlob blob = _blobContainer.GetBlockBlobReference(StoragePath.Normalize(id, false));

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

      public async Task DeleteAsync(IEnumerable<string> ids, CancellationToken cancellationToken)
      {
         GenericValidation.CheckBlobId(ids);

         await Task.WhenAll(ids.Select(id => DeleteAsync(id, cancellationToken)));
      }

      private Task DeleteAsync(string id, CancellationToken cancellationToken)
      {
         CloudBlockBlob blob = _blobContainer.GetBlockBlobReference(StoragePath.Normalize(id, false));
         return blob.DeleteIfExistsAsync();
      }

      public async Task<IReadOnlyCollection<bool>> ExistsAsync(IEnumerable<string> ids, CancellationToken cancellationToken)
      {
         var result = new List<bool>();

         foreach (string id in ids)
         {
            GenericValidation.CheckBlobId(id);
            CloudBlockBlob blob = _blobContainer.GetBlockBlobReference(StoragePath.Normalize(id, false));
            bool exists = await blob.ExistsAsync();
            result.Add(exists);
         }

         return result;
      }

      public async Task<IEnumerable<BlobMeta>> GetMetaAsync(IEnumerable<string> ids, CancellationToken cancellationToken)
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
         CloudBlob blob = _blobContainer.GetBlobReference(StoragePath.Normalize(id, false));
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

      public void Dispose()
      {
      }

      public Task<ITransaction> OpenTransactionAsync()
      {
         return Task.FromResult(EmptyTransaction.Instance);
      }

   }
}
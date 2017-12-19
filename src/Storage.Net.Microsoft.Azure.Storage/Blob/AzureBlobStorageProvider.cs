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
   public class AzureBlobStorageProvider : IBlobStorageProvider
   {
      private readonly CloudBlobClient _client;
      private readonly CloudBlobContainer _blobContainer;

      /// <summary>
      /// Creates an instance from account name, private key and container name
      /// </summary>
      public AzureBlobStorageProvider(string accountName, string key, string containerName, bool createIfNotExists = true)
      {
         ValidateContainerName(containerName);

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
      /// <param name="containerName"></param>
      public AzureBlobStorageProvider(Uri sasUrl, string containerName)
      {
         _client = new CloudBlobClient(sasUrl);

         _blobContainer = _client.GetContainerReference(containerName);
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
         ValidateContainerName(containerName);

         if (!CloudStorageAccount.TryParse(connectionString, out CloudStorageAccount account))
         {
            throw new ArgumentException("could not parse provided connection string");
         }

         CloudBlobClient blobClient = account.CreateCloudBlobClient();

         _blobContainer = blobClient.GetContainerReference(containerName);
         _blobContainer.CreateIfNotExistsAsync().Wait();
      }

      private void ValidateContainerName(string containerName)
      {
         if (containerName == null) throw new ArgumentNullException(nameof(containerName));

         /* from MSDN:
          *
          A container name must be a valid DNS name, conforming to the following naming rules:
          1. Container names must start with a letter or number, and can contain only letters, numbers, and the dash (-) character.
          2. Every dash (-) character must be immediately preceded and followed by a letter or number; consecutive dashes are not permitted in container names.
          3. All letters in a container name must be lowercase.
          4. Container names must be from 3 through 63 characters long.
         */

         //1. todo

         //2. todo

         //3. check that all characters are lowercase
         for (int i = 0; i < containerName.Length; i++)
         {
            if (char.IsLetter(containerName[i]) && !char.IsLower(containerName, i))
            {
               throw new ArgumentOutOfRangeException(nameof(containerName),
                  $"container [{containerName}] has uppercase character at position {i}");
            }
         }

         //4. check for length
         if (containerName.Length < 3 || containerName.Length > 63)
         {
            throw new ArgumentOutOfRangeException(nameof(containerName),
               $"container [{containerName}] length must be between 3 and 63 but it's {containerName.Length}");
         }
      }

      /// <summary>
      /// Gets all the blob names, then filters by prefix optionally
      /// </summary>
      public async Task<IEnumerable<BlobId>> ListAsync(ListOptions options, CancellationToken cancellationToken)
      {
         if (options == null) options = new ListOptions();

         var browser = new AzureBlobDirectoryBrowser(_blobContainer);

         return await browser.ListFolder(options, cancellationToken); 
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
            if (IsDoesntExist(ex)) return null;

            if (!TryHandleStorageException(ex)) throw;
         }

         throw new Exception("must not be here");
      }

      public async Task DeleteAsync(IEnumerable<string> ids, CancellationToken cancellationToken)
      {
         foreach (string id in ids)
         {
            GenericValidation.CheckBlobId(id);

            CloudBlockBlob blob = _blobContainer.GetBlockBlobReference(StoragePath.Normalize(id, false));
            await blob.DeleteIfExistsAsync();
         }
      }

      public async Task<IEnumerable<bool>> ExistsAsync(IEnumerable<string> ids, CancellationToken cancellationToken)
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

         //ContentMD5 is base64-encoded hash, whereas we work with HEX encoded ones
         string md5 = blob.Properties.ContentMD5.Base64DecodeAsBytes().ToHexString();

         var meta = new BlobMeta(
            blob.Properties.Length,
            md5);

         return meta;
      }

      private static bool TryHandleStorageException(AzureStorageException ex)
      {
         if (IsDoesntExist(ex))
         {
            throw new StorageException(ErrorCode.NotFound, ex);
         }

         return false;
      }

      private static bool IsDoesntExist(AzureStorageException ex)
      {
         return ex.RequestInformation?.HttpStatusCode == 404;
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
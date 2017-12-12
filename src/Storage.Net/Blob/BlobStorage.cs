using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using NetBox.Extensions;

namespace Storage.Net.Blob
{
   /// <summary>
   /// Blob storage on steroids. Takes in <see cref="IBlobStorageProvider"/> and adds a lot of extra useful operations on top we as
   /// normal people use every day.
   /// </summary>
   public class BlobStorage
   {
      private const int BufferSize = 81920;

      private readonly IBlobStorageProvider _provider;

      /// <summary>
      /// Creates an instance of the blob storage helper.
      /// </summary>
      /// <param name="provider"></param>
      public BlobStorage(IBlobStorageProvider provider)
      {
         _provider = provider ?? throw new ArgumentNullException(nameof(provider));
      }

      /// <summary>
      /// Provider underneath this helper
      /// </summary>
      public IBlobStorageProvider Provider => _provider;

      #region [ Simple delegation ]

      /// <summary>
      /// Returns the list of available blobs
      /// </summary>
      /// <param name="options"></param>
      /// <param name="cancellationToken"></param>
      /// <returns>List of blob IDs</returns>
      public Task<IEnumerable<BlobId>> ListAsync(ListOptions options,
         CancellationToken cancellationToken = default(CancellationToken))
      {
         return _provider.ListAsync(options, cancellationToken);
      }

      /// <summary>
      /// Creates a new blob and returns a writeable stream to it. If the blob already exists it will be
      /// overwritten.
      /// </summary>
      /// <param name="id">Blob ID</param>
      /// <param name="sourceStream">Source stream, must be readable and support Length</param>
      /// <param name="cancellationToken"></param>
      /// <param name="append">When true, appends to the file instead of writing a new one.</param>
      /// <returns>Writeable stream</returns>
      /// <exception cref="ArgumentNullException">Thrown when any parameter is null</exception>
      /// <exception cref="ArgumentException">Thrown when ID is too long. Long IDs are the ones longer than 50 characters.</exception>
      public Task WriteAsync(string id, Stream sourceStream, bool append = false, CancellationToken cancellationToken = default(CancellationToken))
      {
         return _provider.WriteAsync(id, sourceStream, append, cancellationToken);
      }

      /// <summary>
      /// Opens the blob stream to read.
      /// </summary>
      /// <param name="id">Blob ID, required</param>
      /// <param name="cancellationToken"></param>
      /// <returns>Stream in an open state, or null if blob doesn't exist by this ID. It is your responsibility to close and dispose this
      /// stream after use.</returns>
      /// <exception cref="ArgumentNullException">Thrown when any parameter is null</exception>
      /// <exception cref="ArgumentException">Thrown when ID is too long. Long IDs are the ones longer than 50 characters.</exception>
      public Task<Stream> OpenReadAsync(string id, CancellationToken cancellationToken = default(CancellationToken))
      {
         return _provider.OpenReadAsync(id, cancellationToken);
      }

      /// <summary>
      /// Deletes a blob by id
      /// </summary>
      /// <param name="ids">Blob IDs to delete.</param>
      /// <param name="cancellationToken"></param>
      /// <exception cref="ArgumentNullException">Thrown when ID is null.</exception>
      /// <exception cref="ArgumentException">Thrown when ID is too long. Long IDs are the ones longer than 50 characters.</exception>
      public Task DeleteAsync(IEnumerable<string> ids, CancellationToken cancellationToken = default(CancellationToken))
      {
         return _provider.DeleteAsync(ids, cancellationToken);
      }

      public Task<IEnumerable<bool>> ExistsAsync(IEnumerable<string> ids, CancellationToken cancellationToken = default(CancellationToken))
      {
         return _provider.ExistsAsync(ids, cancellationToken);
      }

      /// <summary>
      /// Gets basic blob metadata
      /// </summary>
      /// <param name="ids">Blob id</param>
      /// <param name="cancellationToken"></param>
      /// <returns>Blob metadata or null if blob doesn't exist</returns>
      public Task<IEnumerable<BlobMeta>> GetMetaAsync(IEnumerable<string> ids, CancellationToken cancellationToken = default(CancellationToken))
      {
         return _provider.GetMetaAsync(ids, cancellationToken);
      }

      /// <summary>
      /// Starts a new transaction
      /// </summary>
      /// <returns></returns>
      public Task<ITransaction> OpenTransactionAsync()
      {
         return _provider.OpenTransactionAsync();
      }

      #endregion

      #region [ List Helpers ]

      /// <summary>
      /// Returns the list of available files, excluding folders.
      /// </summary>
      /// <param name="options"></param>
      /// <param name="cancellationToken"></param>
      /// <returns>List of blob IDs</returns>
      public async Task<IEnumerable<BlobId>> ListFilesAsync(ListOptions options,
         CancellationToken cancellationToken = default(CancellationToken))
      {
         IEnumerable<BlobId> all = await _provider.ListAsync(options, cancellationToken);

         return all.Where(i => i.Kind == BlobItemKind.File);
      }

      #endregion

      #region [ Text ]

      /// <summary>
      /// Reads blob content and converts to text in UTF-8 encoding
      /// </summary>
      /// <param name="id">Blob id</param>
      /// <param name="cancellationToken"></param>
      /// <returns></returns>
      public async Task<string> ReadTextAsync(string id, CancellationToken cancellationToken = default(CancellationToken))
      {
         Stream src = await _provider.OpenReadAsync(id, cancellationToken);
         if (src == null) return null;

         var ms = new MemoryStream();
         using (src)
         {
            await src.CopyToAsync(ms);
         }

         return Encoding.UTF8.GetString(ms.ToArray());
      }

      /// <summary>
      /// Converts text to blob content and writes to storage
      /// </summary>
      /// <param name="id">Blob id</param>
      /// <param name="text">Text to write, treated in UTF-8 encoding</param>
      /// <param name="cancellationToken"></param>
      /// <returns></returns>
      public async Task WriteTextAsync(string id, string text, CancellationToken cancellationToken = default(CancellationToken))
      {
         using (Stream s = text.ToMemoryStream())
         {
            await _provider.WriteAsync(id, s, false, cancellationToken);
         }
      }

      /// <summary>
      /// Converts text to blob content and writes to storage
      /// </summary>
      /// <param name="id">Blob id</param>
      /// <param name="text">Text to write, treated in UTF-8 encoding</param>
      /// <param name="cancellationToken"></param>
      /// <returns></returns>
      public void WriteText(string id, string text)
      {
         G.CallAsync(() => WriteTextAsync(id, text));
      }

      #endregion

      #region [ Singletons ]

      /// <summary>
      /// Deletes a single blob
      /// </summary>
      /// <param name="id"></param>
      /// <param name="cancellationToken"></param>
      /// <returns></returns>
      public Task DeleteAsync(string id, CancellationToken cancellationToken = default(CancellationToken))
      {
         return _provider.DeleteAsync(new[] {id}, cancellationToken);
      }

      #endregion

      #region [ Streaming ]

      /// <summary>
      /// Downloads blob to a stream
      /// </summary>
      /// <param name="id">Blob ID, required</param>
      /// <param name="targetStream">Target stream to copy to, required</param>
      /// <exception cref="System.ArgumentNullException">Thrown when any parameter is null</exception>
      /// <exception cref="System.ArgumentException">Thrown when ID is too long. Long IDs are the ones longer than 50 characters.</exception>
      /// <exception cref="StorageException">Thrown when blob does not exist, error code set to <see cref="ErrorCode.NotFound"/></exception>
      public async Task ReadToStreamAsync(string id, Stream targetStream, CancellationToken cancellationToken = default(CancellationToken))
      {
         if (targetStream == null)
            throw new ArgumentNullException(nameof(targetStream));

         Stream src = await _provider.OpenReadAsync(id, cancellationToken);
         if (src == null) return;

         using (src)
         {
            await src.CopyToAsync(targetStream, BufferSize, cancellationToken);
         }
      }

      #endregion

      #region [ Files ]

      /// <summary>
      /// Downloads a blob to the local filesystem.
      /// </summary>
      /// <param name="id">Blob ID to download</param>
      /// <param name="filePath">Full path to the local file to be downloaded to. If the file exists it will be recreated wtih blob data.</param>
      /// <param name="cancellationToken"></param>
      public async Task ReadToFileAsync(string id, string filePath, CancellationToken cancellationToken = default(CancellationToken))
      {
         Stream src = await _provider.OpenReadAsync(id, cancellationToken);
         if (src == null) return;

         using (src)
         {
            using (Stream dest = File.Create(filePath))
            {
               await src.CopyToAsync(dest, BufferSize, cancellationToken);
               await dest.FlushAsync();
            }
         }
      }

      /// <summary>
      /// Uploads local file to the blob storage
      /// </summary>
      /// <param name="id">Blob ID to create or overwrite</param>
      /// <param name="filePath">Path to local file</param>
      /// <param name="cancellationToken"></param>
      public async Task WriteFileAsync(string id, string filePath, CancellationToken cancellationToken = default(CancellationToken))
      {
         using (Stream src = File.OpenRead(filePath))
         {
            await _provider.WriteAsync(id, src, false, cancellationToken);
         }
      }

      #endregion

      #region [ Uniqueue ]

      /// <summary>
      /// Copies blob to another storage
      /// </summary>
      /// <param name="blobId">Blob ID to copy</param>
      /// <param name="targetStorage">Target storage</param>
      /// <param name="newId">Optional, when specified uses this id in the target storage. If null uses the original ID.</param>
      /// <param name="cancellationToken"></param>
      public async Task CopyToAsync(string blobId, IBlobStorageProvider targetStorage, string newId, CancellationToken cancellationToken = default(CancellationToken))
      {
         Stream src = await _provider.OpenReadAsync(blobId, cancellationToken);
         if (src == null) return;

         using (src)
         {
            await targetStorage.WriteAsync(newId ?? blobId, src, false, cancellationToken);
         }
      }

      #endregion
   }
}
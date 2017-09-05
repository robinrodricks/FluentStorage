using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace Storage.Net.Blob
{
   /// <summary>
   /// Blob storage on steroids. Takes in <see cref="IBlobStorageProvider"/> and adds a lot of extra useful operations on top we as
   /// normal people use every day.
   /// </summary>
   public class BlobStorage
   {
      private readonly IBlobStorageProvider _provider;

      public BlobStorage(IBlobStorageProvider provider)
      {
         this._provider = provider ?? throw new ArgumentNullException(nameof(provider));
      }

      #region [ Text ]

      public async Task<string> ReadTextAsync(string id)
      {
         Stream src = await _provider.OpenReadAsync(id);
         if (src == null) return null;

         var ms = new MemoryStream();
         using (src)
         {
            await src.CopyToAsync(ms);
         }

         return Encoding.UTF8.GetString(ms.ToArray());
      }

      public async Task WriteTextAsync(string id, string text)
      {
         using (Stream s = text.ToMemoryStream())
         {
            await _provider.WriteAsync(id, s);
         }
      }

      public void WriteText(string id, string text)
      {
         G.CallAsync(() => WriteTextAsync(id, text));
      }

      #endregion

      #region [ Singletons ]

      public Task DeleteAsync(string id)
      {
         return _provider.DeleteAsync(new[] {id});
      }

      #endregion

      #region [ Streaming ]

      /// <summary>
      /// Downloads blob to a stream
      /// </summary>
      /// <param name="storage"></param>
      /// <param name="id">Blob ID, required</param>
      /// <param name="targetStream">Target stream to copy to, required</param>
      /// <exception cref="System.ArgumentNullException">Thrown when any parameter is null</exception>
      /// <exception cref="System.ArgumentException">Thrown when ID is too long. Long IDs are the ones longer than 50 characters.</exception>
      /// <exception cref="StorageException">Thrown when blob does not exist, error code set to <see cref="ErrorCode.NotFound"/></exception>
      public async Task ReadToStreamAsync(string id, Stream targetStream)
      {
         if (targetStream == null)
            throw new ArgumentNullException(nameof(targetStream));

         Stream src = await _provider.OpenReadAsync(id);
         if (src == null) return;

         using (src)
         {
            await src.CopyToAsync(targetStream);
         }
      }

      #endregion

      #region [ Files ]

      /// <summary>
      /// Downloads a blob to the local filesystem.
      /// </summary>
      /// <param name="id">Blob ID to download</param>
      /// <param name="filePath">Full path to the local file to be downloaded to. If the file exists it will be recreated wtih blob data.</param>
      public async Task ReadToFileAsync(string id, string filePath)
      {
         Stream src = await _provider.OpenReadAsync(id);
         if (src == null) return;

         using (src)
         {
            using (Stream dest = File.Create(filePath))
            {
               await src.CopyToAsync(dest);
               await dest.FlushAsync();
            }
         }
      }

      /// <summary>
      /// Uploads local file to the blob storage
      /// </summary>
      /// <param name="id">Blob ID to create or overwrite</param>
      /// <param name="filePath">Path to local file</param>
      public async Task WriteFileAsync(string id, string filePath)
      {
         using (Stream src = File.OpenRead(filePath))
         {
            await _provider.WriteAsync(id, src);
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
      public async Task CopyToAsync(string blobId, IBlobStorageProvider targetStorage, string newId)
      {
         Stream src = await _provider.OpenReadAsync(blobId);
         if (src == null) return;

         using (src)
         {
            await targetStorage.WriteAsync(newId ?? blobId, src);
         }
      }

      #endregion

      #region [ Objects ]

      /// <summary>
      /// Downloads blob and tried to deserialize it to an object instance. If the blob doesn't exist or can't be
      /// deserialized returns a default value
      /// </summary>
      /// <typeparam name="T">Object type</typeparam>
      /// <param name="id">Blob ID.</param>
      /// <returns>Deserialized object or null</returns>
      public async Task<T> ReadObjectFromJsonAsync<T>(string id) where T : new()
      {
         string json = await ReadTextAsync(id);
         return json == null ? default(T) : json.AsJsonObject<T>();
      }

      /// <summary>
      /// Uploads object instance as a blob by serializing it
      /// </summary>
      /// <typeparam name="T">Object type</typeparam>
      /// <param name="id">Blob ID</param>
      /// <param name="instance">Object instance. If this parameter is null the blob is deleted if it exists</param>
      public async Task WriteObjectToJsonAsync<T>(string id, T instance) where T : new()
      {
         if (EqualityComparer<T>.Default.Equals(instance, default(T)))
         {
            await DeleteAsync(id);
         }
         else
         {
            await WriteTextAsync(id, instance.ToJsonString());
         }
      }

      #endregion
   }
}
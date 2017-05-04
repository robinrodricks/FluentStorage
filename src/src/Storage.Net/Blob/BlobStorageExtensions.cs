using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace Storage.Net.Blob
{
   /// <summary>
   /// Extension utilities for blog storage
   /// </summary>
   public static class BlobStorageExtensions
   {

      /// <summary>
      /// Downloads blob to a stream
      /// </summary>
      /// <param name="storage"></param>
      /// <param name="id">Blob ID, required</param>
      /// <param name="targetStream">Target stream to copy to, required</param>
      /// <exception cref="System.ArgumentNullException">Thrown when any parameter is null</exception>
      /// <exception cref="System.ArgumentException">Thrown when ID is too long. Long IDs are the ones longer than 50 characters.</exception>
      /// <exception cref="StorageException">Thrown when blob does not exist, error code set to <see cref="ErrorCode.NotFound"/></exception>
      public static void DownloadToStream(this IBlobStorage storage, string id, Stream targetStream)
      {
         if (targetStream == null)
            throw new ArgumentNullException(nameof(targetStream));

         Stream src = storage.OpenStreamToRead(id);
         if (src == null) throw new StorageException(ErrorCode.NotFound, null);

         using (src)
         {
            src.CopyTo(targetStream);
         }
      }

      /// <summary>
      /// Downloads blob to a stream
      /// </summary>
      /// <param name="storage"></param>
      /// <param name="id">Blob ID, required</param>
      /// <param name="targetStream">Target stream to copy to, required</param>
      /// <exception cref="System.ArgumentNullException">Thrown when any parameter is null</exception>
      /// <exception cref="System.ArgumentException">Thrown when ID is too long. Long IDs are the ones longer than 50 characters.</exception>
      /// <exception cref="StorageException">Thrown when blob does not exist, error code set to <see cref="ErrorCode.NotFound"/></exception>
      public static async Task DownloadToStreamAsync(this IBlobStorage storage, string id, Stream targetStream)
      {
         if (targetStream == null)
            throw new ArgumentNullException(nameof(targetStream));

         Stream src = await storage.OpenStreamToReadAsync(id);
         if (src == null) throw new StorageException(ErrorCode.NotFound, null);

         using (src)
         {
            await src.CopyToAsync(targetStream);
         }
      }


      /// <summary>
      /// Downloads a blob to the local filesystem.
      /// </summary>
      /// <param name="storage">Blob storage</param>
      /// <param name="id">Blob ID to download</param>
      /// <param name="filePath">Full path to the local file to be downloaded to. If the file exists it will be recreated wtih blob data.</param>
      public static void DownloadToFile(this IBlobStorage storage, string id, string filePath)
      {
         Stream src = storage.OpenStreamToRead(id);
         if (src == null) throw new StorageException(ErrorCode.NotFound, null);

         using (src)
         {
            using (Stream dest = File.Create(filePath))
            {
               src.CopyTo(dest);
               dest.Flush();
            }
         }
      }

      /// <summary>
      /// Downloads a blob to the local filesystem.
      /// </summary>
      /// <param name="storage">Blob storage</param>
      /// <param name="id">Blob ID to download</param>
      /// <param name="filePath">Full path to the local file to be downloaded to. If the file exists it will be recreated wtih blob data.</param>
      public static async Task DownloadToFileAsync(this IBlobStorage storage, string id, string filePath)
      {
         Stream src = storage.OpenStreamToRead(id);
         if (src == null) throw new StorageException(ErrorCode.NotFound, null);

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
      /// <param name="blobStorage">Blob storage</param>
      /// <param name="id">Blob ID to create or overwrite</param>
      /// <param name="filePath">Path to local file</param>
      public static void UploadFromFile(this IBlobStorage blobStorage, string id, string filePath)
      {
         using (Stream src = File.OpenRead(filePath))
         {
            blobStorage.UploadFromStream(id, src);
         }
      }

      /// <summary>
      /// Uploads local file to the blob storage
      /// </summary>
      /// <param name="blobStorage">Blob storage</param>
      /// <param name="id">Blob ID to create or overwrite</param>
      /// <param name="filePath">Path to local file</param>
      public static async Task UploadFromFileAsync(this IBlobStorage blobStorage, string id, string filePath)
      {
         using (Stream src = File.OpenRead(filePath))
         {
            await blobStorage.UploadFromStreamAsync(id, src);
         }
      }

      /// <summary>
      /// Uploads to blob from a string
      /// </summary>
      /// <param name="blobStorage">Blob storage</param>
      /// <param name="id">Blob ID</param>
      /// <param name="text">Test to upload</param>
      public static void UploadText(this IBlobStorage blobStorage, string id, string text)
      {
         using (Stream s = text.ToMemoryStream())
         {
            blobStorage.UploadFromStream(id, s);
         }
      }

      /// <summary>
      /// Uploads to blob from a string
      /// </summary>
      /// <param name="blobStorage">Blob storage</param>
      /// <param name="id">Blob ID</param>
      /// <param name="text">Test to upload</param>
      public static async Task UploadTextAsync(this IBlobStorage blobStorage, string id, string text)
      {
         using (Stream s = text.ToMemoryStream())
         {
            await blobStorage.UploadFromStreamAsync(id, s);
         }
      }

      /// <summary>
      /// Downloads from blob storage as string
      /// </summary>
      /// <param name="blobStorage">Blob storage</param>
      /// <param name="id">Blob ID</param>
      /// <returns>Text representation of the blob</returns>
      public static string DownloadText(this IBlobStorage blobStorage, string id)
      {
         Stream src = blobStorage.OpenStreamToRead(id);
         if (src == null) throw new StorageException(ErrorCode.NotFound, null);

         var ms = new MemoryStream();
         using (src)
         {
            src.CopyTo(ms);
         }

         return Encoding.UTF8.GetString(ms.ToArray());
      }

      /// <summary>
      /// Downloads from blob storage as string
      /// </summary>
      /// <param name="blobStorage">Blob storage</param>
      /// <param name="id">Blob ID</param>
      /// <returns>Text representation of the blob</returns>
      public static async Task<string> DownloadTextAsync(this IBlobStorage blobStorage, string id)
      {
         Stream src = blobStorage.OpenStreamToRead(id);
         if (src == null) throw new StorageException(ErrorCode.NotFound, null);

         var ms = new MemoryStream();
         using (src)
         {
            await src.CopyToAsync(ms);
         }

         return Encoding.UTF8.GetString(ms.ToArray());
      }

      /// <summary>
      /// Copies blob to another storage
      /// </summary>
      /// <param name="blobStorage">Source storage</param>
      /// <param name="blobId">Blob ID to copy</param>
      /// <param name="targetStorage">Target storage</param>
      /// <param name="newId">Optional, when specified uses this id in the target storage. If null uses the original ID.</param>
      public static void CopyTo(this IBlobStorage blobStorage, string blobId, IBlobStorage targetStorage, string newId)
      {
         Stream src = blobStorage.OpenStreamToRead(blobId);
         if (src == null) throw new StorageException(ErrorCode.NotFound, null);

         using (src)
         {
            targetStorage.UploadFromStream(newId ?? blobId, src);
         }
      }

      /// <summary>
      /// Copies blob to another storage
      /// </summary>
      /// <param name="blobStorage">Source storage</param>
      /// <param name="blobId">Blob ID to copy</param>
      /// <param name="targetStorage">Target storage</param>
      /// <param name="newId">Optional, when specified uses this id in the target storage. If null uses the original ID.</param>
      public static async Task CopyToAsync(this IBlobStorage blobStorage, string blobId, IBlobStorage targetStorage, string newId)
      {
         Stream src = await blobStorage.OpenStreamToReadAsync(blobId);
         if (src == null) throw new StorageException(ErrorCode.NotFound, null);

         using (src)
         {
            await targetStorage.UploadFromStreamAsync(newId ?? blobId, src);
         }
      }

      /// <summary>
      /// Downloads blob and tried to deserialize it to an object instance. If the blob doesn't exist or can't be
      /// deserialized returns a default value
      /// </summary>
      /// <typeparam name="T">Object type</typeparam>
      /// <param name="blobStorage">Storage reference</param>
      /// <param name="id">Blob ID.</param>
      /// <returns>Deserialized object or null</returns>
      public static T Download<T>(this IBlobStorage blobStorage, string id) where T : new()
      {
         string json;

         try
         {
            json = blobStorage.DownloadText(id);
         }
         catch(StorageException ex) when (ex.ErrorCode == ErrorCode.NotFound)
         {
            return default(T);
         }

         return json.AsJsonObject<T>();
      }

      /// <summary>
      /// Downloads blob and tried to deserialize it to an object instance. If the blob doesn't exist or can't be
      /// deserialized returns a default value
      /// </summary>
      /// <typeparam name="T">Object type</typeparam>
      /// <param name="blobStorage">Storage reference</param>
      /// <param name="id">Blob ID.</param>
      /// <returns>Deserialized object or null</returns>
      public async static Task<T> DownloadAsync<T>(this IBlobStorage blobStorage, string id) where T : new()
      {
         string json;

         try
         {
            json = await blobStorage.DownloadTextAsync(id);
         }
         catch (StorageException ex) when (ex.ErrorCode == ErrorCode.NotFound)
         {
            return default(T);
         }

         return json.AsJsonObject<T>();
      }

      /// <summary>
      /// Uploads object instance as a blob by serializing it
      /// </summary>
      /// <typeparam name="T">Object type</typeparam>
      /// <param name="blobStorage">Storage reference</param>
      /// <param name="id">Blob ID</param>
      /// <param name="instance">Object instance. If this parameter is null the blob is deleted if it exists</param>
      public static void Upload<T>(this IBlobStorage blobStorage, string id, T instance) where T : new()
      {
         if(EqualityComparer<T>.Default.Equals(instance, default(T)))
         {
            blobStorage.Delete(id);
         }
         else
         {
            blobStorage.UploadText(id, instance.ToJsonString());
         }
      }

      /// <summary>
      /// Uploads object instance as a blob by serializing it
      /// </summary>
      /// <typeparam name="T">Object type</typeparam>
      /// <param name="blobStorage">Storage reference</param>
      /// <param name="id">Blob ID</param>
      /// <param name="instance">Object instance. If this parameter is null the blob is deleted if it exists</param>
      public static async Task UploadAsync<T>(this IBlobStorage blobStorage, string id, T instance) where T : new()
      {
         if (EqualityComparer<T>.Default.Equals(instance, default(T)))
         {
            await blobStorage.DeleteAsync(id);
         }
         else
         {
            await blobStorage.UploadTextAsync(id, instance.ToJsonString());
         }
      }
   }
}

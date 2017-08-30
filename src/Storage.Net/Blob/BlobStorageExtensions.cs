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
         if (src == null) throw new StorageException(ErrorCode.NotFound, null);

         var ms = new MemoryStream();
         using (src)
         {
            src.CopyTo(ms);
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

      #region [ JSON Transforms ]

      public async Task<T> ReadFromJsonAsync<T>(string id) where T : new()
      {
         throw new NotImplementedException();
      }

      public async Task WriteAsJsonAsync<T>(string id, T instance) where T : new()
      {
         throw new NotImplementedException();
      }

      #endregion

      #region [ Singletons ]

      public Task DeleteAsync(string id)
      {
         return _provider.DeleteAsync(new[] {id});
      }

      #endregion


      /*
      /// <summary>
      /// Returns the list of available blobs
      /// </summary>
      /// <param name="folderPath">Path to the folder. When null works with a root folder.</param>
      /// <param name="prefix">Blob prefix to filter by. When null returns all blobs.
      /// Cannot be longer than 50 characters.</param>
      /// <param name="recurse">When true returns files recursively</param>
      /// <returns>List of blob IDs</returns>
      public static IEnumerable<BlobId> List(this IBlobStorageProvider storage, string folderPath = null, string prefix = null, bool recurse = false)
      {
         return G.CallAsync(() => storage.ListAsync(folderPath, prefix, recurse));
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
      public static void ReadToStream(this IBlobStorageProvider storage, string id, Stream targetStream)
      {
         if (targetStream == null)
            throw new ArgumentNullException(nameof(targetStream));

         Stream src = storage.OpenRead(id);
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
      public static async Task ReadToStreamAsync(this IBlobStorageProvider storage, string id, Stream targetStream)
      {
         if (targetStream == null)
            throw new ArgumentNullException(nameof(targetStream));

         Stream src = await storage.OpenReadAsync(id);
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
      public static void ReadToFile(this IBlobStorageProvider storage, string id, string filePath)
      {
         Stream src = storage.OpenRead(id);
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
      public static async Task ReadToFileAsync(this IBlobStorageProvider storage, string id, string filePath)
      {
         Stream src = await storage.OpenReadAsync(id);
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
      public static void WriteFile(this IBlobStorageProvider blobStorage, string id, string filePath)
      {
         using (Stream src = File.OpenRead(filePath))
         {
            blobStorage.Write(id, src);
         }
      }

      /// <summary>
      /// Uploads local file to the blob storage
      /// </summary>
      /// <param name="blobStorage">Blob storage</param>
      /// <param name="id">Blob ID to create or overwrite</param>
      /// <param name="filePath">Path to local file</param>
      public static async Task WriteFileAsync(this IBlobStorageProvider blobStorage, string id, string filePath)
      {
         using (Stream src = File.OpenRead(filePath))
         {
            await blobStorage.WriteAsync(id, src);
         }
      }

      /// <summary>
      /// Uploads to blob from a string
      /// </summary>
      /// <param name="blobStorage">Blob storage</param>
      /// <param name="id">Blob ID</param>
      /// <param name="text">Test to upload</param>
      public static void WriteText(this IBlobStorageProvider blobStorage, string id, string text)
      {
         using (Stream s = text.ToMemoryStream())
         {
            blobStorage.Write(id, s);
         }
      }

      /// <summary>
      /// Uploads to blob from a string
      /// </summary>
      /// <param name="blobStorage">Blob storage</param>
      /// <param name="id">Blob ID</param>
      /// <param name="text">Test to upload</param>
      public static async Task WriteTextAsync(this IBlobStorageProvider blobStorage, string id, string text)
      {
         using (Stream s = text.ToMemoryStream())
         {
            await blobStorage.WriteAsync(id, s);
         }
      }

      /// <summary>
      /// Downloads from blob storage as string
      /// </summary>
      /// <param name="blobStorage">Blob storage</param>
      /// <param name="id">Blob ID</param>
      /// <returns>Text representation of the blob</returns>
      public static string ReadText(this IBlobStorageProvider blobStorage, string id)
      {
         Stream src = blobStorage.OpenRead(id);
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
      public static async Task<string> ReadTextAsync(this IBlobStorageProvider blobStorage, string id)
      {
         Stream src = await blobStorage.OpenReadAsync(id);
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
      public static void CopyTo(this IBlobStorageProvider blobStorage, string blobId, IBlobStorageProvider targetStorage, string newId)
      {
         Stream src = blobStorage.OpenRead(blobId);
         if (src == null) throw new StorageException(ErrorCode.NotFound, null);

         using (src)
         {
            targetStorage.Write(newId ?? blobId, src);
         }
      }

      /// <summary>
      /// Copies blob to another storage
      /// </summary>
      /// <param name="blobStorage">Source storage</param>
      /// <param name="blobId">Blob ID to copy</param>
      /// <param name="targetStorage">Target storage</param>
      /// <param name="newId">Optional, when specified uses this id in the target storage. If null uses the original ID.</param>
      public static async Task CopyToAsync(this IBlobStorageProvider blobStorage, string blobId, IBlobStorageProvider targetStorage, string newId)
      {
         Stream src = await blobStorage.OpenReadAsync(blobId);
         if (src == null) throw new StorageException(ErrorCode.NotFound, null);

         using (src)
         {
            await targetStorage.WriteAsync(newId ?? blobId, src);
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
      public static T Read<T>(this IBlobStorageProvider blobStorage, string id) where T : new()
      {
         string json;

         try
         {
            json = blobStorage.ReadText(id);
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
      public async static Task<T> ReadAsync<T>(this IBlobStorageProvider blobStorage, string id) where T : new()
      {
         string json;

         try
         {
            json = await blobStorage.ReadTextAsync(id);
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
      public static void Write<T>(this IBlobStorageProvider blobStorage, string id, T instance) where T : new()
      {
         if(EqualityComparer<T>.Default.Equals(instance, default(T)))
         {
            blobStorage.Delete(id);
         }
         else
         {
            blobStorage.WriteText(id, instance.ToJsonString());
         }
      }

      /// <summary>
      /// Uploads object instance as a blob by serializing it
      /// </summary>
      /// <typeparam name="T">Object type</typeparam>
      /// <param name="blobStorage">Storage reference</param>
      /// <param name="id">Blob ID</param>
      /// <param name="instance">Object instance. If this parameter is null the blob is deleted if it exists</param>
      public static async Task WriteAsync<T>(this IBlobStorageProvider blobStorage, string id, T instance) where T : new()
      {
         if (EqualityComparer<T>.Default.Equals(instance, default(T)))
         {
            await blobStorage.DeleteAsync(id);
         }
         else
         {
            await blobStorage.WriteTextAsync(id, instance.ToJsonString());
         }
      }
      */
   }
}

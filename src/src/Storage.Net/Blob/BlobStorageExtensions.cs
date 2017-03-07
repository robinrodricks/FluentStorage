using System;
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
   }
}

using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace Storage.Net.Blob
{
   /// <summary>
   /// Blob storage abstraction that virtualizes sync/async operations and tries to autogenerate the missing ones
   /// </summary>
   public abstract class AsyncBlobStorage : IBlobStorage
   {
      /// <summary>
      /// See interface
      /// </summary>
      public virtual void AppendFromStream(string id, Stream chunkStream)
      {
         CallAsync(() => AppendFromStreamAsync(id, chunkStream));
      }

      /// <summary>
      /// See interface
      /// </summary>
      public virtual Task AppendFromStreamAsync(string id, Stream chunkStream)
      {
         AppendFromStream(id, chunkStream);
         return Task.FromResult(true);
      }

      /// <summary>
      /// See interface
      /// </summary>
      public virtual void Delete(string id)
      {
         CallAsync(() => DeleteAsync(id));
      }

      /// <summary>
      /// See interface
      /// </summary>
      public virtual Task DeleteAsync(string id)
      {
         Delete(id);
         return Task.FromResult(true);
      }

      /// <summary>
      /// See interface
      /// </summary>
      public virtual void Dispose()
      {
      }

      /// <summary>
      /// See interface
      /// </summary>
      public virtual void DownloadToStream(string id, Stream targetStream)
      {
         CallAsync(() => DownloadToStreamAsync(id, targetStream));
      }

      /// <summary>
      /// See interface
      /// </summary>
      public virtual Task DownloadToStreamAsync(string id, Stream targetStream)
      {
         DownloadToStream(id, targetStream);
         return Task.FromResult(true);
      }

      /// <summary>
      /// See interface
      /// </summary>
      public virtual bool Exists(string id)
      {
         return CallAsync(() => ExistsAsync(id));
      }

      /// <summary>
      /// See interface
      /// </summary>
      public virtual Task<bool> ExistsAsync(string id)
      {
         return Task.FromResult(Exists(id));
      }

      /// <summary>
      /// See interface
      /// </summary>
      public virtual BlobMeta GetMeta(string id)
      {
         return CallAsync(() => GetMetaAsync(id));
      }

      /// <summary>
      /// See interface
      /// </summary>
      public virtual Task<BlobMeta> GetMetaAsync(string id)
      {
         return Task.FromResult(GetMeta(id));
      }

      /// <summary>
      /// See interface
      /// </summary>
      public virtual IEnumerable<string> List(string prefix)
      {
         return CallAsync(() => ListAsync(prefix));
      }

      /// <summary>
      /// See interface
      /// </summary>
      public virtual Task<IEnumerable<string>> ListAsync(string prefix)
      {
         return Task.FromResult(List(prefix));
      }

      /// <summary>
      /// See interface
      /// </summary>
      public virtual Stream OpenStreamToRead(string id)
      {
         return CallAsync(() => OpenStreamToReadAsync(id));
      }

      /// <summary>
      /// See interface
      /// </summary>
      public virtual Task<Stream> OpenStreamToReadAsync(string id)
      {
         return Task.FromResult(OpenStreamToRead(id));
      }

      /// <summary>
      /// See interface
      /// </summary>
      public virtual void UploadFromStream(string id, Stream sourceStream)
      {
         CallAsync(() => UploadFromStreamAsync(id, sourceStream));
      }

      /// <summary>
      /// See interface
      /// </summary>
      public virtual Task UploadFromStreamAsync(string id, Stream sourceStream)
      {
         UploadFromStream(id, sourceStream);
         return Task.FromResult(true);
      }

      private void CallAsync(Func<Task> lambda)
      {
         try
         {
            lambda().Wait();
         }
         catch (AggregateException ex)
         {
            throw ex.InnerException;
         }
      }

      private T CallAsync<T>(Func<Task<T>> lambda)
      {
         try
         {
            return lambda().Result;
         }
         catch (AggregateException ex)
         {
            throw ex.InnerException;
         }
      }
   }
}

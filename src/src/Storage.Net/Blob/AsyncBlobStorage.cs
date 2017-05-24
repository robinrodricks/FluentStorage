using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.ExceptionServices;
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
      public virtual Stream OpenRead(string id)
      {
         return CallAsync(() => OpenReadAsync(id));
      }

      /// <summary>
      /// See interface
      /// </summary>
      public virtual Task<Stream> OpenReadAsync(string id)
      {
         return Task.FromResult(OpenRead(id));
      }

      /// <summary>
      /// See interface
      /// </summary>
      public virtual Stream OpenWrite(string id)
      {
         return CallAsync(() => OpenWriteAsync(id));
      }

      /// <summary>
      /// See interface
      /// </summary>
      public virtual Task<Stream> OpenWriteAsync(string id)
      {
         return Task.FromResult(OpenWrite(id));
      }

      /// <summary>
      /// See interface
      /// </summary>
      public virtual Stream OpenAppend(string id)
      {
         return CallAsync(() => OpenAppendAsync(id));
      }

      /// <summary>
      /// See interface
      /// </summary>
      public virtual Task<Stream> OpenAppendAsync(string id)
      {
         return Task.FromResult(OpenAppend(id));
      }

      private void CallAsync(Func<Task> lambda)
      {
         try
         {
            Task.Run(lambda).Wait();
         }
         catch (AggregateException ex)
         {
            ExceptionDispatchInfo.Capture(ex.InnerException).Throw();
            //throw ex.InnerException;
         }
      }

      private T CallAsync<T>(Func<Task<T>> lambda)
      {
         try
         {
            return Task.Run(lambda).Result;
         }
         catch (AggregateException ex)
         {
            ExceptionDispatchInfo.Capture(ex.InnerException).Throw();
            return default(T);
            //throw ex.InnerException;
         }
      }
   }
}

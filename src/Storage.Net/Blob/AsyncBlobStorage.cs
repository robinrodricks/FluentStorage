using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace Storage.Net.Blob
{
   /// <summary>
   /// Blob storage abstraction that virtualizes sync/async operations and tries to autogenerate the missing ones
   /// </summary>
   /// <seealso cref="Storage.Net.Blob.IBlobStorage" />
   public abstract class AsyncBlobStorage : IBlobStorage
   {
      /// <summary>
      /// Deletes a blob by id
      /// </summary>
      /// <param name="id">Blob ID, required.</param>
      public virtual void Delete(string id)
      {
         G.CallAsync(() => DeleteAsync(id));
      }

      /// <summary>
      /// Deletes a blob by id
      /// </summary>
      /// <param name="id">Blob ID, required.</param>
      /// <returns></returns>
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
         return G.CallAsync(() => ExistsAsync(id));
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
         return G.CallAsync(() => GetMetaAsync(id));
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
      public virtual IEnumerable<BlobItem> List(string folderPath, string prefix, bool recurse)
      {
         return G.CallAsync(() => ListAsync(folderPath, prefix, recurse));
      }

      /// <summary>
      /// See interface
      /// </summary>
      public virtual Task<IEnumerable<BlobItem>> ListAsync(string folderPath, string prefix, bool recurse)
      {
         return Task.FromResult(List(folderPath, prefix, recurse));
      }

      /// <summary>
      /// See interface
      /// </summary>
      public virtual Stream OpenRead(string id)
      {
         return G.CallAsync(() => OpenReadAsync(id));
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
      public virtual void Write(string id, Stream sourceStream)
      {
         G.CallAsync(() => WriteAsync(id, sourceStream));
      }

      /// <summary>
      /// See interface
      /// </summary>
      public virtual Task WriteAsync(string id, Stream sourceStream)
      {
         Write(id, sourceStream);
         return Task.FromResult(true);
      }

      /// <summary>
      /// See interface
      /// </summary>
      public virtual void Append(string id, Stream sourceStream)
      {
         G.CallAsync(() => AppendAsync(id, sourceStream));
      }

      /// <summary>
      /// See interface
      /// </summary>
      public virtual Task AppendAsync(string id, Stream sourceStream)
      {
         Append(id, sourceStream);

         return Task.FromResult(true);
      }

   }
}

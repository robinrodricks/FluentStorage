using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Storage.Net.Blob.Universal
{
   /// <summary>
   /// Universal storage provider, in preview.
   /// </summary>
   class UniversalStorage : IBlobStorage
   {
      private readonly ConcurrentDictionary<string, IBlobStorage> _prefixToProvider = new ConcurrentDictionary<string, IBlobStorage>();

      public UniversalStorage()
      {

      }

      public void Register(string prefix, IBlobStorage storage)
      {
         _prefixToProvider[prefix] = storage;
      }

      private IBlobStorage GetProvider(string id, out string nativeId)
      {
         foreach(KeyValuePair<string, IBlobStorage> kvp in _prefixToProvider)
         {
            if(id.StartsWith(kvp.Key))
            {
               nativeId = id.Substring(kvp.Key.Length);
               return kvp.Value;
            }
         }

         nativeId = null;
         return null;
      }

      #region [ IBlobStorage ]

      public Task DeleteAsync(IEnumerable<string> ids, CancellationToken cancellationToken = default(CancellationToken))
      {
         throw new NotImplementedException();
      }

      public void Dispose()
      {
         throw new NotImplementedException();
      }

      public Task<IEnumerable<bool>> ExistsAsync(IEnumerable<string> ids, CancellationToken cancellationToken = default(CancellationToken))
      {
         throw new NotImplementedException();
      }

      public Task<IEnumerable<BlobMeta>> GetMetaAsync(IEnumerable<string> ids, CancellationToken cancellationToken = default(CancellationToken))
      {
         throw new NotImplementedException();
      }

      public Task<IEnumerable<BlobId>> ListAsync(ListOptions options, CancellationToken cancellationToken = default(CancellationToken))
      {

         throw new NotImplementedException();
      }

      public Task<Stream> OpenReadAsync(string id, CancellationToken cancellationToken = default(CancellationToken))
      {
         throw new NotImplementedException();
      }

      public Task<ITransaction> OpenTransactionAsync()
      {
         throw new NotImplementedException();
      }

      public Task WriteAsync(string id, Stream sourceStream, bool append = false, CancellationToken cancellationToken = default(CancellationToken))
      {
         throw new NotImplementedException();
      }

      #endregion
   }
}

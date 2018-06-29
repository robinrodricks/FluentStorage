using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Storage.Net.Blob
{
   class DataTransformingBlobStorage : IBlobStorage
   {
      private readonly IBlobStorage _parentStorage;

      public DataTransformingBlobStorage(IBlobStorage parentStorage)
      {
         _parentStorage = parentStorage ?? throw new ArgumentNullException(nameof(parentStorage));
      }

      public Task DeleteAsync(IEnumerable<string> ids, CancellationToken cancellationToken = default)
      {
         return _parentStorage.DeleteAsync(ids, cancellationToken);
      }

      public Task<IEnumerable<bool>> ExistsAsync(IEnumerable<string> ids, CancellationToken cancellationToken = default)
      {
         return _parentStorage.ExistsAsync(ids, cancellationToken);
      }

      public Task<IEnumerable<BlobMeta>> GetMetaAsync(IEnumerable<string> ids, CancellationToken cancellationToken = default)
      {
         return _parentStorage.GetMetaAsync(ids, cancellationToken);
      }

      public Task<IEnumerable<BlobId>> ListAsync(ListOptions options, CancellationToken cancellationToken = default)
      {
         return _parentStorage.ListAsync(options, cancellationToken);
      }

      public Task<Stream> OpenReadAsync(string id, CancellationToken cancellationToken = default)
      {
         // HERE

         throw new NotImplementedException();
      }

      public Task<ITransaction> OpenTransactionAsync()
      {
         return Task.FromResult(EmptyTransaction.Instance);
      }

      public Task WriteAsync(string id, Stream sourceStream, bool append = false, CancellationToken cancellationToken = default)
      {
         // HERE

         throw new NotImplementedException();
      }

      public void Dispose()
      {
         _parentStorage.Dispose();
      }

   }
}

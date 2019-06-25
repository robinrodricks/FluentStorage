using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Storage.Net.Blobs;

namespace Storage.Net.DataDecorators
{
   class DataDecoratedBlobStorage : IBlobStorage
   {
      private readonly IBlobStorage _parent;
      private readonly IDataDecorator _dataDecorator;

      public DataDecoratedBlobStorage(IBlobStorage parent, IDataDecorator dataDecorator)
      {
         _parent = parent ?? throw new ArgumentNullException(nameof(parent));
         _dataDecorator = dataDecorator ?? throw new ArgumentNullException(nameof(dataDecorator));
      }

      public Task DeleteAsync(IEnumerable<string> fullPaths, CancellationToken cancellationToken = default) =>
         _parent.DeleteAsync(fullPaths, cancellationToken);

      public void Dispose() => _parent.Dispose();

      public Task<IReadOnlyCollection<bool>> ExistsAsync(IEnumerable<string> fullPaths, CancellationToken cancellationToken = default) =>
         _parent.ExistsAsync(fullPaths, cancellationToken);

      public Task<IReadOnlyCollection<Blob>> GetBlobsAsync(IEnumerable<string> fullPaths, CancellationToken cancellationToken = default) =>
         _parent.GetBlobsAsync(fullPaths, cancellationToken);

      public Task<IReadOnlyCollection<Blob>> ListAsync(ListOptions options = null, CancellationToken cancellationToken = default) =>
         _parent.ListAsync(options, cancellationToken);

      public Task SetBlobsAsync(IEnumerable<Blob> blobs, CancellationToken cancellationToken = default) =>
         _parent.SetBlobsAsync(blobs, cancellationToken);

      public Task<ITransaction> OpenTransactionAsync() => Task.FromResult(EmptyTransaction.Instance);

      #region [ Data Handling ]

      public async Task<Stream> OpenReadAsync(string fullPath, CancellationToken cancellationToken = default)
      {
         //open stream from parent
         Stream result = await _parent.OpenReadAsync(fullPath, cancellationToken).ConfigureAwait(false);

         return _dataDecorator.Untransform(result);
      }

      public async Task<Stream> OpenWriteAsync(string fullPath, bool append = false, CancellationToken cancellationToken = default)
      {
         Stream writeStream = await _parent.OpenWriteAsync(fullPath, append, cancellationToken).ConfigureAwait(false);

         return _dataDecorator.Transform(writeStream);
      }

      #endregion
   }
}

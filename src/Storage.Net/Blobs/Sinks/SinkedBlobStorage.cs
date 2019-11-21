using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Storage.Net.Blobs.Sinks
{
   class SinkedBlobStorage : IBlobStorage
   {
      private readonly IBlobStorage _parent;
      private readonly ITransformSink _sink;

      public SinkedBlobStorage(IBlobStorage blobStorage, ITransformSink sink)
      {
         _parent = blobStorage ?? throw new ArgumentNullException(nameof(blobStorage));
         _sink = sink ?? throw new ArgumentNullException(nameof(sink));
      }

      public Task DeleteAsync(IEnumerable<string> fullPaths, CancellationToken cancellationToken = default)
      {
         return _parent.DeleteAsync(fullPaths, cancellationToken);
      }

      public void Dispose() => _parent.Dispose();

      public Task<IReadOnlyCollection<bool>> ExistsAsync(IEnumerable<string> fullPaths, CancellationToken cancellationToken = default)
      {
         return _parent.ExistsAsync(fullPaths, cancellationToken);
      }

      public Task<IReadOnlyCollection<Blob>> GetBlobsAsync(IEnumerable<string> fullPaths, CancellationToken cancellationToken = default)
      {
         return _parent.GetBlobsAsync(fullPaths, cancellationToken);
      }

      public Task<IReadOnlyCollection<Blob>> ListAsync(ListOptions options = null, CancellationToken cancellationToken = default)
      {
         return _parent.ListAsync(options, cancellationToken);
      }

      public Task<ITransaction> OpenTransactionAsync() => _parent.OpenTransactionAsync();

      public Task SetBlobsAsync(IEnumerable<Blob> blobs, CancellationToken cancellationToken = default) => _parent.SetBlobsAsync(blobs, cancellationToken);

      public async Task<Stream> OpenReadAsync(string fullPath, CancellationToken cancellationToken = default)
      {
         Stream original = await _parent.OpenReadAsync(fullPath, cancellationToken).ConfigureAwait(false);
         if(original == null)
            return null;

         return _sink.OpenReadStream(fullPath, original);
      }

      public Task WriteAsync(string fullPath, Stream dataStream, bool append = false, CancellationToken cancellationToken = default)
      {
         using(var ms = new MemoryStream())
         {

            using(Stream wrappedDataStream = _sink.OpenWriteStream(fullPath, ms))
            {
               dataStream.CopyTo(wrappedDataStream);
               ms.Position = 0;

               return _parent.WriteAsync(fullPath, ms, append, cancellationToken);
            }
         }
      }
   }
}

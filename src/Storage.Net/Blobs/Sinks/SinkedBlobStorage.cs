using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Storage.Net.Blobs.Sinks
{
   class SinkedBlobStorage : IBlobStorage
   {
      private readonly IBlobStorage _parent;
      private readonly List<ITransformSink> _sinks;

      public SinkedBlobStorage(IBlobStorage blobStorage, params ITransformSink[] sinks)
      {
         if(sinks is null)
            throw new ArgumentNullException(nameof(sinks));

         _parent = blobStorage ?? throw new ArgumentNullException(nameof(blobStorage));
         _sinks = sinks.ToList();
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
         //chain streams
         Stream readStream = await _parent.OpenReadAsync(fullPath, cancellationToken);

         foreach(ITransformSink sink in _sinks)
         {
            readStream = sink.OpenReadStream(fullPath, readStream);
         }

         return readStream;
      }

      public async Task WriteAsync(string fullPath, Stream dataSourceStream, bool append = false, CancellationToken cancellationToken = default)
      {
         //chain streams
         var bottom = new MemoryStream();
         Stream top = bottom;
         foreach(ITransformSink sink in _sinks)
         {
            top = sink.OpenWriteStream(ref fullPath, top);
         }

         //write using reverse chain
         using(var rev = new ReverseStream(dataSourceStream, bottom, top))
         {
            await _parent.WriteAsync(fullPath, rev, append, cancellationToken).ConfigureAwait(false);
         }
      }
   }
}

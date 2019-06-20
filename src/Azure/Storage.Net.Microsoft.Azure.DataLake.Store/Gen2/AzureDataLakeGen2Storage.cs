using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Storage.Net.Blobs;

namespace Storage.Net.Microsoft.Azure.DataLake.Store.Gen2
{
   //prefer https://www.nuget.org/packages/System.Text.Json for JSON

   class AzureDataLakeGen2Storage : IBlobStorage
   {
      public Task DeleteAsync(IEnumerable<string> fullPaths, CancellationToken cancellationToken = default) => throw new NotImplementedException();
      public void Dispose() => throw new NotImplementedException();
      public Task<IReadOnlyCollection<bool>> ExistsAsync(IEnumerable<string> fullPaths, CancellationToken cancellationToken = default) => throw new NotImplementedException();
      public Task<IReadOnlyCollection<Blob>> GetBlobsAsync(IEnumerable<string> fullPaths, CancellationToken cancellationToken = default) => throw new NotImplementedException();
      public Task<IReadOnlyCollection<Blob>> ListAsync(ListOptions options = null, CancellationToken cancellationToken = default) => throw new NotImplementedException();
      public Task<Stream> OpenReadAsync(string fullPath, CancellationToken cancellationToken = default) => throw new NotImplementedException();
      public Task<ITransaction> OpenTransactionAsync() => throw new NotImplementedException();
      public Task<Stream> OpenWriteAsync(string fullPath, bool append = false, CancellationToken cancellationToken = default) => throw new NotImplementedException();
      public Task SetBlobsAsync(IEnumerable<Blob> blobs, CancellationToken cancellationToken = default) => throw new NotImplementedException();
      public Task WriteAsync(string fullPath, Stream sourceStream, bool append = false, CancellationToken cancellationToken = default) => throw new NotImplementedException();
   }
}

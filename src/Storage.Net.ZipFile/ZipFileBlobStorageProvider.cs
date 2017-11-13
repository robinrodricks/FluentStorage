using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Threading;
using System.Threading.Tasks;
using Storage.Net.Blob;

namespace Storage.Net.ZipFile
{
   class ZipFileBlobStorageProvider : IBlobStorageProvider
   {
      private ZipArchive _file;
      private readonly string _filePath;

      public ZipFileBlobStorageProvider(string filePath)
      {
         _filePath = filePath;
      }

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
   }
}

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;

namespace Storage.Net.Microsoft.Azure.Storage.Blob
{
   class AzureBlockBlobRandomAccessStream : Stream
   {
      private readonly CloudBlockBlob _blob;

      public AzureBlockBlobRandomAccessStream(CloudBlockBlob blob)
      {
         _blob = blob;
      }

      public override bool CanRead => true;

      public override bool CanSeek => true;

      public override bool CanWrite => false;

      public override long Length => _blob.Properties.Length;

      public override long Position { get; set; }

      public override void Flush() { }

      public override int Read(byte[] buffer, int offset, int count)
      {
         return ReadAsync(buffer, offset, count).Result;
      }

      public override async Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
      {
         //see https://docs.microsoft.com/en-us/rest/api/storageservices/specifying-the-range-header-for-blob-service-operations#range-header-formats

         int read = await _blob.DownloadRangeToByteArrayAsync(buffer, offset, Position, count);

         Position += read;

         return read;
      }

      public override long Seek(long offset, SeekOrigin origin)
      {
         switch(origin)
         {
            case SeekOrigin.Begin:
               Position = offset;
               break;
            case SeekOrigin.End:
               Position = Length - offset;
               break;
            case SeekOrigin.Current:
               Position += offset;
               break;
         }

         if (Position < 0) Position = 0;
         if (Position + 1 >= Length) Position = Length - 1;

         return Position;
      }

      public override void SetLength(long value) => throw new NotSupportedException();
      public override void Write(byte[] buffer, int offset, int count) => throw new NotSupportedException();
   }
}

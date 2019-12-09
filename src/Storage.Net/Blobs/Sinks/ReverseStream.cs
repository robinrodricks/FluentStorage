using System;
using System.IO;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;

namespace Storage.Net.Blobs.Sinks
{
   class ReverseStream : Stream
   {
      private readonly Stream _sourceData;
      private readonly MemoryStream _bottom;
      private readonly Stream _top;

      public ReverseStream(
         Stream sourceDataStream,
         MemoryStream bottom,
         Stream top)
      {
         _sourceData = sourceDataStream;
         _bottom = bottom;
         _top = top;
      }

      public override bool CanRead => true;

      public override bool CanSeek => false;

      public override bool CanWrite => false;

      public override long Length => throw new NotImplementedException();

      public override long Position { get; set; }

      public override void Flush()
      {

      }

      public override async Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
      {
         //read a chunk from the original stream
         int read = await _sourceData.ReadAsync(buffer, offset, count).ConfigureAwait(false);

         if(read == 0)
         {
            return 0;
         }

         //write this chunk to the top stream
         _bottom.SetLength(0);   //clear
         await _top.WriteAsync(buffer, 0, read).ConfigureAwait(false);
         await _top.FlushAsync().ConfigureAwait(false);

#if !NET16
         if(_top is CryptoStream cst)
         {
            cst.FlushFinalBlock();
         }
#endif

         //get the raw data
         byte[] raw = _bottom.ToArray();
         Array.Copy(raw, 0, buffer, offset, raw.Length);

         return raw.Length;
      }

      public override int Read(byte[] buffer, int offset, int count)
      {
         return ReadAsync(buffer, offset, count).Result;

      }

      public override long Seek(long offset, SeekOrigin origin)
      {
         throw new NotSupportedException();
      }

      public override void SetLength(long value)
      {
         throw new NotSupportedException();
      }

      public override void Write(byte[] buffer, int offset, int count)
      {
         throw new NotSupportedException();
      }
   }
}

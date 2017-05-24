using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Storage.Net.Blob
{
   public class ReaderWriterStream : Stream
   {
      public ReaderWriterStream()
      {

      }

      public static Stream Pump(Func<Stream, Task> fn)
      {
         var s = new ReaderWriterStream();

         Task.Factory.StartNew(async () =>
         {
            await fn(s);
         });

         return s;
      }

      private readonly Queue<byte> _buffer = new Queue<byte>();
      private readonly ManualResetEventSlim _availEvent = new ManualResetEventSlim(false);

      public override bool CanRead => true;

      public override bool CanSeek => false;

      public override bool CanWrite => true;

      public override long Length => throw new NotSupportedException();

      public override long Position
      {
         get => throw new NotSupportedException();
         set => throw new NotSupportedException();
      }

      public override int Read(byte[] buffer, int offset, int count)
      {
         //read as much as I can

         if (_buffer.Count == 0)
         {
            _availEvent.Wait();
         }

         if (_buffer.Count == 0) return 0;

         int maxCount = Math.Min(count, _buffer.Count);
         byte[] b = new byte[maxCount];
         for (int i = 0; i < maxCount; i++)
         {
            b[i] = _buffer.Dequeue();
         }

         return maxCount;
      }

      public override void Write(byte[] buffer, int offset, int count)
      {
         //push to queue
         for(int i = offset; i < offset + count; i++)
         {
            _buffer.Enqueue(buffer[i]);
         }
      }

      protected override void Dispose(bool disposing)
      {
         base.Dispose(disposing);
      }

      public override void Flush()
      {
      }

      public override long Seek(long offset, SeekOrigin origin)
      {
         throw new NotSupportedException();
      }

      public override void SetLength(long value)
      {
         throw new NotSupportedException();
      }


   }
}

using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Storage.Net.Blob
{
   public class ReaderWriterStream : Stream
   {
      private ReaderWriterStream()
      {

      }

      public static Stream Create(Func<Stream, Task> fn)
      {
         var s = new ReaderWriterStream();

         Task.Factory.StartNew(async () =>
         {
            try
            {
               await fn(s);
            }
            catch (Exception ex)
            {
               //todo: this has to be thrown back to _writer_
               Console.WriteLine(ex);
            }
            finally
            {
               //that's when the reader is done
               s.MarkFinished();
            }
         });

         return s;
      }

      internal void MarkFinished()
      {
         _readerFinishedEvent.Set();
      }

      private readonly Queue<byte> _buffer = new Queue<byte>();
      private readonly ManualResetEventSlim _moreDataEvent = new ManualResetEventSlim(false);
      private readonly ManualResetEventSlim _readerFinishedEvent = new ManualResetEventSlim(false);
      private bool _hasMoreWrites = true;

      public override int Read(byte[] buffer, int offset, int count)
      {
         //read as much as I can

         while(_buffer.Count == 0)
         {
            if (!_hasMoreWrites) return 0;

            _moreDataEvent.Wait(TimeSpan.FromSeconds(1));
            _moreDataEvent.Reset();
         }

         if (_buffer.Count == 0) return 0;

         int maxCount = Math.Min(count, _buffer.Count);
         byte[] b = new byte[maxCount];
         for (int i = 0; i < maxCount; i++)
         {
            b[i] = _buffer.Dequeue();
         }
         Array.Copy(b, 0, buffer, offset, maxCount);

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
         //called by a writer stream
         _hasMoreWrites = false;

         _readerFinishedEvent.Wait();
      }

      public override void Flush()
      {
      }

      #region [ Unused ]

      public override bool CanRead => true;

      public override bool CanSeek => false;

      public override bool CanWrite => true;

      public override long Length => throw new NotSupportedException();

      public override long Position
      {
         get => throw new NotSupportedException();
         set => throw new NotSupportedException();
      }

      public override long Seek(long offset, SeekOrigin origin)
      {
         throw new NotSupportedException();
      }

      public override void SetLength(long value)
      {
         throw new NotSupportedException();
      }

      #endregion
   }
}
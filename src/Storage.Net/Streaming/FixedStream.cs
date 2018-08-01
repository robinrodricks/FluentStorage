using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Storage.Net.Streaming
{
   /// <summary>
   /// Fixes deficiencies in thrird party streaming impelmentations
   /// </summary>
   public class FixedStream : Stream
   {
      private readonly Stream _parent;
      private readonly long? _length;
      private long _position;

      public FixedStream(Stream parent, long? length = null)
      {
         _parent = parent ?? throw new ArgumentNullException(nameof(parent));
         _length = length;
      }

      public override bool CanRead => _parent.CanRead;

      public override bool CanSeek => _parent.CanSeek;

      public override bool CanWrite => _parent.CanWrite;

      public override long Length => _length == null ? _parent.Length : _length.Value;

      public override long Position
      {
         get => _parent.Position;
         set
         {
            _parent.Position = value;
         }
      }

      public override void Flush()
      {
         _parent.Flush();
      }

      public override int Read(byte[] buffer, int offset, int count)
      {
         int read = _parent.Read(buffer, offset, count);
         _position += read;
         return read;
      }

      public override long Seek(long offset, SeekOrigin origin)
      {
         return (_position = _parent.Seek(offset, origin));
      }

      public override void SetLength(long value)
      {
         _parent.SetLength(value);
      }

      public override void Write(byte[] buffer, int offset, int count)
      {
         _parent.Write(buffer, offset, count);

         _position += count;
      }
   }
}

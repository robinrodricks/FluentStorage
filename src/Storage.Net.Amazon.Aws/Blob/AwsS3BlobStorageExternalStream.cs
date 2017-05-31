using System.IO;
using Amazon.S3.Model;
using System;

namespace Storage.Net.Aws.Blob
{
   class AwsS3BlobStorageExternalStream : Stream
   {
      private readonly GetObjectResponse _response;
      private readonly Stream _stream;

      //position hack: when streaming files from and into AWS the SDK requests position which source stream doesn't support,
      //therefore we're implementing it here
      private long _position;

      public AwsS3BlobStorageExternalStream(GetObjectResponse response)
      {
         _response = response;
         _stream = _response.ResponseStream;
         _position = 0;
      }

      public override void Flush()
      {
         _stream.Flush();
      }

      public override long Seek(long offset, SeekOrigin origin)
      {
         throw new NotSupportedException();
      }

      public override void SetLength(long value)
      {
         throw new NotSupportedException();
      }

      public override int Read(byte[] buffer, int offset, int count)
      {
         int read = _stream.Read(buffer, offset, count);
         _position += read;
         return read;
      }

      public override void Write(byte[] buffer, int offset, int count)
      {
         throw new NotSupportedException();
      }

      public override bool CanRead => true;
      public override bool CanSeek => false;
      public override bool CanWrite => false;
      public override long Length => _stream.Length;
      public override long Position
      {
         get { return _position; }
         set { throw new NotSupportedException(); }
      }

      public override void Close()
      {
         _stream.Close();
      }

      protected override void Dispose(bool disposing)
      {
         _response.Dispose();

         base.Dispose(disposing);
      }
   }
}

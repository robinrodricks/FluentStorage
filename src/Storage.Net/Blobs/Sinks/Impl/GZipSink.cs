using System.IO;
using System.IO.Compression;

namespace Storage.Net.Blobs.Sinks.Impl
{
   class GZipSink : ITransformSink
   {
      private readonly CompressionLevel _compressionLevel;

      public GZipSink(CompressionLevel compressionLevel)
      {
         _compressionLevel = compressionLevel;
      }

      public Stream OpenReadStream(string fullPath, Stream parentStream)
      {
         return new GZipStream(parentStream, CompressionMode.Decompress, false);
      }

      public Stream OpenWriteStream(string fullPath, Stream parentStream)
      {
         return new GZipStream(parentStream, _compressionLevel, false);
      }
   }
}

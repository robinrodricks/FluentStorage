using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Text;

namespace Storage.Net.DataDecorators
{
   class GzipDecorator : IDataDecorator
   {
      private readonly CompressionLevel _compressionLevel;

      public GzipDecorator(CompressionLevel compressionLevel)
      {
         _compressionLevel = compressionLevel;
      }

      public Stream DecorateReader(Stream parentStream)
      {
         return new GZipStream(parentStream, CompressionMode.Decompress, false);
      }

      public Stream DecorateWriter(Stream parentStream)
      {
         return new GZipStream(parentStream, _compressionLevel);
      }
   }
}

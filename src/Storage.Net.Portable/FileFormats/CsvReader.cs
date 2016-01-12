using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Storage.Net.FileFormats
{
   /// <summary>
   /// The most advanced CSV reader
   /// </summary>
   public class CsvReader
   {
      private readonly StreamReader _reader;

      /// <summary>
      /// Creates an instance from readable stream and text encoding
      /// </summary>
      public CsvReader(Stream stream, Encoding encoding)
      {
         _reader = new StreamReader(stream, encoding);
      }

      /// <summary>
      /// Reads the next row
      /// </summary>
      /// <returns></returns>
      public IEnumerable<string> ReadNextRow()
      {
         string line = _reader.ReadLine();
         if(line == null) return null;

         string[] parts = line.Split(CsvFormat.ColumnSeparator);

         return parts.Select(CsvFormat.UnescapeValue);
      }
   }
}

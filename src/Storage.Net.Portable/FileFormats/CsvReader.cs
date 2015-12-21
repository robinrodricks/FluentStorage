using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Storage.Net.FileFormats
{
   public class CsvReader
   {
      private readonly StreamReader _reader;

      public CsvReader(Stream stream, Encoding encoding)
      {
         _reader = new StreamReader(stream, encoding);
      }

      public IEnumerable<string> ReadNextRow()
      {
         string line = _reader.ReadLine();
         if(line == null) return null;

         string[] parts = line.Split(CsvFormat.ColumnSeparator);

         return parts.Select(CsvFormat.UnescapeValue);
      }
   }
}

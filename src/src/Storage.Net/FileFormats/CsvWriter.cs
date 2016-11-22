using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Storage.Net.FileFormats
{
   /// <summary>
   /// CSV format writer
   /// </summary>
   public class CsvWriter
   {
      private readonly Stream _destination;
      private readonly Encoding _encoding;
      private readonly byte[] _newLine;
      private readonly byte[] _separator;
      private bool _firstRowWritten;

      /// <summary>
      /// Creates a new instance of CsvWriter which uses UTF8 encoding
      /// </summary>
      /// <param name="destination"></param>
      public CsvWriter(Stream destination)
         : this(destination, Encoding.UTF8)
      {

      }

      /// <summary>
      /// Creates a new instance of CsvWriter and allows to specify the writer encoding
      /// </summary>
      /// <param name="destination"></param>
      /// <param name="encoding"></param>
      public CsvWriter(Stream destination, Encoding encoding)
      {
         if(destination == null) throw new ArgumentNullException("destination");
         if(encoding == null) throw new ArgumentNullException("encoding");
         if(!destination.CanWrite) throw new ArgumentException("must be writeable", "destination");

         _destination = destination;
         _encoding = encoding;
         _separator = encoding.GetBytes(CsvFormat.ColumnSeparator);
         _newLine = _encoding.GetBytes(CsvFormat.NewLine);
      }

      /// <summary>
      /// Writes the values as a CSV row
      /// </summary>
      public void Write(params string[] values)
      {
         Write((IEnumerable<string>)values);
      }

      /// <summary>
      /// Writes the values as a CSV row
      /// </summary>
      public void Write(IEnumerable<string> values)
      {
         if(values == null) return;

         if(_firstRowWritten) _destination.Write(_newLine, 0, _newLine.Length);

         int i = 0;
         foreach(string column in values)
         {
            if(i != 0) _destination.Write(_separator, 0, _separator.Length);

            byte[] escaped = _encoding.GetBytes(CsvFormat.EscapeValue(column));
            _destination.Write(escaped, 0, escaped.Length);
            i++;
         }

         _firstRowWritten = true;
      }
   }
}

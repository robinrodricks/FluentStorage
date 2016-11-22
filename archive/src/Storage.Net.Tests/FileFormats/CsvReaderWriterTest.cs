using System.IO;
using System.Linq;
using System.Text;
using Xunit;
using Storage.Net.FileFormats;

namespace Storage.Net.Tests.FileFormats
{
   public class CsvReaderWriterTest
   {
      private CsvWriter _writer;
      private CsvReader _reader;
      private MemoryStream _ms;

      public CsvReaderWriterTest()
      {
         _ms = new MemoryStream();
         _writer = new CsvWriter(_ms, Encoding.UTF8);
         _reader = new CsvReader(_ms, Encoding.UTF8);
      }

      [Fact]
      public void Write_2RowsOfDifferentSize_Succeeds()
      {
         _writer.Write("11", "12");
         _writer.Write("21", "22", "23");
      }

      [Fact]
      public void Write_2RowsOfSameSize_Succeeds()
      {
         _writer.Write("11", "12");
         _writer.Write("21", "22");
      }

      [Fact]
      public void Write_NoEscaping_JustQuotes()
      {
         _writer.Write("1", "-=--=,,**\r\n77$$");

         string result = Encoding.UTF8.GetString(_ms.ToArray());

         Assert.Equal("\"1\",\"-=--=,,**\r\n77$$\"", result);
      }

      [Fact]
      public void Write_WithEscaping_EscapingAndQuoting()
      {
         _writer.Write("1", "two of \"these\"");

         string result = Encoding.UTF8.GetString(_ms.ToArray());

         Assert.Equal("\"1\",\"two of \"\"these\"\"\"", result);

      }

      [Fact]
      public void WriteRead_WriteTwoRows_ReadsTwoRows()
      {
         _writer.Write("r1c1", "r1c2", "r1c3");
         _writer.Write("r2c1", "r2c2");

         _ms.Flush();
         _ms.Position = 0;

         _reader = new CsvReader(_ms, Encoding.UTF8);
         string[] r1 = _reader.ReadNextRow().ToArray();
         string[] r2 = _reader.ReadNextRow().ToArray();
         var r3 = _reader.ReadNextRow();

         Assert.Null(r3);
         Assert.Equal(2, r2.Length);
         Assert.Equal(3, r1.Length);

         Assert.Equal("r2c1", r2[0]);
      }
   }
}

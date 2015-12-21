using System.IO;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Storage.Net.FileFormats;

namespace Storage.Net.Tests.FileFormats
{
   [TestFixture]
   public class CsvReaderWriterTest
   {
      private CsvWriter _writer;
      private CsvReader _reader;
      private MemoryStream _ms;

      [SetUp]
      public void SetUp()
      {
         _ms = new MemoryStream();
         _writer = new CsvWriter(_ms, Encoding.UTF8);
         _reader = new CsvReader(_ms, Encoding.UTF8);
      }

      [Test]
      public void Write_2RowsOfDifferentSize_Succeeds()
      {
         _writer.Write("11", "12");
         _writer.Write("21", "22", "23");
      }

      [Test]
      public void Write_2RowsOfSameSize_Succeeds()
      {
         _writer.Write("11", "12");
         _writer.Write("21", "22");
      }

      [Test]
      public void Write_NoEscaping_JustQuotes()
      {
         _writer.Write("1", "-=--=,,**\r\n77$$");

         string result = Encoding.UTF8.GetString(_ms.ToArray());

         Assert.AreEqual("\"1\",\"-=--=,,**\r\n77$$\"", result);
      }

      [Test]
      public void Write_WithEscaping_EscapingAndQuoting()
      {
         _writer.Write("1", "two of \"these\"");

         string result = Encoding.UTF8.GetString(_ms.ToArray());

         Assert.AreEqual("\"1\",\"two of \"\"these\"\"\"", result);

      }

      [Test]
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

         Assert.IsNull(r3);
         Assert.AreEqual(2, r2.Length);
         Assert.AreEqual(3, r1.Length);

         Assert.AreEqual("r2c1", r2[0]);
      }
   }
}

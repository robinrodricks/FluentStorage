using System.IO;
using Storage.Net.Blob;
using Storage.Net.Blob.Files;
using Storage.Net.Table;
using Storage.Net.Table.Files;

namespace Storage.Net
{
   public static class Factory
   {
      public static ITableStorage CsvFiles(this ITableStorageFactory factory,
         DirectoryInfo rootDir)
      {
         return new CsvFileTableStorage(rootDir);
      }

      public static IBlobStorage DirectoryFiles(this IBlobStorageFactory factory,
         DirectoryInfo directory)
      {
         return new DirectoryFilesBlobStorage(directory);
      }
   }
}

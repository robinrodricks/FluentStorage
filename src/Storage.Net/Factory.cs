using System.IO;
using Storage.Net.Blob;
using Storage.Net.Blob.Files;
using Storage.Net.Messaging;
using Storage.Net.Table;
using Storage.Net.Table.Files;

namespace Storage.Net
{
   /// <summary>
   /// Factory extension methods
   /// </summary>
   public static class Factory
   {
      /// <summary>
      /// Creates a new instance of CSV file storage
      /// </summary>
      /// <param name="factory"></param>
      /// <param name="rootDir"></param>
      public static ITableStorage CsvFiles(this ITableStorageFactory factory,
         DirectoryInfo rootDir)
      {
         return new CsvFileTableStorage(rootDir);
      }

      /// <summary>
      /// Creates an instance in a specific disk directory
      /// <param name="factory"></param>
      /// <param name="directory">Root directory</param>
      /// </summary>
      public static IBlobStorageProvider DirectoryFiles(this IBlobStorageFactory factory,
         DirectoryInfo directory)
      {
         return new DiskDirectoryBlobStorageProvider(directory);
      }

      /// <summary>
      /// Creates an instance of blob storage which stores everyting in memory. Useful for testing purposes only or if blobs don't
      /// take much space.
      /// </summary>
      /// <param name="factory">Factory reference</param>
      /// <returns>In-memory blob storage instance</returns>
      public static IBlobStorageProvider InMemory(this IBlobStorageFactory factory)
      {
         return new InMemoryBlobStorageProvider();
      }
   }
}

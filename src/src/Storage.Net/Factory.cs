using System.IO;
using Storage.Net.Blob;
using Storage.Net.Blob.Files;
using Storage.Net.Messaging;
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

      /// <summary>
      /// Creates a pair of inmemory publisher and receiver using the same block of memory
      /// </summary>
      /// <param name="receiver">Receiver</param>
      /// <returns>Publisher</returns>
      public static IMessagePublisher InMemory(this IMessagingFactory factory, out IMessageReceiver receiver)
      {
         var inmem = new InMemoryMessagePublisherReceiver();
         receiver = inmem;
         return inmem;
      }
   }
}

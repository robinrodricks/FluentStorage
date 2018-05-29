using System;
using System.Collections.Generic;
using System.IO;
using Storage.Net.Blob;
using Storage.Net.Blob.Files;
using Storage.Net.Messaging;
using Storage.Net.Table;
using Storage.Net.Table.Files;
using NetBox.Extensions;

namespace Storage.Net
{
   /// <summary>
   /// Factory extension methods
   /// </summary>
   public static class Factory
   {
      private static readonly Dictionary<string, InMemoryMessagePublisherReceiver> _inMemoryMessagingNameToInstance =
         new Dictionary<string, InMemoryMessagePublisherReceiver>();

      /// <summary>
      /// Creates a new instance of CSV file storage
      /// </summary>
      /// <param name="factory"></param>
      /// <param name="rootDir"></param>
      public static ITableStorage CsvFiles(this ITableStorageFactory factory,
         DirectoryInfo rootDir)
      {
         return new CsvFileTableStorageProvider(rootDir);
      }

      /// <summary>
      /// Creates an instance in a specific disk directory
      /// <param name="factory"></param>
      /// <param name="directory">Root directory</param>
      /// </summary>
      public static IBlobStorage DirectoryFiles(this IBlobStorageFactory factory,
         DirectoryInfo directory)
      {
         return new DiskDirectoryBlobStorage(directory);
      }

      /// <summary>
      /// Creates an instance of blob storage which stores everyting in memory. Useful for testing purposes only or if blobs don't
      /// take much space.
      /// </summary>
      /// <param name="factory">Factory reference</param>
      /// <returns>In-memory blob storage instance</returns>
      public static IBlobStorage InMemory(this IBlobStorageFactory factory)
      {
         return new InMemoryBlobStorage();
      }

      /// <summary>
      /// Creates a message publisher which holds messages in memory.
      /// </summary>
      /// <param name="factory"></param>
      /// <param name="name">Memory buffer name. Publishers with the same name will contain identical messages. Querying a publisher again
      /// with the same name returns an identical publisher. To create a receiver for this memory bufffer use the same name.</param>
      public static IMessagePublisher InMemoryPublisher(this IMessagingFactory factory, string name)
      {
         if (name == null) throw new ArgumentNullException(nameof(name));

         return _inMemoryMessagingNameToInstance.GetOrAdd(name, () => new InMemoryMessagePublisherReceiver());
      }

      /// <summary>
      /// Creates a message receiver to receive messages from a specified memory buffer.
      /// </summary>
      /// <param name="factory"></param>
      /// <param name="name">Memory buffer name. Use the name used when you've created a publisher to receive messages from that buffer.</param>
      public static IMessageReceiver InMemoryReceiver(this IMessagingFactory factory, string name)
      {
         if (name == null) throw new ArgumentNullException(nameof(name));

         return _inMemoryMessagingNameToInstance.GetOrAdd(name, () => new InMemoryMessagePublisherReceiver());
      }
   }
}

namespace Storage.Net
{
   /// <summary>
   /// Helper syntax for creating instances of storage library objects
   /// </summary>
   public static class StorageFactory
   {
      private static ITableStorageFactory _tables = new InternalTablesFactory();
      private static IBlobStorageFactory _blobs = new InternalBlobsFactory();
      private static IMessagingFactory _messages = new InternalMessagingFactory();

      /// <summary>
      /// Access to creating tables
      /// </summary>
      public static ITableStorageFactory Tables => _tables;

      /// <summary>
      /// Access to creating blobs
      /// </summary>
      public static IBlobStorageFactory Blobs => _blobs;

      /// <summary>
      /// Access to creating messaging
      /// </summary>
      public static IMessagingFactory Messages => _messages;

      class InternalTablesFactory : ITableStorageFactory
      {
      }

      class InternalBlobsFactory : IBlobStorageFactory
      {
      }

      class InternalMessagingFactory : IMessagingFactory
      {

      }
   }

}
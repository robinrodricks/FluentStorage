namespace Storage.Net
{
   public static class StorageFactory
   {
      private static ITableStorageFactory _tables = new InternalTablesFactory();
      private static IBlobStorageFactory _blobs = new InternalBlobsFactory();

      public static ITableStorageFactory Tables => _tables;

      public static IBlobStorageFactory Blobs => _blobs;

      class InternalTablesFactory : ITableStorageFactory
      {
      }

      class InternalBlobsFactory : IBlobStorageFactory
      {
      }

   }

}
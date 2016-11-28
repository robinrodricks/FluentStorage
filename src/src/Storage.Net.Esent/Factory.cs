using Storage.Net.Net45.Esent;
using Storage.Net.Table;

namespace Storage.Net
{
   public static class Factory
   {
      public static ITableStorage Esent(this ITableStorageFactory factor,
         string databasePath)
      {
         return new EsentTableStorage(databasePath);
      }
   }
}

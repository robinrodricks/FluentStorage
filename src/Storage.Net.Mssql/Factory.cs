using Storage.Net.Mssql;
using Storage.Net.Table;

namespace Storage.Net
{
   public static class Factory
   {
      public static ITableStorageProvider MssqlServer(this ITableStorageFactory factory, string connectionString, SqlConfiguration config = null)
      {
         return new MssqlTableStorageProvider(connectionString, config);
      }
   }
}

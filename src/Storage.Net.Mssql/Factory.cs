using Storage.Net.Mssql;
using Storage.Net.Table;

namespace Storage.Net
{
   public static class Factory
   {
      public static ITableStorage MssqlServer(this ITableStorageFactory factory, string connectionString)
      {
         return new MssqlTableStorage(connectionString);
      }
   }
}

using System;
using Storage.Net.Messaging;
using Storage.Net.Mssql;
using Storage.Net.Table;

namespace Storage.Net
{
   public static class Factory
   {
      /// <summary>
      /// Creates Microsoft SQL Server table provider.
      /// </summary>
      /// <param name="factory"></param>
      /// <param name="connectionString">Full connection string to the server.</param>
      /// <param name="config">Optional configuration</param>
      /// <returns></returns>
      public static ITableStorage MssqlServer(this ITableStorageFactory factory, string connectionString,
         SqlConfiguration config = null)
      {
         return new MssqlTableStorageProvider(connectionString, config);
      }

      public static IMessagePublisher MssqlServerPublisher(this IMessagingFactory factory,
         string connectionString,
         string tableName)
      {
         throw new NotImplementedException();
      }
   }
}

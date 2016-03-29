using System.Data.SQLite;
using System.IO;

namespace Storage.Net.Application
{
   class SqliteDriver
   {
      private const string MainDatabaseName = "main.db";

      private readonly SQLiteConnection _conn;


      public SqliteDriver(DirectoryInfo dataDirectory)
      {
         if (!dataDirectory.Exists) dataDirectory.Create();

         string connectionString = $"Data Source={dataDirectory.FullName}\\{MainDatabaseName}";

         _conn = new SQLiteConnection(connectionString);
      }

      public void EnsureTable(string tableName, params string[] columnNames)
      {
         
      }
   }
}

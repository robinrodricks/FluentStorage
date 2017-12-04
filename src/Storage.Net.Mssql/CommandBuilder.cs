using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Text;
using Storage.Net.Table;

namespace Storage.Net.Mssql
{
   class CommandBuilder
   {
      private const string PartitionKey = "PK";
      private const string RowKey = "RK";
      private readonly SqlConnection _sqlConnection;

      public CommandBuilder(SqlConnection sqlConnection)
      {
         _sqlConnection = sqlConnection;
      }

      public SqlCommand BuidInsertRowCommand(string tableName, TableRow row)
      {
         var s = new StringBuilder();
         s.Append("INSERT INTO [");
         s.Append(tableName);
         s.Append("] (");
         s.Append(PartitionKey);
         s.Append(", ");
         s.Append(RowKey);

         s.Append(") values (@pk, @rk");
         s.Append(")");

         SqlCommand cmd = _sqlConnection.CreateCommand();
         cmd.CommandText = s.ToString();
         cmd.Parameters.AddWithValue("@pk", row.PartitionKey);
         cmd.Parameters.AddWithValue("@rk", row.RowKey);

         return cmd;
      }

      public SqlCommand BuildCreateSchemaCommand(string tableName, TableRow row)
      {
         var s = new StringBuilder();
         s.Append("CREATE TABLE [");
         s.Append(tableName);
         s.Append("] ([");
         s.Append(PartitionKey);
         s.Append("] NVARCHAR(50) NOT NULL, [");
         s.Append(RowKey);
         s.Append("] NVARCHAR(50) NOT NULL, ");

         s.Append($"PRIMARY KEY ([{PartitionKey}], [{RowKey}])");
         s.Append(")");

         SqlCommand cmd = _sqlConnection.CreateCommand();
         cmd.CommandText = s.ToString();
         return cmd;
      }
   }
}

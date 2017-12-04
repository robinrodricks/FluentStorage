using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Text;
using NetBox;
using Storage.Net.Table;

namespace Storage.Net.Mssql
{
   class CommandBuilder
   {
      private readonly SqlConnection _sqlConnection;
      private readonly SqlConfiguration _config;

      public CommandBuilder(SqlConnection sqlConnection, SqlConfiguration config)
      {
         _sqlConnection = sqlConnection;
         _config = config;
      }

      public SqlCommand BuidInsertRowCommand(string tableName, TableRow row)
      {
         var s = new StringBuilder();
         s.Append("INSERT INTO [");
         s.Append(tableName);
         s.Append("] (");
         s.Append(_config.PartitionKeyColumnName);
         s.Append(", ");
         s.Append(_config.RowKeyColumnName);

         foreach(KeyValuePair<string, DynamicValue> cell in row)
         {
            s.Append(", [");
            s.Append(cell.Key);
            s.Append("]");
         }

         s.Append(") values (@pk, @rk");
         for(int i = 0; i < row.Count; i++)
         {
            s.Append(", @c");
            s.Append(i);
         }

         s.Append(")");

         SqlCommand cmd = _sqlConnection.CreateCommand();
         cmd.CommandText = s.ToString();
         cmd.Parameters.AddWithValue("@pk", row.PartitionKey);
         cmd.Parameters.AddWithValue("@rk", row.RowKey);

         int c = 0;
         foreach(KeyValuePair<string, DynamicValue> cell in row)
         {
            string pn = $"@c{c++}";
            cmd.Parameters.AddWithValue(pn, cell.Value.OriginalValue);
         }

         return cmd;
      }

   }
}

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
      private const string PartitionKey = "PK";
      private const string RowKey = "RK";
      private readonly SqlConnection _sqlConnection;

      private static readonly Dictionary<Type, string> TypeToSqlTypeName = new Dictionary<Type, string>
      {
         [typeof(bool)] = "BIT",
         [typeof(DateTime)] = "DATETIME",
         [typeof(DateTimeOffset)] = "DATETIMEOFFSET",
         [typeof(int)] = "INT",
         [typeof(long)] = "BIGINT",
         [typeof(double)] = "FLOAT",
         [typeof(Guid)] = "UNIQUEIDENTIFIER",
      };

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

         foreach(KeyValuePair<string, DynamicValue> cell in row)
         {
            Type t = cell.Value.OriginalType;

            if(!TypeToSqlTypeName.TryGetValue(t, out string typeName))
            {
               typeName = "NVARCHAR(MAX)";
            }

            s.Append("[");
            s.Append(cell.Key);
            s.Append("] ");
            s.Append(typeName);
            s.Append(" NULL, ");
         }

         s.Append($"PRIMARY KEY ([{PartitionKey}], [{RowKey}])");
         s.Append(")");

         SqlCommand cmd = _sqlConnection.CreateCommand();
         cmd.CommandText = s.ToString();
         return cmd;
      }
   }
}

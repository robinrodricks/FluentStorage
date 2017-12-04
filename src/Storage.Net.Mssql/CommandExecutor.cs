using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NetBox;
using Storage.Net.Table;

namespace Storage.Net.Mssql
{
   class CommandExecutor
   {
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

      private readonly SqlConnection _sqlConnection;
      private readonly SqlConfiguration _config;

      public CommandExecutor(SqlConnection sqlConnection, SqlConfiguration config)
      {
         _sqlConnection = sqlConnection ?? throw new ArgumentNullException(nameof(sqlConnection));
         _config = config;
      }

      private async Task ExecAsync(SqlCommand cmd)
      {
         await CheckConnection();

         await cmd.ExecuteNonQueryAsync();
      }

      private async Task ExecAsync(string tableName, SqlCommand cmd, TableRow row)
      {
         try
         {
            await ExecAsync(cmd);
         }
         catch (SqlException ex) when (ex.Number == 208)
         {
            await CreateTable(tableName, row);

            await ExecAsync(cmd);
         }
         catch (SqlException ex) when (ex.Number == 2627)
         {
            throw new StorageException(ErrorCode.DuplicateKey, ex);
         }

      }

      private async Task CreateTable(string tableName, TableRow row)
      {
         SqlCommand cmd = BuildCreateSchemaCommand(tableName, row);
         await ExecAsync(cmd);
      }

      public async Task ExecAsync(string sql)
      {
         SqlCommand cmd = _sqlConnection.CreateCommand();
         cmd.CommandText = sql;
         await ExecAsync(cmd);
      }

      public async Task<ICollection<TableRow>> ExecRowsAsync(string sql, params object[] parameters)
      {
         var result = new List<TableRow>();

         using (SqlCommand cmd = _sqlConnection.CreateCommand())
         {
            cmd.CommandText = string.Format(sql, parameters);

            await CheckConnection();

            using (SqlDataReader reader = await cmd.ExecuteReaderAsync())
            {
               while(await reader.ReadAsync())
               {
                  result.Add(CreateRow(reader));
               }
            }
         }

         return result;
      }

      private async Task CheckConnection()
      {
         if(_sqlConnection.State != ConnectionState.Open)
         {
            await _sqlConnection.OpenAsync();
         }
      }

      private TableRow CreateRow(SqlDataReader reader)
      {
         var row = new TableRow(
            reader[_config.PartitionKeyColumnName] as string,
            reader[_config.RowKeyColumnName] as string);

         string[] columnNames = Enumerable
            .Range(0, reader.FieldCount)
            .Select(i => reader.GetName(i))
            .Where(n => n != _config.PartitionKeyColumnName)
            .Where(n => n != _config.RowKeyColumnName)
            .Where(n => n != null)
            .ToArray();

         foreach(string name in columnNames)
         {
            object value = reader[name];
            row[name] = new DynamicValue(value);
         }

         return row;
      }

      private SqlCommand BuildCreateSchemaCommand(string tableName, TableRow row)
      {
         var s = new StringBuilder();
         s.Append("CREATE TABLE [");
         s.Append(tableName);
         s.Append("] ([");
         s.Append(_config.PartitionKeyColumnName);
         s.Append("] NVARCHAR(50) NOT NULL, [");
         s.Append(_config.RowKeyColumnName);
         s.Append("] NVARCHAR(50) NOT NULL, ");

         foreach (KeyValuePair<string, DynamicValue> cell in row)
         {
            Type t = cell.Value.OriginalType;

            if (!TypeToSqlTypeName.TryGetValue(t, out string typeName))
            {
               typeName = "NVARCHAR(MAX)";
            }

            s.Append("[");
            s.Append(cell.Key);
            s.Append("] ");
            s.Append(typeName);
            s.Append(" NULL, ");
         }

         s.Append($"PRIMARY KEY ([{_config.PartitionKeyColumnName}], [{_config.RowKeyColumnName}])");
         s.Append(")");

         SqlCommand cmd = _sqlConnection.CreateCommand();
         cmd.CommandText = s.ToString();
         return cmd;
      }

   }
}

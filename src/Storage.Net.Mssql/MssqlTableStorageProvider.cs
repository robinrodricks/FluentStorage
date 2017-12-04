using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using Storage.Net.Table;

namespace Storage.Net.Mssql
{
   public class MssqlTableStorageProvider : ITableStorageProvider
   {
      private readonly SqlConnection _connection;
      private readonly CommandBuilder _cb;
      private readonly CommandExecutor _exec;
      private readonly SqlConfiguration _config;

      public MssqlTableStorageProvider(string connectionString, SqlConfiguration config)
      {
         _config = config ?? new SqlConfiguration();

         _connection = new SqlConnection(connectionString);
         _cb = new CommandBuilder(_connection, _config);
         _exec = new CommandExecutor(_connection, _config);
      }

      public bool HasOptimisticConcurrency => false;

      public async Task DeleteAsync(string tableName)
      {
         try
         {
            await _exec.ExecAsync($"DROP TABLE [{tableName}]");
         }
         catch(SqlException ex) when (ex.Number == 3701)
         {
         }
      }

      public Task DeleteAsync(string tableName, IEnumerable<TableRowId> rowIds)
      {
         throw new NotImplementedException();
      }

      public void Dispose()
      {
         _connection.Dispose();
      }

      public Task<IEnumerable<TableRow>> GetAsync(string tableName, string partitionKey)
      {
         throw new NotImplementedException();
      }

      public async Task<TableRow> GetAsync(string tableName, string partitionKey, string rowKey)
      {
         return (await InternalGetAsync(tableName, partitionKey, rowKey)).FirstOrDefault();
      }

      private async Task<ICollection<TableRow>> InternalGetAsync(string tableName, string partitionKey, string rowKey)
      {
         string sql = $"SELECT * FROM [{tableName}] WHERE [{_config.PartitionKeyColumnName}] = '{partitionKey}'";
         if (rowKey != null) sql += $" AND [{_config.RowKeyColumnName}] = '{rowKey}'";
         return await _exec.ExecRowsAsync(sql);
      }

      public async Task InsertAsync(string tableName, IEnumerable<TableRow> rows)
      {
         await Exec(tableName, rows);
      }

      public Task InsertOrReplaceAsync(string tableName, IEnumerable<TableRow> rows)
      {
         throw new NotImplementedException();
      }

      public async Task<IEnumerable<string>> ListTableNamesAsync()
      {
         string sql = $"SELECT TABLE_NAME FROM {_connection.Database}.INFORMATION_SCHEMA.TABLES WHERE TABLE_TYPE = 'BASE TABLE'";
         var names = new List<string>();

         using (SqlDataReader reader = await ExecReaderAsync(sql))
         {
            while(await reader.ReadAsync())
            {
               string name = reader[0] as string;
               names.Add(name);
            }
         }

         return names;
      }

      public Task MergeAsync(string tableName, IEnumerable<TableRow> rows)
      {
         throw new NotImplementedException();
      }

      public Task UpdateAsync(string tableName, IEnumerable<TableRow> rows)
      {
         throw new NotImplementedException();
      }

      private async Task Exec(string tableName, IEnumerable<TableRow> rows)
      {
         List<Tuple<SqlCommand, TableRow>> commands = rows.Select(r => Tuple.Create(_cb.BuidInsertRowCommand(tableName, r), r)).ToList();

         foreach(Tuple<SqlCommand, TableRow> cmd in commands)
         {
            await _exec.ExecRowsAsync(tableName, cmd.Item1, cmd.Item2);
         }
      }

      private async Task<SqlDataReader> ExecReaderAsync(string sql)
      {
         SqlCommand cmd = _connection.CreateCommand();
         cmd.CommandText = sql;

         if (_connection.State != ConnectionState.Open) await _connection.OpenAsync();
         return await cmd.ExecuteReaderAsync();
      }
   }
}

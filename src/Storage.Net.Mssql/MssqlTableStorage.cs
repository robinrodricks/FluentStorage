using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using Storage.Net.Table;

namespace Storage.Net.Mssql
{
   public class MssqlTableStorage : ITableStorage
   {
      private readonly SqlConnection _connection;

      public MssqlTableStorage(string connectionString)
      {
         _connection = new SqlConnection(connectionString);
      }

      public bool HasOptimisticConcurrency => false;

      public async Task DeleteAsync(string tableName)
      {
         try
         {
            await Exec($"DROP TABLE [{tableName}]");
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

      private async Task<List<TableRow>> InternalGetAsync(string tableName, string partitionKey, string rowKey)
      {
         string sql = $"SELECT * FROM [{tableName}] WHERE [{CommandBuilder.PartitionKey}] = '{partitionKey}'";
         if (rowKey != null) sql += $" AND [{CommandBuilder.RowKey}] = '{rowKey}'";
         var result = new List<TableRow>();

         using (SqlDataReader reader = await ExecReaderAsync(sql))
         {
            while(await reader.ReadAsync())
            {
               string pk = reader[CommandBuilder.PartitionKey] as string;
               string rk = reader[CommandBuilder.RowKey] as string;

               var row = new TableRow(pk, rk);

               result.Add(row);
            }
         }

         return result;
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
         var b = new CommandBuilder(_connection);
         List<Tuple<SqlCommand, TableRow>> commands = rows.Select(r => Tuple.Create(b.BuidInsertRowCommand(tableName, r), r)).ToList();

         foreach(Tuple<SqlCommand, TableRow> cmd in commands)
         {
            await Exec(tableName, cmd.Item1, cmd.Item2);
         }
      }

      private async Task Exec(string tableName, SqlCommand cmd, TableRow row)
      {
         try
         {
            await Exec(cmd);
         }
         catch (SqlException ex) when (ex.Number == 208)
         {
            await CreateTable(tableName, row);

            await Exec(cmd);
         }
         catch(SqlException ex) when (ex.Number == 2627)
         {
            throw new StorageException(ErrorCode.DuplicateKey, ex);
         }

      }

      private async Task CreateTable(string tableName, TableRow row)
      {
         var b = new CommandBuilder(_connection);
         SqlCommand cmd = b.BuildCreateSchemaCommand(tableName, row);
         await Exec(cmd);
      }

      private async Task Exec(string sql)
      {
         SqlCommand cmd = _connection.CreateCommand();
         cmd.CommandText = sql;
         await Exec(cmd);
      }

      private async Task<SqlDataReader> ExecReaderAsync(string sql)
      {
         SqlCommand cmd = _connection.CreateCommand();
         cmd.CommandText = sql;

         if (_connection.State != ConnectionState.Open) await _connection.OpenAsync();
         return await cmd.ExecuteReaderAsync();
      }

      private async Task Exec(SqlCommand cmd)
      {
         if (_connection.State != ConnectionState.Open) await _connection.OpenAsync();

         await cmd.ExecuteNonQueryAsync();
      }
   }
}

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Storage.Net.Table;

namespace Storage.Net.Mssql
{
   public class MssqlTableStorage : ITableStorage
   {
      public MssqlTableStorage(string connectionString, string tableName)
      {

      }

      public bool HasOptimisticConcurrency => throw new NotImplementedException();

      public Task DeleteAsync(string tableName)
      {
         throw new NotImplementedException();
      }

      public Task DeleteAsync(string tableName, IEnumerable<TableRowId> rowIds)
      {
         throw new NotImplementedException();
      }

      public void Dispose()
      {
         throw new NotImplementedException();
      }

      public Task<IEnumerable<TableRow>> GetAsync(string tableName, string partitionKey)
      {
         throw new NotImplementedException();
      }

      public Task<TableRow> GetAsync(string tableName, string partitionKey, string rowKey)
      {
         throw new NotImplementedException();
      }

      public Task InsertAsync(string tableName, IEnumerable<TableRow> rows)
      {
         throw new NotImplementedException();
      }

      public Task InsertOrReplaceAsync(string tableName, IEnumerable<TableRow> rows)
      {
         throw new NotImplementedException();
      }

      public Task<IEnumerable<string>> ListTableNamesAsync()
      {
         throw new NotImplementedException();
      }

      public Task MergeAsync(string tableName, IEnumerable<TableRow> rows)
      {
         throw new NotImplementedException();
      }

      public Task UpdateAsync(string tableName, IEnumerable<TableRow> rows)
      {
         throw new NotImplementedException();
      }
   }
}

using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Storage.Net.Table
{
   /// <summary>
   /// Storage that only implements async operations, synchronous methods are generated
   /// </summary>
   public abstract class AsyncTableStorage : ITableStorage
   {
      /// <summary>
      /// See interface
      /// </summary>
      public virtual bool HasOptimisticConcurrency
      {
         get
         {
            return false;
         }
      }

      public void Delete(string tableName)
      {
         CallAsync(() => DeleteAsync(tableName));
      }

      public abstract Task DeleteAsync(string tableName);

      public void Delete(string tableName, TableRowId rowId)
      {
         CallAsync(() => DeleteAsync(tableName, rowId));
      }

      public async Task DeleteAsync(string tableName, TableRowId rowId)
      {
         await DeleteAsync(tableName, new[] { rowId });
      }

      public void Delete(string tableName, IEnumerable<TableRowId> rowIds)
      {
         CallAsync(() => DeleteAsync(tableName, rowIds));
      }

      public abstract Task DeleteAsync(string tableName, IEnumerable<TableRowId> rowIds);

      public virtual void Dispose()
      {

      }

      public IEnumerable<TableRow> Get(string tableName, string partitionKey)
      {
         return CallAsync(() => GetAsync(tableName, partitionKey));
      }

      public abstract Task<IEnumerable<TableRow>> GetAsync(string tableName, string partitionKey);

      public TableRow Get(string tableName, string partitionKey, string rowKey)
      {
         return CallAsync(() => GetAsync(tableName, partitionKey, rowKey));
      }

      public abstract Task<TableRow> GetAsync(string tableName, string partitionKey, string rowKey);

      public void Insert(string tableName, TableRow row)
      {
         CallAsync(() => InsertAsync(tableName, row));
      }

      public async Task InsertAsync(string tableName, TableRow row)
      {
         await InsertAsync(tableName, new[] { row });
      }

      public void Insert(string tableName, IEnumerable<TableRow> rows)
      {
         CallAsync(() => InsertAsync(tableName, rows));
      }

      public abstract Task InsertAsync(string tableName, IEnumerable<TableRow> rows);

      public void InsertOrReplace(string tableName, TableRow row)
      {
         CallAsync(() => InsertOrReplaceAsync(tableName, row));
      }

      public async Task InsertOrReplaceAsync(string tableName, TableRow row)
      {
         await InsertOrReplaceAsync(tableName, new[] { row });
      }

      public void InsertOrReplace(string tableName, IEnumerable<TableRow> rows)
      {
         CallAsync(() => InsertOrReplaceAsync(tableName, rows));
      }

      public abstract Task InsertOrReplaceAsync(string tableName, IEnumerable<TableRow> rows);

      public IEnumerable<string> ListTableNames()
      {
         return CallAsync(() => ListTableNamesAsync());
      }

      public abstract Task<IEnumerable<string>> ListTableNamesAsync();

      public void Merge(string tableName, TableRow row)
      {
         CallAsync(() => MergeAsync(tableName, row));
      }

      public async Task MergeAsync(string tableName, TableRow row)
      {
         await MergeAsync(tableName, new[] { row });
      }

      public void Merge(string tableName, IEnumerable<TableRow> rows)
      {
         CallAsync(() => MergeAsync(tableName, rows));
      }

      public abstract Task MergeAsync(string tableName, IEnumerable<TableRow> rows);

      public void Update(string tableName, TableRow row)
      {
         CallAsync(() => UpdateAsync(tableName, row));
      }

      public async Task UpdateAsync(string tableName, TableRow row)
      {
         await UpdateAsync(tableName, new[] { row });
      }

      public void Update(string tableName, IEnumerable<TableRow> rows)
      {
         CallAsync(() => UpdateAsync(tableName, rows));
      }

      public abstract Task UpdateAsync(string tableName, IEnumerable<TableRow> rows);

      private void CallAsync(Func<Task> lambda)
      {
         try
         {
            lambda().Wait();
         }
         catch (AggregateException ex)
         {
            throw ex.InnerException;
         }
      }

      private T CallAsync<T>(Func<Task<T>> lambda)
      {
         try
         {
            return lambda().Result;
         }
         catch (AggregateException ex)
         {
            throw ex.InnerException;
         }
      }

   }
}

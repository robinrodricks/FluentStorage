using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Storage.Net.Table
{
   /// <summary>
   /// Storage that virtualizes sync/async operations and tries to autogenerate the missing ones.
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

      public virtual void Delete(string tableName)
      {
         CallAsync(() => DeleteAsync(tableName));
      }

      public virtual Task DeleteAsync(string tableName)
      {
         Delete(tableName);

         return Task.FromResult(true);
      }

      public virtual void Delete(string tableName, TableRowId rowId)
      {
         Delete(tableName, new[] { rowId });
      }

      public virtual async Task DeleteAsync(string tableName, TableRowId rowId)
      {
         await DeleteAsync(tableName, new[] { rowId });
      }

      public virtual void Delete(string tableName, IEnumerable<TableRowId> rowIds)
      {
         CallAsync(() => DeleteAsync(tableName, rowIds));
      }

      public virtual Task DeleteAsync(string tableName, IEnumerable<TableRowId> rowIds)
      {
         Delete(tableName, rowIds);
         return Task.FromResult(true);
      }

      public virtual void Dispose()
      {

      }

      public virtual IEnumerable<TableRow> Get(string tableName, string partitionKey)
      {
         return CallAsync(() => GetAsync(tableName, partitionKey));
      }

      public virtual Task<IEnumerable<TableRow>> GetAsync(string tableName, string partitionKey)
      {
         return Task.FromResult(Get(tableName, partitionKey));
      }

      public virtual TableRow Get(string tableName, string partitionKey, string rowKey)
      {
         return CallAsync(() => GetAsync(tableName, partitionKey, rowKey));
      }

      public virtual Task<TableRow> GetAsync(string tableName, string partitionKey, string rowKey)
      {
         return Task.FromResult(Get(tableName, partitionKey, rowKey));
      }

      public virtual void Insert(string tableName, TableRow row)
      {
         Insert(tableName, new[] { row });
      }

      public virtual async Task InsertAsync(string tableName, TableRow row)
      {
         await InsertAsync(tableName, new[] { row });
      }

      public virtual void Insert(string tableName, IEnumerable<TableRow> rows)
      {
         CallAsync(() => InsertAsync(tableName, rows));
      }

      public virtual Task InsertAsync(string tableName, IEnumerable<TableRow> rows)
      {
         Insert(tableName, rows);

         return Task.FromResult(true);
      }

      public virtual void InsertOrReplace(string tableName, TableRow row)
      {
         InsertOrReplace(tableName, new[] { row });
      }

      public virtual async Task InsertOrReplaceAsync(string tableName, TableRow row)
      {
         await InsertOrReplaceAsync(tableName, new[] { row });
      }

      public virtual void InsertOrReplace(string tableName, IEnumerable<TableRow> rows)
      {
         CallAsync(() => InsertOrReplaceAsync(tableName, rows));
      }

      public virtual Task InsertOrReplaceAsync(string tableName, IEnumerable<TableRow> rows)
      {
         InsertOrReplace(tableName, rows);
         return Task.FromResult(true);
      }

      public virtual IEnumerable<string> ListTableNames()
      {
         return CallAsync(() => ListTableNamesAsync());
      }

      public virtual Task<IEnumerable<string>> ListTableNamesAsync()
      {
         return Task.FromResult(ListTableNames());
      }

      public virtual void Merge(string tableName, TableRow row)
      {
         Merge(tableName, new[] { row });
      }

      public virtual async Task MergeAsync(string tableName, TableRow row)
      {
         await MergeAsync(tableName, new[] { row });
      }

      public virtual void Merge(string tableName, IEnumerable<TableRow> rows)
      {
         CallAsync(() => MergeAsync(tableName, rows));
      }

      public virtual Task MergeAsync(string tableName, IEnumerable<TableRow> rows)
      {
         Merge(tableName, rows);
         return Task.FromResult(true);
      }

      public virtual void Update(string tableName, TableRow row)
      {
         CallAsync(() => UpdateAsync(tableName, row));
      }

      public virtual async Task UpdateAsync(string tableName, TableRow row)
      {
         await UpdateAsync(tableName, new[] { row });
      }

      public virtual void Update(string tableName, IEnumerable<TableRow> rows)
      {
         CallAsync(() => UpdateAsync(tableName, rows));
      }

      public virtual Task UpdateAsync(string tableName, IEnumerable<TableRow> rows)
      {
         Update(tableName, rows);
         return Task.FromResult(true);
      }

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

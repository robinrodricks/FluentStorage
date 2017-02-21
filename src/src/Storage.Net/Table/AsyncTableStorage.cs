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

      /// <summary>
      /// See interface
      /// </summary>
      public virtual void Delete(string tableName)
      {
         CallAsync(() => DeleteAsync(tableName));
      }

      /// <summary>
      /// See interface
      /// </summary>
      public virtual Task DeleteAsync(string tableName)
      {
         Delete(tableName);

         return Task.FromResult(true);
      }

      /// <summary>
      /// See interface
      /// </summary>
      public virtual void Delete(string tableName, TableRowId rowId)
      {
         if (rowId == null) return;

         Delete(tableName, new[] { rowId });
      }

      /// <summary>
      /// See interface
      /// </summary>
      public virtual async Task DeleteAsync(string tableName, TableRowId rowId)
      {
         if (rowId == null) return;

         await DeleteAsync(tableName, new[] { rowId });
      }

      /// <summary>
      /// See interface
      /// </summary>
      public virtual void Delete(string tableName, IEnumerable<TableRowId> rowIds)
      {
         CallAsync(() => DeleteAsync(tableName, rowIds));
      }

      /// <summary>
      /// See interface
      /// </summary>
      public virtual Task DeleteAsync(string tableName, IEnumerable<TableRowId> rowIds)
      {
         Delete(tableName, rowIds);
         return Task.FromResult(true);
      }

      /// <summary>
      /// See interface
      /// </summary>
      public virtual void Dispose()
      {

      }

      /// <summary>
      /// See interface
      /// </summary>
      public virtual IEnumerable<TableRow> Get(string tableName, string partitionKey)
      {
         return CallAsync(() => GetAsync(tableName, partitionKey));
      }

      /// <summary>
      /// See interface
      /// </summary>
      public virtual Task<IEnumerable<TableRow>> GetAsync(string tableName, string partitionKey)
      {
         return Task.FromResult(Get(tableName, partitionKey));
      }

      /// <summary>
      /// See interface
      /// </summary>
      public virtual TableRow Get(string tableName, string partitionKey, string rowKey)
      {
         return CallAsync(() => GetAsync(tableName, partitionKey, rowKey));
      }

      /// <summary>
      /// See interface
      /// </summary>
      public virtual Task<TableRow> GetAsync(string tableName, string partitionKey, string rowKey)
      {
         return Task.FromResult(Get(tableName, partitionKey, rowKey));
      }

      /// <summary>
      /// See interface
      /// </summary>
      public virtual void Insert(string tableName, TableRow row)
      {
         if (row == null) throw new ArgumentNullException(nameof(row));

         Insert(tableName, new[] { row });
      }

      /// <summary>
      /// See interface
      /// </summary>
      public virtual async Task InsertAsync(string tableName, TableRow row)
      {
         await InsertAsync(tableName, new[] { row });
      }

      /// <summary>
      /// See interface
      /// </summary>
      public virtual void Insert(string tableName, IEnumerable<TableRow> rows)
      {
         CallAsync(() => InsertAsync(tableName, rows));
      }

      /// <summary>
      /// See interface
      /// </summary>
      public virtual Task InsertAsync(string tableName, IEnumerable<TableRow> rows)
      {
         Insert(tableName, rows);

         return Task.FromResult(true);
      }

      /// <summary>
      /// See interface
      /// </summary>
      public virtual void InsertOrReplace(string tableName, TableRow row)
      {
         InsertOrReplace(tableName, new[] { row });
      }

      /// <summary>
      /// See interface
      /// </summary>
      public virtual async Task InsertOrReplaceAsync(string tableName, TableRow row)
      {
         await InsertOrReplaceAsync(tableName, new[] { row });
      }

      /// <summary>
      /// See interface
      /// </summary>
      public virtual void InsertOrReplace(string tableName, IEnumerable<TableRow> rows)
      {
         CallAsync(() => InsertOrReplaceAsync(tableName, rows));
      }

      /// <summary>
      /// See interface
      /// </summary>
      public virtual Task InsertOrReplaceAsync(string tableName, IEnumerable<TableRow> rows)
      {
         InsertOrReplace(tableName, rows);
         return Task.FromResult(true);
      }

      /// <summary>
      /// See interface
      /// </summary>
      public virtual IEnumerable<string> ListTableNames()
      {
         return CallAsync(() => ListTableNamesAsync());
      }

      /// <summary>
      /// See interface
      /// </summary>
      public virtual Task<IEnumerable<string>> ListTableNamesAsync()
      {
         return Task.FromResult(ListTableNames());
      }

      /// <summary>
      /// See interface
      /// </summary>
      public virtual void Merge(string tableName, TableRow row)
      {
         Merge(tableName, new[] { row });
      }

      /// <summary>
      /// See interface
      /// </summary>
      public virtual async Task MergeAsync(string tableName, TableRow row)
      {
         await MergeAsync(tableName, new[] { row });
      }

      /// <summary>
      /// See interface
      /// </summary>
      public virtual void Merge(string tableName, IEnumerable<TableRow> rows)
      {
         CallAsync(() => MergeAsync(tableName, rows));
      }

      /// <summary>
      /// See interface
      /// </summary>
      public virtual Task MergeAsync(string tableName, IEnumerable<TableRow> rows)
      {
         Merge(tableName, rows);
         return Task.FromResult(true);
      }

      /// <summary>
      /// See interface
      /// </summary>
      public virtual void Update(string tableName, TableRow row)
      {
         CallAsync(() => UpdateAsync(tableName, row));
      }

      /// <summary>
      /// See interface
      /// </summary>
      public virtual async Task UpdateAsync(string tableName, TableRow row)
      {
         await UpdateAsync(tableName, new[] { row });
      }

      /// <summary>
      /// See interface
      /// </summary>
      public virtual void Update(string tableName, IEnumerable<TableRow> rows)
      {
         CallAsync(() => UpdateAsync(tableName, rows));
      }

      /// <summary>
      /// See interface
      /// </summary>
      public virtual Task UpdateAsync(string tableName, IEnumerable<TableRow> rows)
      {
         Update(tableName, rows);
         return Task.FromResult(true);
      }

      private void CallAsync(Func<Task> lambda)
      {
         try
         {
            Task.Run(lambda).Wait();
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
            return Task.Run(lambda).Result;
         }
         catch (AggregateException ex)
         {
            throw ex.InnerException;
         }
      }

   }
}

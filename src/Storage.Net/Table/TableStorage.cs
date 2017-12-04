using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Storage.Net.Table
{
   /// <summary>
   /// Helper methods on top of <see cref="ITableStorageProvider"/>
   /// </summary>
   public class TableStorage
   {
      public TableStorage(ITableStorageProvider provider)
      {
         Provider = provider ?? throw new ArgumentNullException(nameof(provider));
      }

      public ITableStorageProvider Provider { get; }

      public Task<IEnumerable<string>> ListTableNamesAsync()
      {
         return Provider.ListTableNamesAsync();
      }

      public Task DeleteAsync(string tableName)
      {
         return Provider.DeleteAsync(tableName);
      }

      public Task<IEnumerable<TableRow>> GetAsync(string tableName, string partitionKey)
      {
         return Provider.GetAsync(tableName, partitionKey);
      }

      public Task<TableRow> GetAsync(string tableName, string partitionKey, string rowKey)
      {
         return Provider.GetAsync(tableName, partitionKey, rowKey);
      }

      public Task InsertAsync(string tableName, IEnumerable<TableRow> rows)
      {
         return Provider.InsertAsync(tableName, rows);
      }

      public Task InsertAsync(string tableName, TableRow row)
      {
         return Provider.InsertAsync(tableName, new TableRow[] { row });
      }

      public Task InsertOrReplaceAsync(string tableName, IEnumerable<TableRow> rows)
      {
         return Provider.InsertOrReplaceAsync(tableName, rows);
      }

      public Task UpdateAsync(string tableName, IEnumerable<TableRow> rows)
      {
         return Provider.UpdateAsync(tableName, rows);
      }

      public Task MergeAsync(string tableName, IEnumerable<TableRow> rows)
      {
         return Provider.MergeAsync(tableName, rows);
      }

      public Task DeleteAsync(string tableName, IEnumerable<TableRowId> rowIds)
      {
         return Provider.DeleteAsync(tableName, rowIds);
      }
   }
}

using System.Threading.Tasks;

namespace Storage.Net.Table
{
   /// <summary>
   /// Extension utilities for table storage
   /// </summary>
   public static class TableStorageExtensions
   {
      private static readonly EntityConverter converter = new EntityConverter();

      /// <summary>
      /// Inserts a single row
      /// </summary>
      /// <param name="storage">Table storage reference</param>
      /// <param name="tableName">Table name, required.</param>
      /// <param name="row">Row to insert, required.</param>
      /// <exception cref="StorageException">
      /// If the row already exists throws this exception with <see cref="ErrorCode.DuplicateKey"/>
      /// </exception>
      public static void Insert(this ITableStorage storage, string tableName, TableRow row)
      {
         if (row == null) return;

         storage.Insert(tableName, new[] { row });
      }

      /// <summary>
      /// Inserts a single row
      /// </summary>
      /// <param name="storage">Table storage reference</param>
      /// <param name="tableName">Table name, required.</param>
      /// <param name="row">Row to insert, required.</param>
      /// <exception cref="StorageException">
      /// If the row already exists throws this exception with <see cref="ErrorCode.DuplicateKey"/>
      /// </exception>
      public static async Task InsertAsync(this ITableStorage storage, string tableName, TableRow row)
      {
         if (row == null) return;

         await storage.InsertAsync(tableName, new[] { row });
      }

      public static void Insert<T>(this ITableStorage storage, string tableName, T[] entities)
      {
         TableRow[] rows = converter.Convert(entities);

         storage.Insert(tableName, rows);
      }

      /// <summary>
      /// Inserts a single row, or replaces the value if row already exists.
      /// </summary>
      /// <param name="storage">Table storage reference</param>
      /// <param name="tableName">Table name, required.</param>
      /// <param name="row">Row to insert, required.</param>
      public static void InsertOrReplace(this ITableStorage storage, string tableName, TableRow row)
      {
         if (row == null) return;

         storage.InsertOrReplace(tableName, new[] { row });
      }

      /// <summary>
      /// Inserts a single row, or replaces the value if row already exists.
      /// </summary>
      /// <param name="storage">Table storage reference</param>
      /// <param name="tableName">Table name, required.</param>
      /// <param name="row">Row to insert, required.</param>
      public static async Task InsertOrReplaceAsync(this ITableStorage storage, string tableName, TableRow row)
      {
         if (row == null) return;

         await storage.InsertOrReplaceAsync(tableName, new[] { row });
      }

      /// <summary>
      /// Updates single row
      /// <param name="storage">Table storage reference</param>
      /// <param name="tableName">Table name, required.</param>
      /// <param name="row">Row to update, required.</param>
      /// </summary>
      public static void Update(this ITableStorage storage, string tableName, TableRow row)
      {
         if (row == null) return;

         storage.Update(tableName, new[] { row });
      }

      /// <summary>
      /// Updates single row
      /// <param name="storage">Table storage reference</param>
      /// <param name="tableName">Table name, required.</param>
      /// <param name="row">Row to update, required.</param>
      /// </summary>
      public static async Task UpdateAsync(this ITableStorage storage, string tableName, TableRow row)
      {
         if (row == null) return;

         await storage.UpdateAsync(tableName, new[] { row });
      }

      /// <summary>
      /// Merges a single row
      /// <param name="storage">Table storage reference</param>
      /// <param name="tableName">Table name, required.</param>
      /// <param name="row">Row to insert, required.</param>
      /// </summary>
      public static void Merge(this ITableStorage storage, string tableName, TableRow row)
      {
         if (row == null) return;

         storage.Merge(tableName, new[] { row });
      }

      /// <summary>
      /// Merges a single row
      /// <param name="storage">Table storage reference</param>
      /// <param name="tableName">Table name, required.</param>
      /// <param name="row">Row to insert, required.</param>
      /// </summary>
      public static async Task MergeAsync(this ITableStorage storage, string tableName, TableRow row)
      {
         if (row == null) return;

         await storage.MergeAsync(tableName, new[] { row });
      }

      /// <summary>
      /// Deletes a single row
      /// <param name="storage">Table storage reference</param>
      /// <param name="tableName">Table name, required.</param>
      /// <param name="rowId">Row ID to delete, required.</param>
      /// </summary>
      public static void Delete(this ITableStorage storage, string tableName, TableRowId rowId)
      {
         if (rowId == null) return;

         storage.Delete(tableName, new[] { rowId });
      }

      /// <summary>
      /// Deletes a single row
      /// <param name="storage">Table storage reference</param>
      /// <param name="tableName">Table name, required.</param>
      /// <param name="rowId">Row ID to delete, required.</param>
      /// </summary>
      public static async Task DeleteAsync(this ITableStorage storage, string tableName, TableRowId rowId)
      {
         if (rowId == null) return;

         await storage.DeleteAsync(tableName, new[] { rowId });
      }

   }
}

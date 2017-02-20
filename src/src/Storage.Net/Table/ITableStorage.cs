using System;
using System.Collections.Generic;

namespace Storage.Net.Table
{
   /// <summary>
   /// Common interface for working with table storage
   /// </summary>
   public interface ITableStorage : IDisposable
   {
      /// <summary>
      /// Storage returns true if it supports optimistic concurrency. This mode affect the update
      /// operations on rows and will throw an exception if the version of the row you have is not
      /// up to date.
      /// </summary>
      bool HasOptimisticConcurrency { get; }

      /// <summary>
      /// Returns the list of all table names in the table storage.
      /// </summary>
      /// <returns></returns>
      IEnumerable<string> ListTableNames();

      /// <summary>
      /// Deletes entire table. If table doesn't exist no errors are raised.
      /// </summary>
      /// <param name="tableName">Name of the table to delete. Passing null raises <see cref="ArgumentNullException"/></param>
      void Delete(string tableName);

      /// <summary>
      /// Gets rows by partition key.
      /// </summary>
      /// <param name="tableName">Table name, required.</param>
      /// <param name="partitionKey">Partition key of the table, required.</param>
      /// <returns>
      /// List of table rows in the table's partition. This method never returns null and if no records
      /// are found an empty collection is returned.
      /// </returns>
      IEnumerable<TableRow> Get(string tableName, string partitionKey);

      /// <summary>
      /// Gets a single row by partition key and row key as this uniquely idendifies a row.
      /// </summary>
      /// <param name="tableName">Table name, required.</param>
      /// <param name="partitionKey">Partition key of the table, required.</param>
      /// <param name="rowKey">Row key, required.</param>
      /// <returns>
      /// List of table rows in the table's partition. This method never returns null and if no records
      /// are found an empty collection is returned.
      /// </returns>
      TableRow Get(string tableName, string partitionKey, string rowKey);

      /// <summary>
      /// Inserts rows in the table.
      /// </summary>
      /// <param name="tableName">Table name, required.</param>
      /// <param name="rows">Rows to insert, required. The rows can belong to different partitions.</param>
      /// <exception cref="StorageException">
      /// If the row already exists throws this exception with <see cref="ErrorCode.DuplicateKey"/>.
      /// Note that exception is thrown only for partiton batch. If rows contains more than one partition to insert
      /// some of them may succeed and some may fail.
      /// </exception>
      void Insert(string tableName, IEnumerable<TableRow> rows);

      /// <summary>
      /// Inserts a single row
      /// </summary>
      /// <param name="tableName">Table name, required.</param>
      /// <param name="row">Row to insert, required.</param>
      /// <exception cref="StorageException">
      /// If the row already exists throws this exception with <see cref="ErrorCode.DuplicateKey"/>
      /// </exception>
      void Insert(string tableName, TableRow row);

      /// <summary>
      /// Inserts rows in the table, and if they exist replaces them with a new value.
      /// </summary>
      /// <param name="tableName">Table name, required.</param>
      /// <param name="rows">Rows to insert, required. The rows can belong to different partitions.</param>
      /// <exception cref="StorageException">
      /// If input rows have duplicated keys throws this exception with <see cref="ErrorCode.DuplicateKey"/>
      /// </exception>
      void InsertOrReplace(string tableName, IEnumerable<TableRow> rows);

      /// <summary>
      /// Inserts a single row, or replaces the value if row already exists.
      /// </summary>
      /// <param name="tableName">Table name, required.</param>
      /// <param name="row">Row to insert, required.</param>
      void InsertOrReplace(string tableName, TableRow row);

      /// <summary>
      /// Updates multiple rows. Note that all the rows must belong to the same partition.
      /// </summary>
      void Update(string tableName, IEnumerable<TableRow> rows);

      /// <summary>
      /// Updates single row
      /// </summary>
      void Update(string tableName, TableRow row);

      /// <summary>
      /// Merges multiple rows. Note that all rows must belong to the same partition
      /// </summary>
      void Merge(string tableName, IEnumerable<TableRow> rows);

      /// <summary>
      /// Merges a single row
      /// </summary>
      void Merge(string tableName, TableRow row);

      /// <summary>
      /// Deletes multiple rows
      /// </summary>
      void Delete(string tableName, IEnumerable<TableRowId> rowIds);

      /// <summary>
      /// Deletes a single row
      /// </summary>
      void Delete(string tableName, TableRowId rowId);
   }
}

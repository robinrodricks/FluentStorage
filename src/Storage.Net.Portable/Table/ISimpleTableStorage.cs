using System.Collections.Generic;

namespace Storage.Net.Table
{
   /// <summary>
   /// Common interface for working with table storage
   /// </summary>
   public interface ISimpleTableStorage
   {
      bool HasOptimisticConcurrency { get; }

      IEnumerable<string> ListTableNames();

      /// <summary>
      /// Deletes entire table
      /// </summary>
      /// <param name="tableName"></param>
      void Delete(string tableName);

      IEnumerable<TableRow> Get(string tableName, string partitionKey);

      TableRow Get(string tableName, string partitionKey, string rowKey);

      void Insert(string tableName, IEnumerable<TableRow> rows);

      void Insert(string tableName, TableRow row);

      void Update(string tableName, IEnumerable<TableRow> rows);

      void Update(string tableName, TableRow row);

      void Merge(string tableName, IEnumerable<TableRow> rows);

      void Merge(string tableName, TableRow row);

      void Delete(string tableName, IEnumerable<TableRowId> rowIds);

      void Delete(string tableName, TableRowId rowId);
   }
}

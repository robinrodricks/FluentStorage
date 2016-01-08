using System.Collections.Generic;

namespace Storage.Net.Table
{
   /// <summary>
   /// More advanced table storage interface
   /// </summary>
   public interface ITableStorage : ISimpleTableStorage
   {
      /// <summary>
      /// Gets rows by specified conditional parameters
      /// </summary>
      /// <param name="tableName">Table name</param>
      /// <param name="partitionKey">Paritition key</param>
      /// <param name="rowKey">Row key</param>
      /// <param name="maxRecords">Max number of records to read</param>
      /// <returns></returns>
      IEnumerable<TableRow> Get(string tableName, string partitionKey, string rowKey, int maxRecords);
   }
}

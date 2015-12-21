using System.Collections.Generic;

namespace Storage.Net.Table
{
   public interface ITableStorage : ISimpleTableStorage
   {
      IEnumerable<TableRow> Get(string tableName, string partitionKey, string rowKey, int maxRecords);
   }
}

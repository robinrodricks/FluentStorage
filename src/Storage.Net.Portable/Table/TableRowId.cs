using System;

namespace Storage.Net.Table
{
   public class TableRowId
   {
      public TableRowId(string partitionKey, string rowKey)
      {
         if(partitionKey == null) throw new ArgumentNullException(nameof(partitionKey));
         if(rowKey == null) throw new ArgumentNullException(nameof(rowKey));

         PartitionKey = partitionKey;
         RowKey = rowKey;
      }

      public string PartitionKey { get; set; }

      public string RowKey { get; set; }

      /// <summary>
      /// Optimistic concurrency key, optional
      /// </summary>
      public string ConcurrencyKey { get; set; }

      /// <summary>
      /// Last modified date, optional
      /// </summary>
      public DateTimeOffset LastModified { get; set; }
   }
}

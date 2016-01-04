using System;

namespace Storage.Net.Table
{
   /// <summary>
   /// ID structure of the <see cref="TableRow"/>
   /// </summary>
   public class TableRowId
   {
      /// <summary>
      /// Constructs an instance of <see cref="TableRowId"/>
      /// </summary>
      /// <param name="partitionKey">Partition key</param>
      /// <param name="rowKey">Row key</param>
      public TableRowId(string partitionKey, string rowKey)
      {
         if(partitionKey == null) throw new ArgumentNullException(nameof(partitionKey));
         if(rowKey == null) throw new ArgumentNullException(nameof(rowKey));

         PartitionKey = partitionKey;
         RowKey = rowKey;
      }

      /// <summary>
      /// Partition key
      /// </summary>
      public string PartitionKey { get; set; }

      /// <summary>
      /// Row key
      /// </summary>
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

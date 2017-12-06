using System;
using System.Collections.Generic;
using System.Text;

namespace Storage.Net.Mssql
{
   public class SqlConfiguration
   {
      public string PartitionKeyColumnName { get; set; } = "PK";

      public string RowKeyColumnName { get; set; } = "RK";

      /// <summary>
      /// When bulk copy is used (inserting over 10 rows at a time) this settings indicates the timeout value to fail after.
      /// Default is 1 minute.
      /// </summary>
      public TimeSpan BulkCopyTimeout { get; set; } = TimeSpan.FromMinutes(1);
   }
}

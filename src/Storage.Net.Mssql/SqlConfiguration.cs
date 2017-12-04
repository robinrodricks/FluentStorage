using System;
using System.Collections.Generic;
using System.Text;

namespace Storage.Net.Mssql
{
   public class SqlConfiguration
   {
      public string PartitionKeyColumnName { get; set; } = "PK";

      public string RowKeyColumnName { get; set; } = "RK";
   }
}

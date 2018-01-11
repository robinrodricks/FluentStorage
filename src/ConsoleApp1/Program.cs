using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Storage.Net;
using Storage.Net.Table;
using Storage.Net.Mssql;

namespace ConsoleApp1
{
   public class TestData
   {
      public string Data { get; set; }
      public double Value { get; set; }

      public TableRow ToTableRow()
      {
         var row = new TableRow("2017-01-01", Guid.NewGuid().ToString());
         row["Data"] = Data;
         row["Value"] = Value;

         return row;
      }
   }

   class Program
   {
      static void Main(string[] args)
      {
         ITableStorageProvider _tables = StorageFactory.Tables.MssqlServer(
              "Server=tcp:aloneguid.database.windows.net,1433;Initial Catalog=clearbank;Persist Security Info=False;User ID=ivan;Password='Password00;';MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;;",
              new SqlConfiguration
              {
                 PartitionKeyColumnName = "date",
                 RowKeyColumnName = "rowId",
                 BulkCopyTimeout = TimeSpan.FromMinutes(10)
              });

         IEnumerable<TestData> data = new List<TestData>()
            {
               new TestData()
               {
                  Data = "test1",
                  Value = 10
           
               },
               new TestData()
               {
                  Data = "test2",
                  Value = 10
               }
               ,
               new TestData()
               {
                  Data = "test3",
                  Value = 10
               }
               ,
               new TestData()
               {
                  Data = "test4",
                  Value = 10
               }
               ,
               new TestData()
               {
                  Data = "test5",
                  Value = 10
               }
               ,
               new TestData()
               {
                  Data = "test6",
                  Value = 10
               },
               new TestData()
               {
                  Data = "test7",
                  Value = 10
               },
               new TestData()
               {
                  Data = "test8",
                  Value = 10
               },
               new TestData()
               {
                  Data = "test9",
                  Value = 10
               },
               new TestData()
               {
                  Data = "test10",
                  Value = 10
               },
               new TestData()
               {
                  Data = "test11",
                  Value = 10
               }
            };

         IEnumerable<TableRow> rows = data.Select(t => t.ToTableRow());

         _tables.InsertAsync("mynewtestsc", rows).Wait();
      }
   }
}

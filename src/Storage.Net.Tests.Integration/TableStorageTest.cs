using System;
using System.Collections.Generic;
using System.Linq;
using Config.Net;
using Xunit;
using Storage.Net.Table;
using Storage.Net.Table.Files;
using Storage.Net.Azure.Table;
using Storage.Net.Net45.Esent;
using System.IO;

namespace Storage.Net.Tests.Integration
{
   public class CsvFilesTableStorageTest : TableStorageTest
   {
      public CsvFilesTableStorageTest() : base("csv-files") { }
   }

   public class AzureTableStorageTest : TableStorageTest
   {
      public AzureTableStorageTest() : base("azure") { }
   }

   public class EsentTableStorageTest : TableStorageTest
   {
      public EsentTableStorageTest() : base("esent") { }
   }

   public abstract class TableStorageTest : AbstractTestFixture
   {
      private readonly string _name;
      private ITableStorage _tables;
      private string _tableName;

      protected TableStorageTest(string name)
      {
         _name = name;
   
         if(_name == "csv-files")
         {
            _tables = new CsvFileTableStorage(TestDir);
         }
         else if(_name == "azure")
         {
            _tables = new AzureTableStorage(
               Cfg.Read(TestSettings.AzureStorageName),
               Cfg.Read(TestSettings.AzureStorageKey));
         }
         else if(_name == "esent")
         {
            _tables = new EsentTableStorage(
               Path.Combine(TestDir.FullName, "test.edb"));
         }

         _tableName = "TableStorageTest" + Guid.NewGuid().ToString().Replace("-", "");
      }

      public override void Dispose()
      {
         _tables.Delete(_tableName);

         _tables.Dispose();

         Cleanup();

         base.Dispose();
      }

      [Fact]
      public void WriteRows_TwoRows_DoesntFail()
      {
         var row1 = new TableRow("part1", "k1");
         row1["col1"] = "value1";

         var row2 = new TableRow("part2", "1");
         row2["col1"] = "value2";
            
         _tables.Insert(_tableName, new[] {row1, row2});
      }

      [Fact]
      public void WriteRows_EmailRowKey_CanFetchBack()
      {
         //this only tests encoding problem

         var row = new TableRow("partition", "ivan@si.com");
         _tables.Insert("test", row);

         var foundRow = _tables.Get("test", "partition", "ivan@si.com");
         Assert.NotNull(foundRow);
      }

      [Fact]
      public void ListTableNames_Unknown_DoesntCrash()
      {
         _tables.ListTableNames();
      }

      [Fact]
      public void ListTables_NoTablesWriteARow_OneTable()
      {
         int count = _tables.ListTableNames().Count();

         var row1 = new TableRow("part1", "k1");
         row1["col1"] = "value1";
         _tables.Insert(_tableName, new[] {row1});

         var names = _tables.ListTableNames().ToList();
         Assert.Equal(count + 1, names.Count);
         Assert.True(names.Contains(_tableName));
         _tables.Delete(_tableName);
      }

      [Fact]
      public void DeleteRows_AddTwoRows_DeletedDisappears()
      {
         var row1 = new TableRow("part1", "1");
         row1["col1"] = "value1";
         var row2 = new TableRow("part1", "2");
         row2["col1"] = "value2";

         _tables.Insert(_tableName, new[] {row1, row2});
         _tables.Delete(_tableName, new[] {new TableRowId("part1", "2")});
         var rows = _tables.Get(_tableName, "part1");

         Assert.Equal(1, rows.Count());
      }

      [Fact]
      public void DeleteRows_NullArrayInput_DoesntCrash()
      {
         _tables.Delete(_tableName, (TableRowId[])null);
      }

      [Fact]
      public void DeleteRows_NullInput_DoesntCrash()
      {
         _tables.Delete(_tableName, (TableRowId)null);
      }

      [Fact]
      public void DeleteRows_TableDoesNotExist_DoesntCrash()
      {
         _tables.Delete(_tableName + "d", (TableRowId)null);
      }

      [Fact]
      public void GetOne_AddTwoRows_ReturnsTheOne()
      {
         var row1 = new TableRow("part1", "1");
         row1["col1"] = "value1";
         var row2 = new TableRow("part1", "2");
         row2["col1"] = "value2";

         _tables.Insert(_tableName, new[] {row1, row2});

         TableRow theOne = _tables.Get(_tableName, "part1", "2");
         Assert.Equal("part1", theOne.PartitionKey);
         Assert.Equal("2", theOne.RowKey);
      }

      [Fact]
      public void Concurrency_DeleteWithWrongEtag_Fails()
      {
         if (!_tables.HasOptimisticConcurrency) return;

         //insert one row
         var row = new TableRow("pk", "rk");
         row["c"] = "1";
         _tables.Insert(_tableName, row);
         Assert.NotNull(row.Id.ConcurrencyKey);

         //change it's ETag and try to delete which must fail!
         row.Id.ConcurrencyKey = Guid.NewGuid().ToString();
         Assert.Throws<StorageException>(() =>
         {
            _tables.Delete(_tableName, row.Id);
         });
      }

      [Fact]
      public void Concurrency_RowOldCopy_MustNotUpdate()
      {
         if (!_tables.HasOptimisticConcurrency) return;

         //insert one row
         var row = new TableRow("pk", "rk");
         row["c"] = "1";
         _tables.Insert(_tableName, row);
         Assert.NotNull(row.Id.ConcurrencyKey);

         //update with a new value
         var row1 = new TableRow("pk", "rk");
         row1["c"] = "2";
         _tables.Merge(_tableName, row1);
         Assert.NotNull(row1.Id.ConcurrencyKey);
         Assert.NotEqual(row.Id.ConcurrencyKey, row1.Id.ConcurrencyKey);

         //now use the first row (old ETag) to set the new value
         row["c"] = "2";
         Assert.Throws<StorageException>(() => _tables.Update(_tableName, row));

         Assert.Throws<StorageException>(() => _tables.Delete(_tableName, row.Id));
      }

      [Fact]
      public void WriteReadValues_VariableRows_StillReads()
      {
         var row1 = new TableRow("pk", "rk1");
         row1["col1"] = "val1";
         row1["col2"] = "val2";

         var row2 = new TableRow("pk", "rk2");
         row2["col2"] = "val2";
         row2["col3"] = "val3";

         _tables.Insert("test", new[] {row1, row2});

         TableRow row11 = _tables.Get("test", "pk", "rk1");
         TableRow row12 = _tables.Get("test", "pk", "rk2");


         Assert.Equal("val1", (string)row11["col1"]);
         Assert.Equal("val2", (string)row11["col2"]);

         Assert.Equal("val2", (string)row12["col2"]);
         Assert.Equal("val3", (string)row12["col3"]);

      }

      [Fact]
      public void ReadFromAllPartitions_WriteToTwoPartitions_GetsAll()
      {
         var row1 = new TableRow("pk1", "rk1");
         var row2 = new TableRow("pk2", "rk2");

         _tables.Insert("test", new[] { row1, row2 });

         List<TableRow> rows = _tables.Get("test", null).ToList();

         Assert.True(rows.Count >= 2);
      }

      [Fact]
      public void Read_TableDoesNotExist_ReturnsNull()
      {
         IEnumerable<TableRow> rows = _tables.Get(_tableName, "test");

         Assert.Null(rows);
      }

      [Fact]
      public void ReadWritePartitions_TwoPartitions_ReadSeparately()
      {
         var row1 = new TableRow("pk1", "rk1");
         var row2 = new TableRow("pk2", "rk1");

         _tables.Insert("test", new[] { row1, row2 });

         TableRow row11 = _tables.Get("test", "pk1", "rk1");
         TableRow row22 = _tables.Get("test", "pk2", "rk1");

         Assert.NotNull(row11);
         Assert.NotNull(row22);
         Assert.Equal("pk1", row11.PartitionKey);
         Assert.Equal("rk1", row11.RowKey);
         Assert.Equal("pk2", row22.PartitionKey);
         Assert.Equal("rk1", row22.RowKey);
      }
   }
}

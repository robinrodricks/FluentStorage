using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;
using Storage.Net.Table;
using System.IO;
using NetBox;
using Config.Net;

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

   //bored of maintaining ESENT tests when I don't really use ESENT anymore for a long time
   /*public class EsentTableStorageTest : TableStorageTest
   {
      public EsentTableStorageTest() : base("esent") { }
   }*/

   public abstract class TableStorageTest : AbstractTestFixture
   {
      private readonly string _name;
      private ITableStorage _tables;
      private string _tableName;
      private ITestSettings _settings;

      protected TableStorageTest(string name)
      {
         _settings = new ConfigurationBuilder<ITestSettings>()
            .UseIniFile("c:\\tmp\\integration-tests.ini")
            .UseEnvironmentVariables()
            .Build();

         _name = name;

         if(_name == "csv-files")
         {
            _tables = StorageFactory.Tables.CsvFiles(TestDir);
         }
         else if(_name == "azure")
         {
            _tables = StorageFactory.Tables.AzureTableStorage(
               _settings.AzureStorageName,
               _settings.AzureStorageKey);
         }
         else if(_name == "esent")
         {
            _tables = StorageFactory.Tables.Esent(
               Path.Combine(TestDir.FullName, "test.edb"));
         }

         _tableName = "TableStorageTest" + Guid.NewGuid().ToString().Replace("-", "");
         //_tableName = "TableStorageTest";
      }

      public override void Dispose()
      {
         _tables.Delete(_tableName);

         _tables.Dispose();

         Cleanup();

         base.Dispose();
      }

      [Fact]
      public void DeleteTable_NonExisting_DoesntCrash()
      {
         _tables.Delete(_tableName + "del");
      }

      [Fact]
      public void DeleteTable_CreateAndDelete_BackToPreviousState()
      {
         int tableNo1 = _tables.ListTableNames().Count();

         _tables.Insert(_tableName, new TableRow("pk", "rk"));

         int tableNo2 = _tables.ListTableNames().Count();
         Assert.Equal(tableNo1 + 1, tableNo2);

         _tables.Delete(_tableName);

         int tableNo3 = _tables.ListTableNames().Count();
         Assert.Equal(tableNo3, tableNo1);
      }

      [Fact]
      public void DeleteTable_NullInput_ArgumentNullException()
      {
         Assert.Throws<ArgumentNullException>(() =>
         {
            _tables.Delete(null);
         });
      }

      [Fact]
      public void ListTables_NoTablesWriteARow_OneTable()
      {
         int count = _tables.ListTableNames().Count();

         var row1 = new TableRow("part1", "k1")
         {
            ["col1"] = "value1"
         };
         _tables.Insert(_tableName, new[] {row1});

         var names = _tables.ListTableNames().ToList();
         Assert.Equal(count + 1, names.Count);
         Assert.True(names.Contains(_tableName));
         _tables.Delete(_tableName);
      }

      [Fact]
      public void DeleteRows_AddTwoRows_DeletedDisappears()
      {
         var row1 = new TableRow("part1", "1")
         {
            ["col1"] = "value1"
         };
         var row2 = new TableRow("part1", "2")
         {
            ["col1"] = "value2"
         };
         _tables.Insert(_tableName, new[] {row1, row2});
         _tables.Delete(_tableName, new[] {new TableRowId("part1", "2")});
         IEnumerable<TableRow> rows = _tables.Get(_tableName, "part1");

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
      public void Concurrency_DeleteWithWrongEtag_Fails()
      {
         if (!_tables.HasOptimisticConcurrency) return;

         //insert one row
         var row = new TableRow("pk", "rk")
         {
            ["c"] = "1"
         };
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
         var row = new TableRow("pk", "rk")
         {
            ["c"] = "1"
         };
         _tables.Insert(_tableName, row);
         Assert.NotNull(row.Id.ConcurrencyKey);

         //update with a new value
         var row1 = new TableRow("pk", "rk")
         {
            ["c"] = "2"
         };
         _tables.Merge(_tableName, row1);
         Assert.NotNull(row1.Id.ConcurrencyKey);
         Assert.NotEqual(row.Id.ConcurrencyKey, row1.Id.ConcurrencyKey);

         //now use the first row (old ETag) to set the new value
         row["c"] = "2";
         Assert.Throws<StorageException>(() => _tables.Update(_tableName, row));

         Assert.Throws<StorageException>(() => _tables.Delete(_tableName, row.Id));
      }

      [Fact]
      public void Insert_OneRowNullTable_ArgumentNullException()
      {
         Assert.Throws<ArgumentNullException>(() => _tables.Insert(null, new TableRow("pk", "rk")));
      }

      [Fact]
      public void Insert_MultipleRowsNullTable_ArgumentNullException()
      {
         Assert.Throws<ArgumentNullException>(() =>
         {
            _tables.Insert(null, new[] { new TableRow("pk", "rk1"), new TableRow("pk", "rk2") });
         });
      }

      [Fact]
      public void Insert_ValidTableNullRow_ArgumentNullException()
      {
         Assert.Throws<ArgumentNullException>(() =>
         {
            _tables.Insert(_tableName, (TableRow)null);
         });
      }

      [Fact]
      public void Insert_ValidTableNullRows_ArgumentNullException()
      {
         Assert.Throws<ArgumentNullException>(() =>
         {
            _tables.Insert(_tableName, (TableRow[])null);
         });
      }

      [Fact]
      public void Insert_VariableRows_StillReads()
      {
         var row1 = new TableRow("pk", "rk1")
         {
            ["col1"] = "val1",
            ["col2"] = "val2"
         };
         var row2 = new TableRow("pk", "rk2")
         {
            ["col2"] = "val2",
            ["col3"] = "val3"
         };
         _tables.Insert(_tableName, new[] {row1, row2});

         TableRow row11 = _tables.Get(_tableName, "pk", "rk1");
         TableRow row12 = _tables.Get(_tableName, "pk", "rk2");


         Assert.Equal("val1", (string)row11["col1"]);
         Assert.Equal("val2", (string)row11["col2"]);

         Assert.Equal("val2", (string)row12["col2"]);
         Assert.Equal("val3", (string)row12["col3"]);

      }

      [Fact]
      public void Insert_DifferentTypes_ReadsBack()
      {
         var row = new TableRow("pk", "rk")
         {
            ["str"] = new DynamicValue("some string"),
            ["int"] = new DynamicValue(4),
            ["bytes"] = new byte[] { 0x1, 0x2 },
            ["date"] = DateTime.UtcNow
         };
         _tables.Insert(_tableName, row);
      }

      [Fact]
      public void Insert_Entities_ReadsBack()
      {
         var e = new TestEntity
         {
            PartitionKey = "pk",
            RowKey = "rke",

            M1 = "a string",
            Date = DateTime.UtcNow
         };

         _tables.Insert(_tableName, new[] { e });

         TestEntity fetched = _tables.Get<TestEntity>(_tableName, "pk", "rke");
      }

      [Fact]
      public void Dates_are_travelled_in_correct_timezone()
      {
         DateTime date = DateTime.UtcNow.RoundToSecond();

         var i = new TableRow("pk", "rk")
         {
            ["date"] = date
         };

         _tables.Insert(_tableName, i);

         TableRow o = _tables.Get(_tableName, "pk", "rk");
         DateTime date2 = o["date"];

         Assert.Equal(date, date2);
      }

      class TestEntity
      {
         public string PartitionKey { get; set; }

         public string RowKey { get; set; }

         public string M1 { get; set; }

         public DateTime Date { get; set; }
      }

      [Fact]
      public void Insert_TwoRows_DoesntFail()
      {
         var row1 = new TableRow("part1", "k1")
         {
            ["col1"] = "value1"
         };
         var row2 = new TableRow("part2", "1")
         {
            ["col1"] = "value2"
         };
         _tables.Insert(_tableName, new[] { row1, row2 });
      }

      [Fact]
      public void Insert_EmailRowKey_CanFetchBack()
      {
         //this only tests encoding problem

         var row = new TableRow("partition", "ivan@si.com");
         _tables.Insert(_tableName, row);

         TableRow foundRow = _tables.Get(_tableName, "partition", "ivan@si.com");
         Assert.NotNull(foundRow);
      }

      [Fact]
      public void Insert_DuplicateRow_StorageExceptionWithDuplicateKeyCode()
      {
         var row = new TableRow("pk", "rk");
         _tables.Insert(_tableName, row);

         StorageException ex = Assert.Throws<StorageException>(() => _tables.Insert(_tableName, row));
         Assert.Equal(ErrorCode.DuplicateKey, ex.ErrorCode);
      }

      [Fact]
      public void Insert_CleanTableDuplicateRows_FailsWithDuplicateKeyCode()
      {
         TableRow[] rows = new[]
         {
            new TableRow("pk", "rk"),
            new TableRow("pk", "rk"),
            new TableRow("pk", "rk1")
         };

         StorageException ex = Assert.Throws<StorageException>(() => _tables.Insert(_tableName, rows));
         Assert.Equal(ErrorCode.DuplicateKey, ex.ErrorCode);

         IEnumerable<TableRow> rows2 = _tables.Get(_tableName, "pk");
         Assert.Equal(0, rows2.Count());
      }

      [Fact]
      public void Insert_RowsExistInsertDuplicateRows_FailsWithDuplicateKeyCode()
      {
         var dupeRow = new TableRow("pk", "rk1");
         TableRow[] insertRows = new[] { new TableRow("pk", "rk2"), dupeRow };

         _tables.Insert(_tableName, dupeRow);

         StorageException ex = Assert.Throws<StorageException>(() => _tables.Insert(_tableName, insertRows));
         Assert.Equal(ErrorCode.DuplicateKey, ex.ErrorCode);

         IEnumerable<TableRow> rows2 = _tables.Get(_tableName, "pk");
         Assert.Equal(1, rows2.Count());
      }

      [Fact]
      public void InsertOrReplace_CleanTableDuplicateRowsInRequest_ContinuesAnyway()
      {
         TableRow[] rows = new[]
         {
            new TableRow("pk", "rk"),
            new TableRow("pk", "rk"),
            new TableRow("pk", "rk1")
         };


         StorageException ex = Assert.Throws<StorageException>(() => _tables.InsertOrReplace(_tableName, rows));
         Assert.Equal(ErrorCode.DuplicateKey, ex.ErrorCode);

         IEnumerable<TableRow> rows2 = _tables.Get(_tableName, "pk");
         Assert.Equal(0, rows2.Count());
      }

      [Fact]
      public void Get_NullTablePartition_ArgumentNullException()
      {
         Assert.Throws<ArgumentNullException>(() =>
         {
            _tables.Get(null, "p");
         });
      }

      [Fact]
      public void Get_TableButNullPartition_ArgumentNullException()
      {
         Assert.Throws<ArgumentNullException>(() => _tables.Get(_tableName, null));
      }

      [Fact]
      public void Get_TableNullPartitionRowKey_ArgumentNullException()
      {
         Assert.Throws<ArgumentNullException>(() => _tables.Get(_tableName, null, "rk"));
      }

      [Fact]
      public void Get_NullTablePartitionRowKey_ArgumentNullException()
      {
         Assert.Throws<ArgumentNullException>(() => _tables.Get(null, "pk", "rk"));
      }

      [Fact]
      public void Get_TablePartitionNullRowKey_ArgumentNullException()
      {
         Assert.Throws<ArgumentNullException>(() => _tables.Get(_tableName, "pk", null));
      }

      [Fact]
      public void Get_NonExistingTable_EmptyNotNull()
      {
         IEnumerable<TableRow> rows = _tables.Get(_tableName, "pk");

         Assert.NotNull(rows);
         Assert.True(!rows.Any());
      }

      [Fact]
      public void Get_WriteToTwoPartitions_GetsAll()
      {
         var row1 = new TableRow("pk1", "rk1");
         var row2 = new TableRow("pk2", "rk2");

         _tables.Insert(_tableName, new[] { row1, row2 });

         List<TableRow> rows1 = _tables.Get(_tableName, "pk1").ToList();
         List<TableRow> rows2 = _tables.Get(_tableName, "pk2").ToList();

         Assert.True(rows1.Count >= 1);
         Assert.True(rows2.Count >= 1);
      }

      [Fact]
      public void Get_AddTwoRows_ReturnsTheOne()
      {
         var row1 = new TableRow("part1", "1")
         {
            ["col1"] = "value1"
         };
         var row2 = new TableRow("part1", "2")
         {
            ["col1"] = "value2"
         };
         _tables.Insert(_tableName, new[] { row1, row2 });

         TableRow theOne = _tables.Get(_tableName, "part1", "2");
         Assert.Equal("part1", theOne.PartitionKey);
         Assert.Equal("2", theOne.RowKey);
      }

      [Fact]
      public void Get_TwoPartitions_ReadSeparately()
      {
         var row1 = new TableRow("pk1", "rk1");
         var row2 = new TableRow("pk2", "rk1");

         _tables.Insert(_tableName, new[] { row1, row2 });

         TableRow row11 = _tables.Get(_tableName, "pk1", "rk1");
         TableRow row22 = _tables.Get(_tableName, "pk2", "rk1");

         Assert.NotNull(row11);
         Assert.NotNull(row22);
         Assert.Equal("pk1", row11.PartitionKey);
         Assert.Equal("rk1", row11.RowKey);
         Assert.Equal("pk2", row22.PartitionKey);
         Assert.Equal("rk1", row22.RowKey);
      }

      [Fact]
      public void Get_PartitionDoesntExist_EmptyCollection()
      {
         IEnumerable<TableRow> rows = _tables.Get(_tableName, Guid.NewGuid().ToString());

         Assert.NotNull(rows);
         Assert.Equal(0, rows.Count());
      }

      [Fact]
      public void Get_RowDoesntExist_Null()
      {
         _tables.Insert(_tableName, new TableRow("pk", "rk1"));

         TableRow row = _tables.Get(_tableName, "pk", Guid.NewGuid().ToString());

         Assert.Null(row);
      }

      [Fact]
      public void Get_RowsDontExist_EmptyCollection()
      {
         _tables.Insert(_tableName, new TableRow("pk", "rk1"));
         _tables.Delete(_tableName, new TableRowId("pk", "rk1"));

         IEnumerable<TableRow> rows = _tables.Get(_tableName, "pk");
         Assert.NotNull(rows);
         Assert.Equal(0, rows.Count());
      }
   }
}

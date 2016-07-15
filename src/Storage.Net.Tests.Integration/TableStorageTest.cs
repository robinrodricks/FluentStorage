using System;
using System.Collections.Generic;
using System.Linq;
using Config.Net;
using NUnit.Framework;
using Storage.Net.Table;
using Storage.Net.Table.Files;
using Storage.Net.Azure.Table;
using Storage.Net.Net45.Esent;
using System.IO;

namespace Storage.Net.Tests.Integration
{
   [TestFixture("csv-files")]
   [TestFixture("azure")]
   //[TestFixture("esent")]
   public class TableStorageTest : AbstractTestFixture
   {
      private readonly string _name;
      private ITableStorage _tables;
      private string _tableName;

      public TableStorageTest(string name)
      {
         _name = name;
      }

      [SetUp]
      public void SetUp()
      {
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

      [TearDown]
      public void TearDown()
      {
         Cleanup();

         _tables.Delete(_tableName);

         _tables.Dispose();
      }

      [Test]
      public void WriteRows_TwoRows_ReadsBack()
      {
         var row1 = new TableRow("part1", "k1");
         row1["col1"] = "value1";

         var row2 = new TableRow("part2", "1");
         row2["col1"] = "value2";

         _tables.Insert("test", new[] {row1, row2});
      }

      [Test]
      public void WriteRows_EmailRowKey_CanFetchBack()
      {
         //this only tests encoding problem

         var row = new TableRow("partition", "ivan@si.com");
         _tables.Insert("test", row);

         var foundRow = _tables.Get("test", "partition", "ivan@si.com");
         Assert.IsNotNull(foundRow);
      }

      [Test]
      public void ListTableNames_Unknown_DoesntCrash()
      {
         _tables.ListTableNames();
      }

      [Test]
      public void ListTables_NoTablesWriteARow_OneTable()
      {
         int count = _tables.ListTableNames().Count();

         var row1 = new TableRow("part1", "k1");
         row1["col1"] = "value1";
         _tables.Insert(_tableName, new[] {row1});

         var names = _tables.ListTableNames().ToList();
         Assert.AreEqual(count + 1, names.Count);
         Assert.IsTrue(names.Contains(_tableName));
         _tables.Delete(_tableName);
      }

      [Test]
      public void DeleteRows_AddTwoRows_DeletedDisappears()
      {
         var row1 = new TableRow("part1", "1");
         row1["col1"] = "value1";
         var row2 = new TableRow("part1", "2");
         row2["col1"] = "value2";

         _tables.Insert(_tableName, new[] {row1, row2});
         _tables.Delete(_tableName, new[] {new TableRowId("part1", "2")});
         var rows = _tables.Get(_tableName, "part1");

         Assert.AreEqual(1, rows.Count());

      }

      [Test]
      public void GetOne_AddTwoRows_ReturnsTheOne()
      {
         var row1 = new TableRow("part1", "1");
         row1["col1"] = "value1";
         var row2 = new TableRow("part1", "2");
         row2["col1"] = "value2";

         _tables.Insert(_tableName, new[] {row1, row2});

         TableRow theOne = _tables.Get(_tableName, "part1", "2");
         Assert.AreEqual("part1", theOne.PartitionKey);
         Assert.AreEqual("2", theOne.RowKey);
      }

      [Test]
      public void Concurrency_DeleteWithWrongEtag_Fails()
      {
         if(!_tables.HasOptimisticConcurrency) Assert.Ignore();

         //insert one row
         var row = new TableRow("pk", "rk");
         row["c"] = "1";
         _tables.Insert(_tableName, row);
         Assert.IsNotNull(row.Id.ConcurrencyKey);

         //change it's ETag and try to delete which must fail!
         row.Id.ConcurrencyKey = Guid.NewGuid().ToString();
         Assert.Throws<StorageException>(() =>
         {
            _tables.Delete(_tableName, row.Id);
         });
      }

      [Test]
      public void Concurrency_RowOldCopy_MustNotUpdate()
      {
         if(!_tables.HasOptimisticConcurrency) Assert.Ignore();

         //insert one row
         var row = new TableRow("pk", "rk");
         row["c"] = "1";
         _tables.Insert(_tableName, row);
         Assert.IsNotNull(row.Id.ConcurrencyKey);

         //update with a new value
         var row1 = new TableRow("pk", "rk");
         row1["c"] = "2";
         _tables.Merge(_tableName, row1);
         Assert.IsNotNull(row1.Id.ConcurrencyKey);
         Assert.AreNotEqual(row.Id.ConcurrencyKey, row1.Id.ConcurrencyKey);

         //now use the first row (old ETag) to set the new value
         row["c"] = "2";
         Assert.Throws<StorageException>(() => _tables.Update(_tableName, row));

         Assert.Throws<StorageException>(() => _tables.Delete(_tableName, row.Id);
      }

      [Test]
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


         Assert.AreEqual("val1", (string)row11["col1"]);
         Assert.AreEqual("val2", (string)row11["col2"]);

         Assert.AreEqual("val2", (string)row12["col2"]);
         Assert.AreEqual("val3", (string)row12["col3"]);

      }

      [Test]
      public void ReadFromAllPartitions_WriteToTwoPartitions_GetsAll()
      {
         var row1 = new TableRow("pk1", "rk1");
         var row2 = new TableRow("pk2", "rk2");

         _tables.Insert("test", new[] { row1, row2 });

         List<TableRow> rows = _tables.Get("test", null).ToList();

         Assert.GreaterOrEqual(rows.Count, 2);
      }

      [Test]
      public void Read_TableDoesNotExist_ReturnsNull()
      {
         IEnumerable<TableRow> rows = _tables.Get(_tableName, "test");

         Assert.IsNull(rows);
      }
   }
}

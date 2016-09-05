using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Microsoft.Isam.Esent.Interop;
using Storage.Net.Table;
using ETable = Microsoft.Isam.Esent.Interop.Table;

namespace Storage.Net.Net45.Esent
{
   //see http://managedesent.codeplex.com/wikipage?title=StockSample&referringTitle=ManagedEsentDocumentation
   //full (and useful) documentation here: https://msdn.microsoft.com/en-us/library/gg269259(v=exchg.10).aspx

   //Note: The ESENT database file cannot be shared between multiple processes simultaneously.
   //ESENT works best for applications with simple, predefined queries; if you have an application with complex,
   //ad-hoc queries, a storage solution that provides a query layer will work better for you.

   /// <summary>
   /// ESENT (Jet Blue) backed engine for <see cref="ITableStorage"/>.
   /// The instance is not thread-safe. You should only use one instance per application, it's cannot be shared
   /// and is very expensive.
   /// </summary>
   public class EsentTableStorage : ITableStorage
   {
      //ISAM - Indexed Sequential Access Method

      private const string PartitionKeyName = "PartitionKey";
      private const string RowKeyName = "RowKey";
      private const string PrimaryIndexName = "primary";
      private const string PartitionIndexName = "partition";

      private string _databasePath;
      private string _databaseFolder;
      private Instance _jetInstance;
      private Session _jetSession;
      private readonly string _instanceName = Guid.NewGuid().ToString();
      private JET_DBID _jetDbId;
      private readonly Dictionary<string, Dictionary<string, JET_COLUMNID>> _tableNameToColumnNameToId =
         new Dictionary<string, Dictionary<string, JET_COLUMNID>>();

      public EsentTableStorage(string databasePath)
      {
         _databasePath = databasePath;
         _databaseFolder = new FileInfo(databasePath).Directory.FullName;

         if(!DatabaseExists())
         {
            CreateDatabase();
         }

         OpenDatabase();
      }

      private void CreateDatabase()
      {
         try
         {
            using (var instance = CreateInstance())
            {
               using (var session = new Session(instance))
               {
                  JET_DBID dbid;
                  Api.JetCreateDatabase(session, _databasePath, null, out dbid, CreateDatabaseGrbit.OverwriteExisting);
               }
            }
         }
         catch(Exception ex)
         {
            throw new StorageException($"cannot create database in '{_databasePath}'", ex);
         }
      }

      private bool DatabaseExists()
      {
         return File.Exists(_databasePath);
      }

      private void OpenDatabase()
      {
         try
         {
            _jetInstance = CreateInstance();
            _jetSession = new Session(_jetInstance);

            Api.JetAttachDatabase(_jetSession, _databasePath, AttachDatabaseGrbit.None);
            Api.JetOpenDatabase(_jetSession, _databasePath, null, out _jetDbId, OpenDatabaseGrbit.None);
         }
         catch(Exception ex)
         {
            throw new StorageException($"failed to open database at '{_databasePath}'", ex);
         }
      }

      private Instance CreateInstance()
      {
         var instance = new Instance(_instanceName);
         instance.Parameters.LogFileDirectory = _databaseFolder;
         instance.Parameters.SystemDirectory = _databaseFolder;
         instance.Parameters.TempDirectory = _databaseFolder;
         instance.Parameters.CircularLog = true;
         instance.Init();
         return instance;
      }

      private ETable OpenTable(string tableName, bool createIfNotExists)
      {
         ETable result;

         try
         {
            result = new ETable(_jetSession, _jetDbId, tableName, OpenTableGrbit.None);
         }
         catch(EsentObjectNotFoundException)
         {
            result = null;
         }

         if(result == null && createIfNotExists)
         {
            //create table
            using (var transaction = new Transaction(_jetSession))
            {
               JET_TABLEID tableId;
               Api.JetCreateTable(_jetSession, _jetDbId, tableName, 16, 100, out tableId);
               try
               {
                  //create index columns

                  var columnDef = new JET_COLUMNDEF
                  {
                     coltyp = JET_coltyp.Text,
                     cp = JET_CP.Unicode
                  };

                  //add key columns
                  JET_COLUMNID columnId;
                  Api.JetAddColumn(_jetSession, tableId, "PartitionKey", columnDef, null, 0, out columnId);
                  Api.JetAddColumn(_jetSession, tableId, "RowKey", columnDef, null, 0, out columnId);

                  //index key columns
                  //+ indicates ascending order, - descending
                  string indexDefPrimary = $"+{PartitionKeyName}\0+{RowKeyName}\0\0";
                  Api.JetCreateIndex(_jetSession, tableId, PrimaryIndexName, CreateIndexGrbit.IndexPrimary, indexDefPrimary, indexDefPrimary.Length, 100);

                  string indexDefPartition = $"+{PartitionKeyName}\0\0";
                  Api.JetCreateIndex(_jetSession, tableId, PartitionIndexName, CreateIndexGrbit.None, indexDefPartition, indexDefPartition.Length, 100);


                  transaction.Commit(CommitTransactionGrbit.LazyFlush);
               }
               finally
               {
                  Api.JetCloseTable(_jetSession, tableId);
               }

               result = new ETable(_jetSession, _jetDbId, tableName, OpenTableGrbit.None);
            }
         }

         return result;
      }

      private void SetValue(JET_TABLEID tableId, JET_COLUMNID column, TableCell cell)
      {
         switch(cell.DataType)
         {
            case CellType.String:
               Api.SetColumn(_jetSession, tableId, column, cell.RawValue, Encoding.Unicode);
               break;
            default:
               throw new NotSupportedException($"date type '{cell.DataType}' is not supported");
         }
      }

      private void SetValue(JET_TABLEID tableId, IDictionary<string, JET_COLUMNID> columns, TableRowId rowId)
      {
         Api.SetColumn(_jetSession, tableId, columns[PartitionKeyName], rowId.PartitionKey, Encoding.Unicode);
         Api.SetColumn(_jetSession, tableId, columns[RowKeyName], rowId.RowKey, Encoding.Unicode);
      }

      private Dictionary<string, JET_COLUMNID> RefreshColumnDictionary(string tableName, JET_TABLEID tableId)
      {
         IDictionary<string, JET_COLUMNID> freshColumns = Api.GetColumnDictionary(_jetSession, tableId);

         Dictionary<string, JET_COLUMNID> columnNameToId;
         if(_tableNameToColumnNameToId.TryGetValue(tableName, out columnNameToId))
         {
            columnNameToId.Clear();
         }
         else
         {
            columnNameToId = new Dictionary<string, JET_COLUMNID>();
            _tableNameToColumnNameToId[tableName] = columnNameToId;
         }

         columnNameToId.AddRange(freshColumns);

         return columnNameToId;
      }
      
      private Dictionary<string, JET_COLUMNID> EnsureColumnsExist(string tableName, JET_TABLEID tableId, IEnumerable<TableRow> rows)
      {
         if (!_tableNameToColumnNameToId.ContainsKey(tableName)) RefreshColumnDictionary(tableName, tableId);

         Dictionary<string, JET_COLUMNID> columns = _tableNameToColumnNameToId[tableName];

         //first detect all possible columns
         var columnNameToType = new Dictionary<string, CellType>();
         foreach(TableRow row in rows)
         {
            //todo: it is possible to have two rows with same column name but different types, this needs to be checked
            //and probably on higher level

            foreach (var column in row)
            {
               if (!columnNameToType.ContainsKey(column.Key))
               {
                  columnNameToType[column.Key] = column.Value.DataType;
               }
            }
         }

         //check against live columns and add missing
         Transaction t = null;
         try
         {
            foreach (var nt in columnNameToType)
            {
               if (!columns.ContainsKey(nt.Key))
               {
                  if (t == null) t = new Transaction(_jetSession);

                  //there is no column in live table, add it now
                  JET_COLUMNDEF def;
                  switch (nt.Value)
                  {
                     case CellType.String:
                        def = new JET_COLUMNDEF { coltyp = JET_coltyp.LongText, cp = JET_CP.Unicode };
                        break;
                     default:
                        throw new NotSupportedException($"column type '{nt.Value}' is not supported.");
                  }

                  JET_COLUMNID columnId;
                  Api.JetAddColumn(_jetSession, tableId, nt.Key, def, null, 0, out columnId);
               }

            }
         }
         finally
         {
            if (t != null) t.Commit(CommitTransactionGrbit.LazyFlush);
         }

         //re-read live columns to get most up-to-date result
         RefreshColumnDictionary(tableName, tableId);

         return columns;
      }

      private void ShutdownDatabase()
      {
         _jetSession.Dispose();
         _jetInstance.Dispose();
      }

      #region [ ITableStorage ]

      public void Dispose()
      {
         ShutdownDatabase();
      }

      public bool HasOptimisticConcurrency => false;

      public void Delete(string tableName)
      {
         if (tableName == null) throw new ArgumentNullException(nameof(tableName));

         try
         {
            Api.JetDeleteTable(_jetSession, _jetDbId, tableName);
         }
         catch(EsentObjectNotFoundException)
         {
            //table doesn't exist
         }
      }

      public void Delete(string tableName, TableRowId rowId)
      {
         if (rowId == null) return;

         Delete(tableName, new[] { rowId });
      }

      public void Delete(string tableName, IEnumerable<TableRowId> rowIds)
      {
         if (rowIds == null) return;

         using (ETable table = OpenTable(tableName, false))
         {
            foreach (TableRowId id in rowIds)
            {
               SeekToPkRk(table.JetTableid, id.PartitionKey, id.RowKey);

               Api.JetDelete(_jetSession, table.JetTableid);   //deletes current record
            }
         }
      }

      public IEnumerable<TableRow> Get(string tableName, string partitionKey)
      {
         if (tableName == null) throw new ArgumentNullException(nameof(tableName));
         if (partitionKey == null) throw new ArgumentNullException(nameof(partitionKey));

         return InternalGet(tableName, partitionKey, null, int.MaxValue);
      }

      public TableRow Get(string tableName, string partitionKey, string rowKey)
      {
         if (tableName == null) throw new ArgumentNullException(nameof(tableName));
         if (partitionKey == null) throw new ArgumentNullException(nameof(partitionKey));
         if (rowKey == null) throw new ArgumentNullException(nameof(rowKey));

         return InternalGet(tableName, partitionKey, rowKey, 1).FirstOrDefault();
      }

      public IEnumerable<TableRow> Get(string tableName, string partitionKey, string rowKey, int maxRecords)
      {
         if (tableName == null) throw new ArgumentNullException(nameof(tableName));
         if (partitionKey == null) throw new ArgumentNullException(nameof(partitionKey));
         if (rowKey == null) throw new ArgumentNullException(nameof(rowKey));

         return InternalGet(tableName, partitionKey, rowKey, maxRecords);
      }

      private IEnumerable<TableRow> InternalGet(string tableName, string partitionKey, string rowKey, int maxRecords)
      {
         using (ETable table = OpenTable(tableName, false))
         {
            if (table == null) return Enumerable.Empty<TableRow>();
            Dictionary<string, JET_COLUMNID> columns = _tableNameToColumnNameToId[tableName];

            SeekToPkRk(table.JetTableid, partitionKey, rowKey);
            return ReadAllRows(tableName, table.JetTableid, columns, rowKey == null ? maxRecords : 1);
         }
      }

      private void SeekToPkRk(JET_TABLEID tableId, string partitionKey, string rowKey)
      {
         //how to perform search: https://msdn.microsoft.com/en-us/library/gg269342(v=exchg.10).aspx

         if (partitionKey == null && rowKey == null)
         {
            //reading all table
            Api.JetSetCurrentIndex(_jetSession, tableId, null);
            Api.TryMoveFirst(_jetSession, tableId);
         }
         else if(rowKey == null)
         {
            //set index to partition index
            Api.JetSetCurrentIndex(_jetSession, tableId, PartitionIndexName);

            //partition index has only partition name
            Api.MakeKey(_jetSession, tableId, partitionKey, Encoding.Unicode, MakeKeyGrbit.NewKey);
            Api.JetSeek(_jetSession, tableId, SeekGrbit.SeekEQ);
         }
         else
         {
            //choose index to use
            Api.JetSetCurrentIndex(_jetSession, tableId, null);

            //create search key
            //To make a key for an index that has multiple columns in it you need to make one call to JetMakeKey
            //for each column. The first call should use the NewKey option.

            Api.MakeKey(_jetSession, tableId, partitionKey, Encoding.Unicode, MakeKeyGrbit.NewKey);
            Api.MakeKey(_jetSession, tableId, rowKey, Encoding.Unicode, MakeKeyGrbit.None);

            //the next JetSeek call essentially positions the cursor to the first matching record meaning
            //that you can start reading straight away. Do not call MoveFirst because that will reset cursor
            //to the beginning of the index so the search result will be lost
            JET_wrn r = Api.JetSeek(_jetSession, tableId, SeekGrbit.SeekEQ);
         }
      }

      private IEnumerable<TableRow> ReadAllRows(string tableName, JET_TABLEID tableId, Dictionary<string, JET_COLUMNID> columns, int maxRecords)
      {
         var result = new List<TableRow>();
         List<string> valueColumns = columns.Keys.Where(n => n != PartitionKeyName && n != RowKeyName).ToList();

         do
         {
            string partitionKey = Api.RetrieveColumnAsString(_jetSession, tableId, columns[PartitionKeyName]);
            string rowKey = Api.RetrieveColumnAsString(_jetSession, tableId, columns[RowKeyName]);
            var row = new TableRow(partitionKey, rowKey);

            foreach (string colName in valueColumns)
            {
               JET_COLUMNID colId = columns[colName];
               JET_COLUMNDEF colDef;
               Api.JetGetColumnInfo(_jetSession, _jetDbId, tableName, colName, out colDef);

               switch (colDef.coltyp)
               {
                  case JET_coltyp.LongText:
                     row[colName] = Api.RetrieveColumnAsString(_jetSession, tableId, colId);
                     break;
                  default:
                     throw new ApplicationException($"column type {colDef.coltyp} is not supported");
               }
            }

            result.Add(row);
         }
         while (result.Count < maxRecords && Api.TryMoveNext(_jetSession, tableId));

         return result;
      }

      public void Insert(string tableName, TableRow row)
      {
         if (row == null) throw new ArgumentNullException(nameof(row));

         Insert(tableName, new[] { row });
      }

      public void Insert(string tableName, IEnumerable<TableRow> rows)
      {
         using (ETable table = OpenTable(tableName, true))
         {
            JET_TABLEID tableId = table.JetTableid;

            Dictionary<string, JET_COLUMNID> columns = EnsureColumnsExist(tableName, tableId, rows);

            using (var transaction = new Transaction(_jetSession))
            {
               foreach(TableRow row in rows)
               {
                  using (var update = new Update(_jetSession, table.JetTableid, JET_prep.Insert))
                  {
                     SetValue(tableId, columns, row.Id);

                     foreach(KeyValuePair<string, TableCell> column in row)
                     {
                        SetValue(tableId, columns[column.Key], column.Value);
                     }

                     update.Save();
                  }
               }

               transaction.Commit(CommitTransactionGrbit.None);
            }
         }
      }

      public IEnumerable<string> ListTableNames()
      {
         return Api.GetTableNames(_jetSession, _jetDbId).ToList();
      }

      public void Merge(string tableName, TableRow row)
      {
         throw new NotImplementedException();
      }

      public void Merge(string tableName, IEnumerable<TableRow> rows)
      {
         throw new NotImplementedException();
      }

      public void Update(string tableName, TableRow row)
      {
         throw new NotImplementedException();
      }

      public void Update(string tableName, IEnumerable<TableRow> rows)
      {
         throw new NotImplementedException();
      }

      #endregion

   }
}
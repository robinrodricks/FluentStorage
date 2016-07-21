using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Isam.Esent.Interop;
using Storage.Net.Table;
using ETable = Microsoft.Isam.Esent.Interop.Table;

namespace Storage.Net.Net45.Esent
{
   //see http://managedesent.codeplex.com/wikipage?title=StockSample&referringTitle=ManagedEsentDocumentation
   //full (and useful) documentation here: https://msdn.microsoft.com/en-us/library/gg269259(v=exchg.10).aspx
   public class EsentTableStorage : ITableStorage
   {
      //ISAM - Indexed Sequential Access Method

      private const string PartitionKeyName = "PartitionKey";
      private const string RowKeyName = "RowKey";
      private const string PKIndexName = "PK";
      private const string PKRKIndexName = "PKRK";

      private string _databasePath;
      private string _databaseFolder;
      private Instance _jetInstance;
      private Session _jetSession;
      private readonly string _instanceName = Guid.NewGuid().ToString();
      private JET_DBID _jetDbId;

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
         using (var instance = CreateInstance())
         {
            using (var session = new Session(instance))
            {
               JET_DBID dbid;
               Api.JetCreateDatabase(session, _databasePath, null, out dbid, CreateDatabaseGrbit.OverwriteExisting);
            }
         }
      }

      private bool DatabaseExists()
      {
         return File.Exists(_databasePath);
      }

      private void OpenDatabase()
      {
         _jetInstance = CreateInstance();
         _jetSession = new Session(_jetInstance);

         Api.JetAttachDatabase(_jetSession, _databasePath, AttachDatabaseGrbit.None);
         Api.JetOpenDatabase(_jetSession, _databasePath, null, out _jetDbId, OpenDatabaseGrbit.None);
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
                  string indexDefPrimary = $"+{PartitionKeyName}\0+{RowKeyName}\0\0";
                  Api.JetCreateIndex(_jetSession, tableId, PKRKIndexName, CreateIndexGrbit.IndexUnique, indexDefPrimary, indexDefPrimary.Length, 100);


                  string indexDefPartition = $"+{PartitionKeyName}\0\0";
                  Api.JetCreateIndex(_jetSession, tableId, PKIndexName, CreateIndexGrbit.IndexPrimary, indexDefPartition, indexDefPartition.Length, 100);


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
      
      private IDictionary<string, JET_COLUMNID> GetOrCreateColumns(JET_TABLEID tableId, IEnumerable<TableRow> rows)
      {
         IDictionary<string, JET_COLUMNID> columns = Api.GetColumnDictionary(_jetSession, tableId);

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
         columns = Api.GetColumnDictionary(_jetSession, tableId);

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
         Api.JetDeleteTable(_jetSession, _jetDbId, tableName);
      }

      public void Delete(string tableName, TableRowId rowId)
      {
         throw new NotImplementedException();
      }

      public void Delete(string tableName, IEnumerable<TableRowId> rowIds)
      {
         throw new NotImplementedException();
      }

      public IEnumerable<TableRow> Get(string tableName, string partitionKey)
      {
         return Get(tableName, partitionKey, null, int.MaxValue);
      }

      public TableRow Get(string tableName, string partitionKey, string rowKey)
      {
         return Get(tableName, partitionKey, rowKey, 1).FirstOrDefault();
      }

      public IEnumerable<TableRow> Get(string tableName, string partitionKey, string rowKey, int maxRecords)
      {
         using (ETable table = OpenTable(tableName, false))
         {
            if (table == null) return null;

            //how to perform search: https://msdn.microsoft.com/en-us/library/gg269342(v=exchg.10).aspx

            //choose index to use
            string indexName = rowKey == null ? PKIndexName : PKRKIndexName;
            Api.JetSetCurrentIndex(_jetSession, table.JetTableid, indexName);

         }
      }

      public void Insert(string tableName, TableRow row)
      {
         throw new NotImplementedException();
      }

      public void Insert(string tableName, IEnumerable<TableRow> rows)
      {
         using (ETable table = OpenTable(tableName, true))
         {
            JET_TABLEID tableId = table.JetTableid;

            IDictionary<string, JET_COLUMNID> columns = GetOrCreateColumns(tableId, rows);

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
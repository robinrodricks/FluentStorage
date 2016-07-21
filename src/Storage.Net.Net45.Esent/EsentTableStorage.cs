using System;
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
   public class EsentTableStorage : ITableStorage
   {
      //ISAM - Indexed Sequential Access Method

      private const string PartitionKeyName = "PartitionKey";
      private const string RowKeyName = "RowKey";

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

      private ETable OpenTable(string tableName)
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

         if(result == null)
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
                  string indexDef = $"+{PartitionKeyName}\0\0";
                  Api.JetCreateIndex(_jetSession, tableId, "PK", CreateIndexGrbit.IndexPrimary, indexDef, indexDef.Length, 100);

                  indexDef = $"+{PartitionKeyName}\0+{RowKeyName}\0\0";
                  Api.JetCreateIndex(_jetSession, tableId, "PKRK", CreateIndexGrbit.IndexUnique, indexDef, indexDef.Length, 100);

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
         throw new NotImplementedException();
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
         throw new NotImplementedException();
      }

      public TableRow Get(string tableName, string partitionKey, string rowKey)
      {
         throw new NotImplementedException();
      }

      public IEnumerable<TableRow> Get(string tableName, string partitionKey, string rowKey, int maxRecords)
      {
         throw new NotImplementedException();
      }

      public void Insert(string tableName, TableRow row)
      {
         throw new NotImplementedException();
      }

      public void Insert(string tableName, IEnumerable<TableRow> rows)
      {
         using (ETable table = OpenTable(tableName))
         {
            using (var transaction = new Transaction(_jetSession))
            {
               foreach(TableRow row in rows)
               {
                  using (var update = new Update(_jetSession, table.JetTableid, JET_prep.Insert))
                  {
                     //Api.SetColumn(_jetSession, table.JetTableid, )

                     update.Save();
                  }
               }

               transaction.Commit(CommitTransactionGrbit.None);
            }
         }
      }

      public IEnumerable<string> ListTableNames()
      {
         throw new NotImplementedException();
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
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Isam.Esent.Interop;
using Storage.Net.Table;

namespace Storage.Net.Net45.Esent
{
   public class EsentTableStorage : ITableStorage
   {
      //ISAM - Indexed Sequential Access Method

      private string _databasePath;
      private Instance _jetInstance;
      private Session _jetSession;

      public EsentTableStorage(string databasePath)
      {
         _databasePath = databasePath;

         CreateOrOpenDatabase();
      }

      private void CreateOrOpenDatabase()
      {
         _jetInstance = new Instance(Guid.NewGuid().ToString());
         _jetInstance.Init();
         _jetSession = new Session(_jetInstance);

         JET_DBID dbid;
         Api.JetCreateDatabase(_jetSession, _databasePath, null, out dbid, CreateDatabaseGrbit.OverwriteExisting);
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
         throw new NotImplementedException();
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
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Storage.Net.Table
{
   /// <summary>
   /// Storage that only implements async operations, synchronous methods are generated
   /// </summary>
   public abstract class AsyncTableStorage : ITableStorage
   {
      /// <summary>
      /// See interface
      /// </summary>
      public virtual bool HasOptimisticConcurrency
      {
         get
         {
            return false;
         }
      }

      public void Delete(string tableName)
      {
         try
         {
            DeleteAsync(tableName).Wait();
         }
         catch(AggregateException ex)
         {
            throw ex.InnerException;
         }
      }

      public abstract Task DeleteAsync(string tableName);

      public void Delete(string tableName, TableRowId rowId)
      {
         throw new NotImplementedException();
      }

      public void Delete(string tableName, IEnumerable<TableRowId> rowIds)
      {
         throw new NotImplementedException();
      }

      public void Dispose()
      {
         throw new NotImplementedException();
      }

      public IEnumerable<TableRow> Get(string tableName, string partitionKey)
      {
         try
         {
            return GetAsync(tableName, partitionKey).Result;
         }
         catch(AggregateException ex)
         {
            throw ex.InnerException;
         }
      }

      public abstract Task<IEnumerable<TableRow>> GetAsync(string tableName, string partitionKey);

      public TableRow Get(string tableName, string partitionKey, string rowKey)
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

      public void InsertOrReplace(string tableName, TableRow row)
      {
         throw new NotImplementedException();
      }

      public void InsertOrReplace(string tableName, IEnumerable<TableRow> rows)
      {
         throw new NotImplementedException();
      }

      public IEnumerable<string> ListTableNames()
      {
         try
         {
            return ListTableNamesAsync().Result;
         }
         catch(AggregateException ex)
         {
            throw ex.InnerException;
         }
      }

      public abstract Task<IEnumerable<string>> ListTableNamesAsync();

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
   }
}

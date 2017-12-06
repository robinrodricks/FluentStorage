#if NETFULL
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NetBox;
using Storage.Net.Table;

namespace Storage.Net.Mssql
{
   class BulkCopyExecutor
   {
      private readonly SqlConnection _connection;
      private readonly SqlConfiguration _configuration;
      private readonly string _tableName;

      public BulkCopyExecutor(SqlConnection connection, SqlConfiguration configuration, string tableName)
      {
         _connection = connection;
         _configuration = configuration;
         _tableName = tableName;
      }

      public async Task InsertAsync(IEnumerable<TableRow> rows)
      {
         List<TableRow> rowsList = rows.ToList();

         using (var sbc = new SqlBulkCopy(_connection))
         {
            sbc.DestinationTableName = _tableName;
            var dataTable = new DataTable(_tableName);
            AddColumns(dataTable, rowsList);

            //copy to rows
            foreach (TableRow row in rows)
            {
               DataRow dataRow = dataTable.NewRow();

               dataRow[_configuration.PartitionKeyColumnName] = row.PartitionKey;
               dataRow[_configuration.RowKeyColumnName] = row.RowKey;

               foreach (KeyValuePair<string, DynamicValue> cell in row)
               {
                  dataRow[cell.Key] = cell.Value.OriginalValue;
               }

               dataTable.Rows.Add(dataRow);
            }

            //todo: do I need to add column mapping still?

            //execute bulk copy?
            await _connection.OpenAsync();

            try
            {
               await sbc.WriteToServerAsync(dataTable);
            }
            catch(InvalidOperationException)
            {
               //table doesn't exist, create it now
               var composer = new TableComposer(_connection, _configuration);
               SqlCommand cmd = composer.BuildCreateSchemaCommand(_tableName, TableRow.Merge(rowsList));
               await cmd.ExecuteNonQueryAsync();

               //run it again
               await sbc.WriteToServerAsync(dataTable);
            }
         }
      }

      private void AddColumns(DataTable dataTable, IReadOnlyCollection<TableRow> rows)
      {
         TableRow schemaRow = TableRow.Merge(rows);

         dataTable.Columns.Add(_configuration.PartitionKeyColumnName);
         dataTable.Columns.Add(_configuration.RowKeyColumnName);

         foreach(KeyValuePair<string, DynamicValue> cell in schemaRow)
         {
            string name = cell.Key;
            dataTable.Columns.Add(name);
         }
      }

   }
}
#endif
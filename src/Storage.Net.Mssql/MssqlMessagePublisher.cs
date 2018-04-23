using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Storage.Net.Messaging;

namespace Storage.Net.Mssql
{
   class MssqlMessagePublisher : IMessagePublisher
   {
      private readonly SqlConnection _connection;

      public MssqlMessagePublisher(string connectionString, string tableName)
      {
         _connection = new SqlConnection(connectionString);
      }

      public Task PutMessagesAsync(IEnumerable<QueueMessage> messages, CancellationToken cancellationToken = default)
      {
         SqlCommand cmd = _connection.CreateCommand();
         cmd.CommandText = "insert into [{0}] (Message) values (@m)";

         cmd.Parameters.Add("@m", System.Data.SqlDbType.Binary);

         throw new NotImplementedException();
      }

      public void Dispose()
      {
         throw new NotImplementedException();
      }
   }
}
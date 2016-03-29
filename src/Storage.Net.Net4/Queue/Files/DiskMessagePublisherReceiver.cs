using System;
using System.Collections.Generic;
using System.IO;
using Storage.Net.Application;
using Storage.Net.Messaging;

namespace Storage.Net.Queue.Files
{
   class DiskMessagePublisherReceiver : IMessagePublisher, IMessageReceiver
   {
      private readonly SqliteDriver _sql;

      public DiskMessagePublisherReceiver(DirectoryInfo directory)
      {
         _sql = new SqliteDriver(directory);

         //_sql.EnsureTable("message", "Id");
      }

      #region [ Publisher ]

      public void PutMessage(QueueMessage message)
      {
         throw new NotImplementedException();
      }

      public void PutMessages(IEnumerable<QueueMessage> messages)
      {
         throw new NotImplementedException();
      }

      #endregion

      #region [ Receiver ]

      public QueueMessage ReceiveMessage()
      {
         throw new NotImplementedException();
      }

      public IEnumerable<QueueMessage> ReceiveMessages(int count)
      {
         throw new NotImplementedException();
      }

      public void ConfirmMessage(QueueMessage message)
      {
         throw new NotImplementedException();
      }

      #endregion

      public void Dispose()
      {
      }

   }
}

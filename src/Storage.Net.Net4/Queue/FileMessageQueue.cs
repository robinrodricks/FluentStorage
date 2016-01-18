using Storage.Net.Messaging;
using System;
using System.Collections.Generic;

namespace Storage.Net.Queue
{
   /// <summary>
   /// Disk based queueing abstraction
   /// </summary>
   class FileMessageQueue : IMessagePublisher, IMessageReceiver
   {
      public FileMessageQueue()
      {
         //
      }

      public void Dispose()
      {
      }

      #region [ Publisher ]

      public void PutMessage(QueueMessage message)
      {
         throw new NotImplementedException();
      }

      #endregion

      #region [ Receiver ]

      public void ConfirmMessage(QueueMessage message)
      {
         throw new NotImplementedException();
      }

      public QueueMessage ReceiveMessage()
      {
         throw new NotImplementedException();
      }

      public IEnumerable<QueueMessage> ReceiveMessages(int count)
      {
         throw new NotImplementedException();
      }

      #endregion
   }
}

using Storage.Net.Messaging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Storage.Net.Queue
{
   /// <summary>
   /// Disk based queueing abstraction
   /// </summary>
   public class FileMessageQueue : IMessagePublisher, IMessageReceiver
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

      public event EventHandler<QueueMessage> OnNewMessage;

      public void ConfirmMessage(QueueMessage message)
      {
         throw new NotImplementedException();
      }

      #endregion
   }
}

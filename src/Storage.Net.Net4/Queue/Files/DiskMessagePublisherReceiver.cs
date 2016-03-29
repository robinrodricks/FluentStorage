using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Storage.Net.Messaging;

namespace Storage.Net.Queue.Files
{
   public class DiskMessagePublisherReceiver : IMessagePublisher, IMessageReceiver
   {
      private readonly DirectoryInfo _directory;

      public DiskMessagePublisherReceiver(DirectoryInfo directory)
      {
         _directory = directory;
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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Storage.Net.Messaging
{
   /// <summary>
   /// Responsible for publishing messages
   /// </summary>
   public interface IMessagePublisher
   {
      /// <summary>
      /// Puts new message to the back of the qeuue
      /// </summary>
      /// <param name="message"></param>
      void PutMessage(QueueMessage message);
   }
}

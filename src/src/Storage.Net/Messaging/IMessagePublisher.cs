using System;
using System.Collections.Generic;

namespace Storage.Net.Messaging
{
   /// <summary>
   /// Responsible for publishing messages
   /// </summary>
   public interface IMessagePublisher : IDisposable
   {
      /// <summary>
      /// Puts new message to the back of the qeuue
      /// </summary>
      void PutMessage(QueueMessage message);

      /// <summary>
      /// Puts a new batch of messages to the back of the queue as quick and efficient as possible for
      /// a given queue implementation.
      /// </summary>
      void PutMessages(IEnumerable<QueueMessage> messages);
   }
}

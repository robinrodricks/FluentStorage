using Aloneguid.Support;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace Storage.Net.Messaging
{
   /// <summary>
   /// Creates queue in memory. All messages are lost when program exists.
   /// The class operates on <see cref="ConcurrentQueue{T}"/> implementation.
   /// Message IDs are generated based on random instance bound prefix and long integer taken from the current time
   /// tick count
   /// </summary>
   class InMemoryMessagePublisherReceiver : IMessagePublisher, IMessageReceiver
   {
      private readonly ConcurrentQueue<QueueMessage> _queue = new ConcurrentQueue<QueueMessage>();
      private readonly string _instancePrefix = Generator.RandomString;
      private readonly long _messageId = DateTime.UtcNow.Ticks;

      /// <summary>
      /// Puts the message in the inmemory queue
      /// </summary>
      /// <param name="message"></param>
      public void PutMessage(QueueMessage message)
      {
         throw new NotImplementedException();
      }

      /// <summary>
      /// Does nothing
      /// </summary>
      public void Dispose()
      {
      }

      /// <summary>
      /// Receives message from inmemory queue
      /// </summary>
      /// <returns></returns>
      public QueueMessage ReceiveMessage()
      {
         throw new NotImplementedException();
      }

      /// <summary>
      /// Deletes message from inmemory queue
      /// </summary>
      /// <param name="message"></param>
      public void ConfirmMessage(QueueMessage message)
      {
         throw new NotImplementedException();
      }

      public IEnumerable<QueueMessage> ReceiveMessages(int count)
      {
         throw new NotImplementedException();
      }

      public void PutMessages(IEnumerable<QueueMessage> messages)
      {
         throw new NotImplementedException();
      }
   }
}

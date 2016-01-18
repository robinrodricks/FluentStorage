using System;
using System.Collections.Generic;

namespace Storage.Net.Messaging
{
   /// <summary>
   /// Responsible for receiving messages
   /// </summary>
   public interface IMessageReceiver : IDisposable
   {
      /// <summary>
      /// Gets the next message from the queue if available.
      /// </summary>
      /// <returns>The message or null if there are no messages available in the queue.</returns>
      QueueMessage ReceiveMessage();

      /// <summary>
      /// Gets a batch of message from the queue, if available
      /// </summary>
      /// <param name="count">Number of messages to fetch</param>
      /// <returns>Batch of messages, or null if no messages are in the queue</returns>
      IEnumerable<QueueMessage> ReceiveMessages(int count);

      /// <summary>
      /// Confirmation call that the message was acknowledged and processed by the receiver.
      /// Client must call this when message processing has succeeded, otherwise the message will reappear.
      /// </summary>
      /// <param name="message"></param>
      void ConfirmMessage(QueueMessage message);
   }
}

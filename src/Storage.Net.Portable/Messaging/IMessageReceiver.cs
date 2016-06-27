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

      /// <summary>
      /// Moves the message to a dead letter queue
      /// </summary>
      void DeadLetter(QueueMessage message, string reason, string errorDescription);

      /// <summary>
      /// Starts automatic message pumping trying to use native features as much as possible. In most cases it is more efficient than calling
      /// <see cref="ReceiveMessage"/> in a loop in your application. Message pump stops when you dispose the instance.
      /// Disposing the instance will also stop message pump for you.
      /// </summary>
      void StartMessagePump(Action<QueueMessage> onMessage);
   }
}

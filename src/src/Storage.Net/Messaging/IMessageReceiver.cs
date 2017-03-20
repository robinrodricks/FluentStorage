using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Storage.Net.Messaging
{
   /// <summary>
   /// Responsible for receiving messages
   /// </summary>
   public interface IMessageReceiver : IDisposable
   {
      /// <summary>
      /// Gets a batch of message from the queue, if available
      /// </summary>
      /// <param name="count">Number of messages to fetch</param>
      /// <returns>Batch of messages, or null if no messages are in the queue</returns>
      IEnumerable<QueueMessage> ReceiveMessages(int count);

      /// <summary>
      /// Gets a batch of message from the queue, if available
      /// </summary>
      /// <param name="count">Number of messages to fetch</param>
      /// <returns>Batch of messages, or null if no messages are in the queue</returns>
      Task<IEnumerable<QueueMessage>> ReceiveMessagesAsync(int count);

      /// <summary>
      /// Confirmation call that the message was acknowledged and processed by the receiver.
      /// Client must call this when message processing has succeeded, otherwise the message will reappear,
      /// however this depends on implementation details when and how.
      /// </summary>
      /// <param name="message"></param>
      void ConfirmMessage(QueueMessage message);

      /// <summary>
      /// Confirmation call that the message was acknowledged and processed by the receiver.
      /// Client must call this when message processing has succeeded, otherwise the message will reappear,
      /// however this depends on implementation details when and how.
      /// </summary>
      /// <param name="message"></param>
      Task ConfirmMessageAsync(QueueMessage message);

      /// <summary>
      /// Moves the message to a dead letter queue
      /// </summary>
      void DeadLetter(QueueMessage message, string reason, string errorDescription);

      /// <summary>
      /// Moves the message to a dead letter queue
      /// </summary>
      Task DeadLetterAsync(QueueMessage message, string reason, string errorDescription);

      /// <summary>
      /// Starts automatic message pumping trying to use native features as much as possible. In most cases it is more efficient than calling
      /// <see cref="ReceiveMessages"/> in a loop in your application. Message pump stops when you dispose the instance.
      /// Disposing the instance will also stop message pump for you.
      /// </summary>
      void StartMessagePump(Action<QueueMessage> onMessage);
   }
}

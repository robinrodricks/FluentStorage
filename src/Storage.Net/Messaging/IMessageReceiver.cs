using System;
using System.Threading.Tasks;

namespace Storage.Net.Messaging
{
   /// <summary>
   /// Responsible for receiving messages
   /// </summary>
   public interface IMessageReceiver : IDisposable
   {
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
      Task DeadLetterAsync(QueueMessage message, string reason, string errorDescription);

      /// <summary>
      /// Starts automatic message pumping trying to use native features as much as possible. Message pump stops when you dispose the instance.
      /// Disposing the instance will also stop message pump for you.
      /// </summary>
      Task StartMessagePumpAsync(Func<QueueMessage, Task> onMessageAsync);
   }
}

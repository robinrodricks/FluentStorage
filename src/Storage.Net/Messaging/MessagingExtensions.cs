using System.Linq;
using System.Threading.Tasks;

namespace Storage.Net.Messaging
{
   /// <summary>
   /// Messaging extensions
   /// </summary>
   public static class MessagingExtensions
   {
      /// <summary>
      /// Puts a single message into the queue
      /// </summary>
      /// <param name="publisher">Publisher reference</param>
      /// <param name="message">Message to put</param>
      public static void PutMessage(this IMessagePublisher publisher, QueueMessage message)
      {
         publisher.PutMessages(new[] { message });
      }

      /// <summary>
      /// Puts a single message into the queue
      /// </summary>
      /// <param name="publisher">Publisher reference</param>
      /// <param name="message">Message to put</param>
      public static async Task PutMessageAsync(this IMessagePublisher publisher, QueueMessage message)
      {
         await publisher.PutMessagesAsync(new[] { message });
      }

      /// <summary>
      /// Gets the next message from the queue if available.
      /// </summary>
      /// <returns>The message or null if there are no messages available in the queue.</returns>
      public static QueueMessage ReceiveMessage(this IMessageReceiver receiver)
      {
         return receiver.ReceiveMessages(1)?.FirstOrDefault();
      }

      /// <summary>
      /// Gets the next message from the queue if available.
      /// </summary>
      /// <returns>The message or null if there are no messages available in the queue.</returns>
      public static async Task<QueueMessage> ReceiveMessageAsync(this IMessageReceiver receiver)
      {
         return (await receiver.ReceiveMessagesAsync(1))?.FirstOrDefault();
      }

   }
}

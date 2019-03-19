using System;
using System.Threading.Tasks;

namespace Storage.Net.Messaging
{
   /// <summary>
   /// Extensions for IMessagePublisher
   /// </summary>
   public static class MessagePublisherExtensions
   {
      /// <summary>
      /// Puts a new message to the back of the queue.
      /// </summary>
      public static Task PutMessageAsync(this IMessagePublisher messagePublisher, QueueMessage message)
      {
         if(message == null)
            throw new ArgumentNullException(nameof(message));

         return messagePublisher.PutMessagesAsync(new[] { message });
      }
   }
}

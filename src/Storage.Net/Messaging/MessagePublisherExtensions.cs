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
         if (message == null) return Task.FromResult(false);

         return messagePublisher.PutMessagesAsync(new[] { message });
      }
   }
}

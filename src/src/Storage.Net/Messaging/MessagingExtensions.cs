using System.Threading.Tasks;

namespace Storage.Net.Messaging
{
   /// <summary>
   /// Messaging extensions
   /// </summary>
   public static class MessagingExtensions
   {
      public static void PutMessage(this IMessagePublisher publisher, QueueMessage message)
      {
         publisher.PutMessages(new[] { message });
      }

      public static async Task PutMessageAsync(this IMessagePublisher publisher, QueueMessage message)
      {
         await publisher.PutMessagesAsync(new[] { message });
      }
   }
}

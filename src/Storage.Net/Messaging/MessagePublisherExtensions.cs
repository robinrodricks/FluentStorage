using System;
using System.Threading;
using System.Threading.Tasks;

namespace Storage.Net.Messaging
{
   /// <summary>
   /// Extensions for <see cref="IMessenger"/>
   /// </summary>
   public static class MessengerExtensions
   {
      /// <summary>
      /// Puts a new message to the back of the queue.
      /// </summary>
      public static Task SendAsync(this IMessenger messenger, string channelName, QueueMessage message, CancellationToken cancellationToken = default)
      {
         if(channelName is null)
            throw new ArgumentNullException(nameof(channelName));

         if(message == null)
            throw new ArgumentNullException(nameof(message));

         return messenger.SendAsync(channelName, new[] { message }, cancellationToken);
      }
   }
}

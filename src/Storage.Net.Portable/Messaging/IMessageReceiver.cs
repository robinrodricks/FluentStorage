using System;

namespace Storage.Net.Messaging
{
   /// <summary>
   /// Responsible for receiving messages
   /// </summary>
   public interface IMessageReceiver
   {
      /// <summary>
      /// Fired on new message received
      /// </summary>
      event EventHandler<QueueMessage> OnNewMessage;

      /// <summary>
      /// Confirmation call that the message was acknowledged and processed by the receiver.
      /// Client must call this when message processing has succeeded, otherwise the message will reappear.
      /// </summary>
      /// <param name="message"></param>
      void ConfirmMessage(QueueMessage message);
   }
}

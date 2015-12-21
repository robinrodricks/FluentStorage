using System;

namespace Storage.Net.Messaging
{
   /// <summary>
   /// Generic queue interface
   /// </summary>
   public interface IMessageQueue
   {
      /// <summary>
      /// Puts new message to the back of the qeuue
      /// </summary>
      /// <param name="content"></param>
      void PutMessage(string content);

      /// <summary>
      /// Gets a message from the queue and marks it as invisible so that further requeusts to this message won't return it
      /// </summary>
      /// <param name="visibilityTimeout">Timeout the message stays as invisible. This message reappears in the queue after
      /// the interval expired. If not specified message will stay invisible for the timeout default to queue implementation
      /// which may be different from implementation to implementation.</param>
      /// <returns>Queued message or null if this queue has no visible messages</returns>
      QueueMessage GetMessage(TimeSpan? visibilityTimeout = null);

      /// <summary>
      /// Peeks the message on top of the queue without deleting it. Subsequent calls to this method should return the same
      /// message over and over again.
      /// </summary>
      /// <returns></returns>
      QueueMessage PeekMesssage();

      /// <summary>
      /// Deletes the message by ID
      /// </summary>
      /// <param name="id"></param>
      void DeleteMessage(string id);
      
      /// <summary>
      /// Clears all messages in the queue
      /// </summary>
      void Clear();
   }
}

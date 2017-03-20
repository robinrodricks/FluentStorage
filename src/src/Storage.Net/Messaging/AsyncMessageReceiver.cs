using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Storage.Net.Messaging
{
   /// <summary>
   /// Async helper
   /// </summary>
   public abstract class AsyncMessageReceiver : IMessageReceiver
   {
      public virtual void ConfirmMessage(QueueMessage message)
      {
         CallAsync(() => ConfirmMessageAsync(message));
      }

      public virtual Task ConfirmMessageAsync(QueueMessage message)
      {
         ConfirmMessage(message);
         return Task.FromResult(true);
      }

      public virtual void DeadLetter(QueueMessage message, string reason, string errorDescription)
      {
         CallAsync(() => DeadLetterAsync(message, reason, errorDescription));
      }

      public virtual Task DeadLetterAsync(QueueMessage message, string reason, string errorDescription)
      {
         DeadLetter(message, reason, errorDescription);
         return Task.FromResult(true);
      }

      public virtual void Dispose()
      {
      }

      public virtual IEnumerable<QueueMessage> ReceiveMessages(int count)
      {
         return CallAsync(() => ReceiveMessagesAsync(count));
      }

      public virtual Task<IEnumerable<QueueMessage>> ReceiveMessagesAsync(int count)
      {
         return Task.FromResult(ReceiveMessages(count));
      }

      public abstract void StartMessagePump(Action<QueueMessage> onMessage);

      private void CallAsync(Func<Task> lambda)
      {
         try
         {
            Task.Run(lambda).Wait();
         }
         catch (AggregateException ex)
         {
            throw ex.InnerException;
         }
      }

      private T CallAsync<T>(Func<Task<T>> lambda)
      {
         try
         {
            return Task.Run(lambda).Result;
         }
         catch (AggregateException ex)
         {
            throw ex.InnerException;
         }
      }

   }
}

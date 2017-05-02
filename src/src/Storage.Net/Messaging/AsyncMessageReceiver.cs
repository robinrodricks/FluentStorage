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
      /// <summary>
      /// Redirects to async version
      /// </summary>
      public virtual void ConfirmMessage(QueueMessage message)
      {
         CallAsync(() => ConfirmMessageAsync(message));
      }

      /// <summary>
      /// Redirects to blocking version
      /// </summary>
      public virtual Task ConfirmMessageAsync(QueueMessage message)
      {
         ConfirmMessage(message);
         return Task.FromResult(true);
      }

      /// <summary>
      /// Redirects to async version
      /// </summary>
      public virtual void DeadLetter(QueueMessage message, string reason, string errorDescription)
      {
         CallAsync(() => DeadLetterAsync(message, reason, errorDescription));
      }

      /// <summary>
      /// Redirects to blocking version
      /// </summary>
      public virtual Task DeadLetterAsync(QueueMessage message, string reason, string errorDescription)
      {
         DeadLetter(message, reason, errorDescription);
         return Task.FromResult(true);
      }

      /// <summary>
      /// Does nothing
      /// </summary>
      public virtual void Dispose()
      {
      }

      /// <summary>
      /// Redirects to async version
      /// </summary>
      public virtual IEnumerable<QueueMessage> ReceiveMessages(int count)
      {
         return CallAsync(() => ReceiveMessagesAsync(count));
      }

      /// <summary>
      /// Redirects to blocking version
      /// </summary>
      public virtual Task<IEnumerable<QueueMessage>> ReceiveMessagesAsync(int count)
      {
         return Task.FromResult(ReceiveMessages(count));
      }

      /// <summary>
      /// Redirects to async version
      /// </summary>
      public virtual void StartMessagePump(Action<QueueMessage> onMessage)
      {
         CallAsync(() => StartMessagePumpAsync((qm) =>
         {
            onMessage(qm);
            return Task.FromResult(true);
         }));
      }

      /// <summary>
      /// Redirects to blocking version
      /// </summary>
      public virtual Task StartMessagePumpAsync(Func<QueueMessage, Task> onMessageAsync)
      {
         StartMessagePump((qm) => onMessageAsync(qm));
         return Task.FromResult(true);
      }

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

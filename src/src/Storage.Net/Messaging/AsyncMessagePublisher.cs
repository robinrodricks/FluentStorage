using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Storage.Net.Messaging
{
   public abstract class AsyncMessagePublisher : IMessagePublisher
   {
      public virtual void Dispose()
      {
      }

      public virtual void PutMessages(IEnumerable<QueueMessage> messages)
      {
         CallAsync(() => PutMessagesAsync(messages));
      }

      public virtual Task PutMessagesAsync(IEnumerable<QueueMessage> messages)
      {
         PutMessages(messages);
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

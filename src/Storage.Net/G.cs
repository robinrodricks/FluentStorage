using System;
using System.Runtime.ExceptionServices;
using System.Threading.Tasks;

namespace Storage.Net
{
   static class G
   {
      public static void CallAsync(Func<Task> lambda)
      {
         try
         {
            Task.Run(lambda).Wait();
         }
         catch (AggregateException ex)
         {
            ExceptionDispatchInfo.Capture(ex.InnerException).Throw();
         }
      }

      public static T CallAsync<T>(Func<Task<T>> lambda)
      {
         try
         {
            return Task.Run(lambda).Result;
         }
         catch (AggregateException ex)
         {
            ExceptionDispatchInfo.Capture(ex.InnerException).Throw();
            return default(T);
         }
      }

   }
}

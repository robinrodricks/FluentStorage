using System;

namespace Storage.Net
{
   public static class GenericValidation
   {
      public static void CheckBlobPrefix(string prefix)
      {
         if (prefix == null) return;

         if (prefix.Length > 50)
            throw new ArgumentException(Exceptions.BlobPrefix_TooLong, nameof(prefix));
      }
   }
}

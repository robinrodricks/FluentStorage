using System;

namespace Storage.Net
{
   public static class GenericValidation
   {
      private const int MaxBlobIdLength = 50;
      private const int MaxBlobPrefixLength = 50;

      public static void CheckBlobPrefix(string prefix)
      {
         if (prefix == null) return;

         if (prefix.Length > MaxBlobPrefixLength)
            throw new ArgumentException(
               string.Format(Exceptions.BlobPrefix_TooLong, MaxBlobPrefixLength),
               nameof(prefix));
      }

      public static void CheckBlobId(string id)
      {
         if (id == null) throw new ArgumentNullException(nameof(id));

         if (id.Length > MaxBlobIdLength)
            throw new ArgumentException(string.Format(Exceptions.BlobId_TooLong, MaxBlobIdLength),
               nameof(id));
      }
   }
}

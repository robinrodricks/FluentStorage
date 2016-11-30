using System;

namespace Storage.Net
{
   /// <summary>
   /// A collection of generic library wise validations
   /// </summary>
   public static class GenericValidation
   {
      private const int MaxBlobIdLength = 50;
      private const int MaxBlobPrefixLength = 50;

      /// <summary>
      /// Validates blob prefix search
      /// </summary>
      /// <param name="prefix"></param>
      public static void CheckBlobPrefix(string prefix)
      {
         if (prefix == null) return;

         if (prefix.Length > MaxBlobPrefixLength)
            throw new ArgumentException(
               string.Format(Exceptions.BlobPrefix_TooLong, MaxBlobPrefixLength),
               nameof(prefix));
      }

      /// <summary>
      /// Validates blob ID
      /// </summary>
      /// <param name="id"></param>
      public static void CheckBlobId(string id)
      {
         if (id == null) throw new ArgumentNullException(nameof(id));

         if (id.Length > MaxBlobIdLength)
            throw new ArgumentException(string.Format(Exceptions.BlobId_TooLong, MaxBlobIdLength),
               nameof(id));
      }
   }
}

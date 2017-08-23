using System;
using System.Collections.Generic;
using System.IO;

namespace Storage.Net
{
   /// <summary>
   /// A collection of generic library wise validations
   /// </summary>
   public static class GenericValidation
   {
      private const int MaxBlobIdPartLength = 50;
      private const int MaxBlobPrefixLength = 50;

      /// <summary>
      /// Validates blob prefix search
      /// </summary>
      /// <param name="prefix"></param>
      public static void CheckBlobPrefix(string prefix)
      {
         if (prefix == null) return;

         string[] parts = prefix.Split('/');

         foreach (string part in parts)
         {
            if (part.Length > MaxBlobPrefixLength)
               throw new ArgumentException(
                  string.Format(Exceptions.BlobPrefix_TooLong, MaxBlobPrefixLength),
                  nameof(prefix));
         }
      }

      /// <summary>
      /// Validates blob ID
      /// </summary>
      /// <param name="id"></param>
      public static void CheckBlobId(string id)
      {
         if (id == null) throw new ArgumentNullException(nameof(id));

         string[] parts = id.Split('/');

         foreach (string part in parts)
         {
            if (part.Length > MaxBlobIdPartLength)
               throw new ArgumentException(string.Format(Exceptions.BlobId_TooLong, MaxBlobIdPartLength),
                  nameof(id));
         }
      }

      public static void CheckBlobId(IEnumerable<string> ids)
      {
         if (ids == null) return;

         foreach (string id in ids)
         {
            CheckBlobId(id);
         }
      }

      public static void CheckSourceStream(Stream inputStream)
      {
         if (inputStream == null) throw new ArgumentNullException(nameof(inputStream));

         try
         {
            var l = inputStream.Length;
         }
         catch(NotSupportedException ex)
         {
            throw new ArgumentException("stream must support getting a length", nameof(inputStream), ex);
         }

      }
   }
}

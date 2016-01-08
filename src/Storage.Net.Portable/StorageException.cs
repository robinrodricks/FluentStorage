using System;

namespace Storage.Net
{
   /// <summary>
   /// Generic storage exception
   /// </summary>
   public class StorageException : Exception
   {
      /// <summary>
      /// Creates a new instance of <see cref="StorageException"/>
      /// </summary>
      public StorageException()
      {
      }

      /// <summary>
      /// Creates a new instance of <see cref="StorageException"/> with exception message
      /// </summary>
      public StorageException(string message) : base(message)
      {
      }

      /// <summary>
      /// Creates a new instance of <see cref="StorageException"/> with exception message and inner exception
      /// </summary>
      public StorageException(string message, Exception inner) : base(message, inner)
      {
      }
   }
}

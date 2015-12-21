using System;

namespace Storage.Net
{
   public class StorageException : Exception
   {
      public StorageException()
      {
      }

      public StorageException(string message) : base(message)
      {
      }

      public StorageException(string message, Exception inner) : base(message, inner)
      {
      }
   }
}

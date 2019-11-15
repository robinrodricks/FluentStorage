using System;

namespace Storage.Net.Microsoft.Azure.Storage.Blobs
{
   /// <summary>
   /// Specifies the set of possible permissions for a shared access policy.
   /// </summary>
   [Flags]
   public enum BlobSasPermission
   {
      /// <summary>
      /// No shared access granted.
      /// </summary>
      None = 0x0,

      /// <summary>
      /// Read access granted.
      /// </summary>
      Read = 0x1,

      /// <summary>
      /// Write access granted.
      /// </summary>
      Write = 0x2,

      /// <summary>
      /// Delete access granted.
      /// </summary>
      Delete = 0x4,

      /// <summary>
      /// List access granted.
      /// </summary>
      List = 0x8,

      /// <summary>
      /// Add access granted.
      /// </summary>
      Add = 0x10,

      /// <summary>
      /// Create access granted.
      /// </summary>
      Create = 0x20
   }
}

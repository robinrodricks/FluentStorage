using System;
using System.Collections.Generic;
using System.Text;

namespace Storage.Net.Microsoft.Azure.Storage.Blobs
{
   /// <summary>
   /// Specifies possible permissions flags for container access policy
   /// </summary>
   [Flags]
   public enum ContainerSasPermission
   {
      /// <summary>
      /// Read access
      /// </summary>
      Read = 0x1,
      
      /// <summary>
      /// Write access granted
      /// </summary>
      Write = 0x2,

      /// <summary>
      /// Delete access granted
      /// </summary>
      Delete = 0x4,

      /// <summary>
      /// List access granted
      /// </summary>
      List = 0x8,

      /// <summary>
      /// Add access granted
      /// </summary>
      Add = 0x10,

      /// <summary>
      /// Create access granted
      /// </summary>
      Create = 0x20
   }
}

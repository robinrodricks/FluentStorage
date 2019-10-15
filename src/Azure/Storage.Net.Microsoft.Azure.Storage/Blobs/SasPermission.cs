using System;
using System.Collections.Generic;
using System.Text;

namespace Storage.Net.Microsoft.Azure.Storage.Blobs
{
   /// <summary>
   /// Specifies the set of possible permissions for a shared access account policy.
   /// </summary>
   [Flags]
   public enum SasPermission
   {
      /// <summary>
      /// On a container: read the content, properties, metadata or block list of any blob in the container. Use any blob in the container as the source of a copy operation.
      /// On a blob: read the content, properties, metadata and block list. Use the blob as the source of a copy operation.
      /// </summary>
      Read = 0x1,

      /// <summary>
      /// On a container: add a block to any append blob in the container.
      /// On a blob: add a block to an append blob.
      /// </summary>
      Add = 0x2,

      /// <summary>
      /// On a container: write a new blob to the container, snapshot any blob in the container, or copy a blob to a new blob in the container.
      /// On a blob: write a new blob, snapshot a blob, or copy a blob to a new blob.
      /// </summary>
      Create = 0x4,

      /// <summary>
      /// On a container: for any blob in the container, create or write content, properties, metadata, or block list. Snapshot or lease the blob. Resize the blob (page blob only). Use the blob as the destination of a copy operation. 
      /// On a blob: create or write content, properties, metadata, or block list. Snapshot or lease the blob. Resize the blob (page blob only). Use the blob as the destination of a copy operation.
      /// </summary>
      Write = 0x20,

      /// <summary>
      /// On a container: delete any blob in the container. Note: You cannot grant permissions to delete a container with a service SAS. Use an account SAS instead. For version 2017-07-29 and later, the Delete permission also allows breaking a lease on a container. See Lease Container for more information.
      /// On a blob: delete any blob, also allows breaking a lease on a blob.
      /// </summary>
      Delete = 0x40,

      /// <summary>
      /// On a container: list blobs in the container.
      /// On a blob: not applicable.
      /// </summary>
      List = 0x80
   }
}

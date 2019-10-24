using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Azure.Storage.Blob;

namespace Storage.Net.Microsoft.Azure.Storage.Blobs
{
   /// <summary>
   /// Defines a SAS policy for a blob
   /// </summary>
   public class BlobSasPolicy : OffsetSasPolicy
   {
      /// <summary>
      /// 
      /// </summary>
      /// <param name="startTime"></param>
      /// <param name="duration"></param>
      public BlobSasPolicy(DateTimeOffset? startTime, TimeSpan duration) : base(startTime, duration)
      {
      }

      /// <summary>
      /// 
      /// </summary>
      public BlobSasPolicy(TimeSpan duration) : base(null, duration)
      {

      }

      /// <summary>
      /// Permissions for this sas policy
      /// </summary>
      public BlobSasPermission Permissions { get; set; } = BlobSasPermission.Read;

      internal SharedAccessBlobPolicy ToSharedAccessBlobPolicy()
      {
         return new SharedAccessBlobPolicy
         {
            SharedAccessStartTime = StartTime,
            SharedAccessExpiryTime = ExpiryTime,
            Permissions = (SharedAccessBlobPermissions)(int)Permissions
         };
      }
   }
}

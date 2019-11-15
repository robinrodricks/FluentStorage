using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Azure.Storage.Blob;

namespace Storage.Net.Microsoft.Azure.Storage.Blobs
{
   /// <summary>
   /// Defines a SAS policy for a container
   /// </summary>
   public class ContainerSasPolicy : OffsetSasPolicy
   {
      /// <summary>
      /// 
      /// </summary>
      /// <param name="startTime"></param>
      /// <param name="duration"></param>
      public ContainerSasPolicy(DateTimeOffset? startTime, TimeSpan? duration) : base(startTime, duration)
      {
       
      }

      /// <summary>
      /// Permissions granted
      /// </summary>
      public ContainerSasPermission Permissions { get; set; } = ContainerSasPermission.List | ContainerSasPermission.Read;

      internal SharedAccessBlobPolicy ToSharedAccessBlobPolicy()
      {
         return new SharedAccessBlobPolicy
         {
            SharedAccessStartTime = StartTime,
            SharedAccessExpiryTime = StartTime + Duration,
            Permissions = (SharedAccessBlobPermissions)(int)Permissions
         };
      }
   }
}

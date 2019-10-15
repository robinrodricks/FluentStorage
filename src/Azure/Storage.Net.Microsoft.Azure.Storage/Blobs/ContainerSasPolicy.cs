using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Azure.Storage.Blob;

namespace Storage.Net.Microsoft.Azure.Storage.Blobs
{
   /// <summary>
   /// Defines a SAS policy for a container
   /// </summary>
   public class ContainerSasPolicy
   {
      /// <summary>
      /// 
      /// </summary>
      /// <param name="startTime"></param>
      /// <param name="duration"></param>
      public ContainerSasPolicy(DateTimeOffset startTime, TimeSpan duration)
      {
         if(duration.TotalSeconds < 0)
            throw new ArgumentException("duration cannot be negative", nameof(duration));

         StartTime = startTime;
         Duration = duration;
      }

      /// <summary>
      /// Time when this policy starts
      /// </summary>
      public DateTimeOffset? StartTime { get; set; }

      /// <summary>
      /// Total duration of the SAS policy
      /// </summary>
      public TimeSpan? Duration { get; set; }


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

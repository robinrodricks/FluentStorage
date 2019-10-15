using System;
using Microsoft.Azure.Storage;
using Microsoft.Azure.Storage.Blob;

namespace Storage.Net.Microsoft.Azure.Storage.Blobs
{
   /// <summary>
   /// Generic Shared Access Signature policy
   /// </summary>
   public class AccountSasPolicy
   {
      /// <summary>
      /// 
      /// </summary>
      /// <param name="startTime"></param>
      /// <param name="duration"></param>
      public AccountSasPolicy(DateTimeOffset startTime, TimeSpan duration)
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
      /// Permissions required.
      /// </summary>
      public AccountSasPermission Permissions { get; set; } = AccountSasPermission.List | AccountSasPermission.Read;

      internal SharedAccessAccountPolicy ToSharedAccessAccountPolicy()
      {
         return new SharedAccessAccountPolicy
         {
            SharedAccessStartTime = StartTime,
            SharedAccessExpiryTime = StartTime + Duration,
            Protocols = SharedAccessProtocol.HttpsOnly,
            Services = SharedAccessAccountServices.Blob,
            Permissions = (SharedAccessAccountPermissions)(int)Permissions,
            ResourceTypes =
               SharedAccessAccountResourceTypes.Container |
               SharedAccessAccountResourceTypes.Object |
               SharedAccessAccountResourceTypes.Service
         };
      }
   }
}

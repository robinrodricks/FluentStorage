using System;
using Microsoft.Azure.Storage;

namespace Storage.Net.Microsoft.Azure.Storage.Blobs
{
   /// <summary>
   /// Generic Shared Access Signature policy
   /// </summary>
   public class AccountSasPolicy : OffsetSasPolicy
   {
      /// <summary>
      /// 
      /// </summary>
      /// <param name="startTime"></param>
      /// <param name="duration"></param>
      public AccountSasPolicy(DateTimeOffset? startTime, TimeSpan? duration) : base(startTime, duration)
      {
      }

      /// <summary>
      /// Permissions required.
      /// </summary>
      public AccountSasPermission Permissions { get; set; } = AccountSasPermission.List | AccountSasPermission.Read;

      internal SharedAccessAccountPolicy ToSharedAccessAccountPolicy()
      {
         return new SharedAccessAccountPolicy
         {
            SharedAccessStartTime = StartTime,
            SharedAccessExpiryTime = ExpiryTime,
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

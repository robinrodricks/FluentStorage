using System;
using System.Collections.Generic;
using System.Text;
using Azure.Storage;
using Azure.Storage.Sas;

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
      public AccountSasPolicy(DateTimeOffset startTime, TimeSpan duration) : base(startTime, duration)
      {
      }

      /// <summary>
      /// Permissions required.
      /// </summary>
      public AccountSasPermission Permissions { get; set; } = AccountSasPermission.List | AccountSasPermission.Read;

      internal string ToSasQuery(StorageSharedKeyCredential sharedKeyCredential)
      {
         if(sharedKeyCredential is null)
            throw new ArgumentNullException(nameof(sharedKeyCredential));
         var sas = new AccountSasBuilder
         {
            Services = AccountSasServices.Blobs,
            Protocol = SasProtocol.Https,
            StartsOn = StartTime,
            ExpiresOn = ExpiryTime,
            ResourceTypes = AccountSasResourceTypes.All
         };

         sas.SetPermissions((AccountSasPermissions)(int)Permissions);

         string query = sas.ToSasQueryParameters(sharedKeyCredential).ToString();
         return query;
      }

      /*internal SharedAccessAccountPolicy ToSharedAccessAccountPolicy()
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
      }*/
   }
}

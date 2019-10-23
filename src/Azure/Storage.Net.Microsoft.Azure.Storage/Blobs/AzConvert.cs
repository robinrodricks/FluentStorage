using System;
using Microsoft.Azure.Storage.Blob;
using Microsoft.Azure.Storage.Blob.Protocol;
using Microsoft.Azure.Storage.File;
using NetBox.Extensions;
using Storage.Net.Blobs;

namespace Storage.Net.Microsoft.Azure.Storage.Blobs
{
   static class AzConvert
   {
      public static Blob ToBlob(CloudFileShare share)
      {
         var blob = new Blob(share.Name, BlobItemKind.Folder);
         blob.TryAddProperties(
            "IsSnapshot", share.IsSnapshot.ToString(),
            "ETag", share.Properties.ETag,
            "LastModified", share.Properties.LastModified?.ToString(),
            "Quota", share.Properties.Quota?.ToString(),
            "SnapshotTime", share.SnapshotTime?.ToString(),
            "IsShare", "True");
         blob.Metadata.MergeRange(share.Metadata);
         return blob;
      }

      public static Blob ToBlob(string path, IListFileItem item)
      {
         if(item is CloudFile file)
         {
            var blob = new Blob(path, file.Name, BlobItemKind.File)
            {
               LastModificationTime = file.Properties.LastWriteTime,
               Size = file.Properties.Length,
               MD5 = file.Properties.ContentMD5
            };
            blob.TryAddProperties(
               "CopyState", file.CopyState?.ToString(),
               "ChangeTime", file.Properties.ChangeTime?.ToString(),
               "ContentType", file.Properties.ContentType,
               "CreationTime", file.Properties.CreationTime?.ToString(),
               "ETag", file.Properties.ETag,
               "IsServerEncrypted", file.Properties.IsServerEncrypted.ToString(),
               "LastModified", file.Properties.LastModified?.ToString(),
               "NtfsAttributes", file.Properties.NtfsAttributes?.ToString());
            blob.Metadata.MergeRange(file.Metadata);
            return blob;
         }
         else if(item is CloudFileDirectory dir)
         {
            var blob = new Blob(path, dir.Name, BlobItemKind.Folder)
            {
               LastModificationTime = dir.Properties.LastWriteTime
            };
            blob.TryAddProperties(
               "ChangeTime", dir.Properties.ChangeTime?.ToString(),
               "CreationTime", dir.Properties.CreationTime?.ToString(),
               "ETag", dir.Properties.ETag,
               "IsServerEncrypted", dir.Properties.IsServerEncrypted.ToString(),
               "LastModified", dir.Properties.LastModified?.ToString(),
               "NtfsAttributes", dir.Properties.NtfsAttributes?.ToString());
            blob.Metadata.MergeRange(dir.Metadata);
            return blob;
         }
         else
         {
            throw new NotSupportedException($"don't know '{item.GetType()}' object type");
         }
      }

      public static Blob ToBlob(CloudBlobContainer container)
      {
         var blob = new Blob(container.Name, BlobItemKind.Folder);
         blob.Properties["IsContainer"] = "True";
         
         if(container.Properties != null)
         {
            BlobContainerProperties props = container.Properties;
            if(props.ETag != null)
               blob.Properties["ETag"] = props.ETag;
            if(props.HasImmutabilityPolicy != null)
               blob.Properties["HasImmutabilityPolicy"] = props.HasImmutabilityPolicy.ToString();
            if(props.HasLegalHold != null)
               blob.Properties["HasLegalHold"] = props.HasLegalHold.ToString();
            blob.Properties["LeaseDuration"] = props.LeaseDuration.ToString();
            blob.Properties["LeaseState"] = props.LeaseState.ToString();
            blob.Properties["LeaseStatus"] = props.LeaseStatus.ToString();
            if(props.PublicAccess != null)
               blob.Properties["PublicAccess"] = props.PublicAccess.ToString();
         }

         if(container.Metadata?.Count > 0)
         {
            blob.Metadata.MergeRange(container.Metadata);
         }
         
         return blob;
      }

      public static Blob ToBlob(string containerName, IListBlobItem nativeBlob)
      {
         string GetFullName(string name) => containerName == null
               ? name
               : StoragePath.Combine(containerName, name);

         Blob blob = nativeBlob switch
         {
            CloudBlockBlob blockBlob => new Blob(GetFullName(blockBlob.Name), BlobItemKind.File),
            CloudAppendBlob appendBlob => new Blob(GetFullName(appendBlob.Name), BlobItemKind.File),
            CloudBlob cloudBlob => new Blob(GetFullName(cloudBlob.Name), BlobItemKind.File),
            CloudBlobDirectory dirBlob => new Blob(GetFullName(dirBlob.Prefix), BlobItemKind.Folder),
            _ => throw new InvalidOperationException($"unknown item type {nativeBlob.GetType()}")
         };

         //attach metadata if we can
         if(nativeBlob is CloudBlob metaBlob)
         {
            //no need to fetch attributes, parent request includes the details
            //await cloudBlob.FetchAttributesAsync().ConfigureAwait(false);
            AzConvert.AttachBlobMeta(blob, metaBlob);
         }

         return blob;
      }

      public static void AttachBlobMeta(Blob destination, CloudBlob source)
      {
         //ContentMD5 is base64-encoded hash, whereas we work with HEX encoded ones
         destination.MD5 = source.Properties.ContentMD5.Base64DecodeAsBytes().ToHexString();
         destination.Size = source.Properties.Length;
         destination.LastModificationTime = source.Properties.LastModified;

         string blobType = source.BlobType.ToString();
         if(blobType.EndsWith("Blob"))
            blobType = blobType.Substring(0, blobType.Length - 4).ToLower();

         destination.TryAddProperties(
            "BlobType", blobType,
            "IsDeleted", source.IsDeleted.ToString());

         if(source.Properties != null)
         {
            BlobProperties props = source.Properties;
            if(props.ContentDisposition != null)
               destination.Properties["ContentDisposition"] = props.ContentDisposition;
            if(props.ContentEncoding != null)
               destination.Properties["ContentEncoding"] = props.ContentEncoding;
            if(props.ContentLanguage != null)
               destination.Properties["ContentLanguage"] = props.ContentLanguage;
            if(props.ContentType != null)
               destination.Properties["ContentType"] = props.ContentType;
            if(props.ContentMD5 != null)
               destination.Properties["ContentMD5"] = props.ContentMD5;
            if(props.ETag != null)
               destination.Properties["ETag"] = props.ETag;
            destination.Properties["IsIncrementalCopy"] = props.IsIncrementalCopy.ToString();
            destination.Properties["IsServerEncrypted"] = props.IsServerEncrypted.ToString();
            destination.Properties["LeaseDuration"] = props.LeaseDuration.ToString();
            destination.Properties["LeaseState"] = props.LeaseState.ToString();
            destination.Properties["LeaseStatus"] = props.LeaseStatus.ToString();
            if(props.RehydrationStatus != null)
               destination.Properties["RehydrationStatus"] = props.RehydrationStatus.Value.ToString();
            if(props.RemainingDaysBeforePermanentDelete != null)
               destination.Properties["RemainingDaysBeforePermanentDelete"] = props.RemainingDaysBeforePermanentDelete.Value.ToString();
            if(props.StandardBlobTier != null)
               destination.Properties["StandardBlobTier"] = props.StandardBlobTier.ToString();
            if(props.PremiumPageBlobTier != null)
               destination.Properties["PremiumPageBlobTier"] = props.PremiumPageBlobTier.ToString();

         }

         if(source.Metadata?.Count > 0)
         {
            destination.Metadata.MergeRange(source.Metadata);
         }
      }
   }
}

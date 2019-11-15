using System;
using System.Collections.Generic;
using System.Text;
using Azure;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using NetBox.Extensions;
using Storage.Net.Blobs;

namespace Storage.Net.Microsoft.Azure.Storage.Blobs
{
   static class AzConvert
   {
      public static Blob ToBlob(BlobContainerItem item)
      {
         var blob = new Blob(item.Name, BlobItemKind.Folder);
         blob.TryAddProperties(
            "IsContainer", true);

         return blob;
      }

      public static Blob ToBlob(BlobContainerClient client)
      {
         var blob = new Blob(client.Name, BlobItemKind.Folder);

         return blob;
      }

      public static Blob ToBlob(string name, Response<BlobContainerProperties> properties)
      {
         return ToBlob(name, properties.Value);
      }

      public static Blob ToBlob(string name, BlobContainerProperties properties)
      {
         var blob = new Blob(name, BlobItemKind.Folder);
         blob.LastModificationTime = properties.LastModified;

         blob.TryAddProperties(
            "IsContainer", true,
            "ETag", properties.ETag,
            "HasImmutabilityPolicy", properties.HasImmutabilityPolicy,
            "HasLegalHold", properties.HasLegalHold,
            "LastModified", properties.LastModified,
            "LeaseDuration", properties.LeaseDuration,
            "LeaseState", properties.LeaseState,
            "LeaseStatus", properties.LeaseStatus,
            "PublicAccess", properties.PublicAccess);

         blob.Metadata.MergeRange(properties.Metadata);

         return blob;
      }

      public static Blob ToBlob(string containerName, BlobHierarchyItem bhi)
      {
         string GetFullName(string name) => containerName == null
            ? name
            : StoragePath.Combine(containerName, name);

         if(bhi.IsBlob)
         {
            var blob = new Blob(GetFullName(bhi.Blob.Name), BlobItemKind.File);
            blob.MD5 = bhi.Blob.Properties.ContentHash.ToHexString();
            blob.Size = bhi.Blob.Properties.ContentLength;
            blob.LastModificationTime = bhi.Blob.Properties.LastModified;

            AddProperties(blob, bhi.Blob.Properties);
            blob.Metadata.MergeRange(bhi.Blob.Metadata);

            return blob;
         }

         if(bhi.IsPrefix)
         {
            var blob = new Blob(GetFullName(bhi.Prefix), BlobItemKind.Folder);
            //nothing else we know about prefix
            return blob;
         }

         throw new NotImplementedException();
      }

      public static Blob ToBlob(string containerName, string path, Response<BlobProperties> properties)
      {
         return ToBlob(containerName, path, properties.Value);
      }

      public static Blob ToBlob(string containerName, string path, BlobProperties properties)
      {
         string GetFullName(string name) => containerName == null
            ? name
            : StoragePath.Combine(containerName, name);

         var blob = new Blob(GetFullName(path), BlobItemKind.File);
         blob.MD5 = properties.ContentHash.ToHexString();
         blob.Size = properties.ContentLength;
         blob.LastModificationTime = properties.LastModified;

         AddProperties(blob, properties);

         blob.Metadata.MergeRange(properties.Metadata);

         return blob;
      }

      private static void AddProperties(Blob blob, BlobItemProperties properties)
      {
         blob.TryAddProperties(
            "CustomerProvidedKeySha256", properties.CustomerProvidedKeySha256,
            "IncrementalCopy", properties.IncrementalCopy,
            "ServerEncrypted", properties.ServerEncrypted,
            "DeletedOn", properties.DeletedOn,
            "RemainingRetentionDays", properties.RemainingRetentionDays,
            "AccessTier", properties.AccessTier,
            "AccessTierChangedOn", properties.AccessTierChangedOn,
            "AccessTierInferred", properties.AccessTierInferred,
            "ArchiveStatus", properties.ArchiveStatus,
            "BlobSequenceNumber", properties.BlobSequenceNumber,
            "BlobType", properties.BlobType,
            "CacheControl", properties.CacheControl,
            "ContentDisposition", properties.ContentDisposition,
            "ContentEncoding", properties.ContentEncoding,
            "ContentHash", properties.ContentHash,
            "ContentLanguage", properties.ContentLanguage,
            "ContentLength", properties.ContentLength,
            "ContentType", properties.ContentType,
            "CopyCompletedOn", properties.CopyCompletedOn,
            "CopyId", properties.CopyId,
            "CopyProgress", properties.CopyProgress,
            "CopySource", properties.CopySource,
            "CopyStatus", properties.CopyStatus,
            "CopyStatusDescription", properties.CopyStatusDescription,
            "CreatedOn", properties.CreatedOn,
            "DestinationSnapshot", properties.DestinationSnapshot,
            "ETag", properties.ETag,
            "LastModified", properties.LastModified,
            "LeaseDuration", properties.LeaseDuration,
            "LeaseState", properties.LeaseState,
            "LeaseStatus", properties.LeaseStatus);
      }

      private static void AddProperties(Blob blob, BlobProperties properties)
      {
         blob.TryAddProperties(
            "AcceptRanges", properties.AcceptRanges,
            "AccessTier", properties.AccessTier,
            "AccessTierChangedOn", properties.AccessTierChangedOn,
            "AccessTierInferred", properties.AccessTierInferred,
            "ArchiveStatus", properties.ArchiveStatus,
            "BlobCommittedBlockCount", properties.BlobCommittedBlockCount,
            "BlobSequenceNumber", properties.BlobSequenceNumber,
            "BlobType", properties.BlobType,
            "CacheControl", properties.CacheControl,
            "ContentDisposition", properties.ContentDisposition,
            "ContentEncoding", properties.ContentEncoding,
            "ContentHash", properties.ContentHash,
            "ContentLanguage", properties.ContentLanguage,
            "ContentLength", properties.ContentLength,
            "ContentType", properties.ContentType,
            "CopyCompletedOn", properties.CopyCompletedOn,
            "CopyId", properties.CopyId,
            "CopyProgress", properties.CopyProgress,
            "CopySource", properties.CopySource,
            "CopyStatus", properties.CopyStatus,
            "CopyStatusDescription", properties.CopyStatusDescription,
            "CreatedOn", properties.CreatedOn,
            "DestinationSnapshot", properties.DestinationSnapshot,
            "EncryptionKeySha256", properties.EncryptionKeySha256,
            "ETag", properties.ETag,
            "IsIncrementalCopy", properties.IsIncrementalCopy,
            "IsServerEncrypted", properties.IsServerEncrypted,
            "LastModified", properties.LastModified,
            "LeaseDuration", properties.LeaseDuration,
            "LeaseState", properties.LeaseState,
            "LeaseStatus", properties.LeaseStatus);
      }
   }
}

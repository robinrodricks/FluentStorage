using System;
using System.Collections.Generic;
using System.Text;
using Azure.Storage.Blobs.Models;
using Storage.Net.Blobs;

namespace Storage.Net.Microsoft.Azure.Storage.Blobs
{
   static class AzConvert
   {
      public static Blob ToBlob(BlobContainerItem item)
      {
         var blob = new Blob(item.Name, BlobItemKind.Folder);
         blob.TryAddProperties(
            "IsContianer", "True");

         return blob;
      }
   }
}

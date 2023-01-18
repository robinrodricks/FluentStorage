using System;
using System.Collections.Generic;
using System.Text;
using FluentStorage.Blobs;
using FileInfo = Microsoft.Azure.Databricks.Client.FileInfo;

namespace FluentStorage.Databricks
{
   static class DConvert
   {
      public static Blob ToBlob(FileInfo fi)
      {
         return new Blob(
            fi.Path,
            fi.IsDirectory ? BlobItemKind.Folder : BlobItemKind.File)
         {
            Size = fi.IsDirectory ? null : (long?)fi.FileSize
         };
      }
   }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Storage.Net.Blob;
using Storage.Net.ZipFile;

namespace Storage.Net
{
   /// <summary>
   /// Factory helpers
   /// </summary>
   public static class Factory
   {
      public static IBlobStorageProvider ZipFile(this IBlobStorageFactory blobStorageFactory, string filePath)
      {
         return new ZipFileBlobStorageProvider(filePath);
      }
   }
}

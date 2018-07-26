using System;
using System.Collections.Generic;
using System.IO;
using Storage.Net.Blob;
using Storage.Net.Blob.Files;

namespace Storage.Net.ConnectionString
{
   class BuiltInConnectionFactory : IConnectionFactory
   {
      public IBlobStorage CreateBlobStorage(StorageConnectionString connectionString)
      {
         if(connectionString.Prefix == "disk")
         {
            connectionString.GetRequired("path", true, out string path);

            return new DiskDirectoryBlobStorage(new DirectoryInfo(path));
         }

         if(connectionString.Prefix == "inmemory")
         {
            return new InMemoryBlobStorage();
         }

         return null;
      }
   }
}

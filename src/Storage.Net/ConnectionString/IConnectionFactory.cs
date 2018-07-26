using System;
using System.Collections.Generic;
using System.Text;
using Storage.Net.Blob;

namespace Storage.Net.ConnectionString
{
   public interface IConnectionFactory
   {
      IBlobStorage CreateBlobStorage(StorageConnectionString connectionString);
   }
}

/*using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Storage.Net.Blob;
using Xunit;

namespace Storage.Net.Tests.Integration
{
   public class TempTest
   {
      [Fact]
      public async Task Discovery()
      {
         ServicePointManager.DefaultConnectionLimit = 100;

         StorageFactory.Modules.UseAzureDataLakeStore();

         IBlobStorage adls = StorageFactory.Blobs.FromConnectionString("");

         IEnumerable<BlobId> all = await adls.ListAsync(new ListOptions { Recurse = true });

      }
   }
}
*/
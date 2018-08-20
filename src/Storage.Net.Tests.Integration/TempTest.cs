using System;
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

         IBlobStorage adls = StorageFactory.Blobs.FromConnectionString("azure.datalakestore://accountname=ecdllake;tenantid=4af8322c-80ee-4819-a9ce-863d5afbea1c;principalId=6814c3a9-39dc-4e2b-9d84-e81bf3d6130c;principalSecret=w/UgKNEy+Qw9aNZRJCt+i3SI/gyUdwSULpSlCn+1Wog=;listBatchSize=10");

         IEnumerable<BlobId> all = await adls.ListAsync(
            new ListOptions
            {
               Recurse = true,
               IncludeMetaWhenKnown = true,
               FolderPath = "/perftest/archiving/F000000"
            });

      }
   }
}
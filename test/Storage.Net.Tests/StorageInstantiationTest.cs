using Storage.Net.Blobs;
using Storage.Net.Blobs.Files;
using Storage.Net.Tests.Integration;
using Xunit;

namespace Storage.Net.Tests
{
   public class StorageInstantiationTest : AbstractTestFixture
   {
      public StorageInstantiationTest()
      {
         StorageFactory.Modules.UseAzureStorage();
      }

      [Fact]
      public void Disk_storage_creates_from_connection_string()
      {
         IBlobStorage disk = StorageFactory.Blobs.FromConnectionString("disk://path=" + TestDir.FullName);

         Assert.IsType<DiskDirectoryBlobStorage>(disk);

         DiskDirectoryBlobStorage ddbs = (DiskDirectoryBlobStorage)disk;

         Assert.Equal(TestDir.FullName, ddbs.RootDirectory.FullName);
      }
   }
}

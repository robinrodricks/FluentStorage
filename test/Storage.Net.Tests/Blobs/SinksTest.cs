using System.Threading.Tasks;
using Storage.Net.Blobs;
using Xunit;

namespace Storage.Net.Tests.Blobs
{
   public class SinksTest
   {
      private readonly IBlobStorage storage = StorageFactory.Blobs.InMemory();

      [Fact]
      public async Task Gzip()
      {
         IBlobStorage zipStorage = storage.WithGzipCompression();

         await zipStorage.WriteTextAsync("1.gzip", "it's a gzipped text");

         Assert.Equal("it's a gzipped text", await zipStorage.ReadTextAsync("1.gzip"));
      }

      [Fact]
      public async Task SymmetricEncryption()
      {
         IBlobStorage enc = storage.WithSymmetricEncryption("6qg/7EgPmrK9ZY70pnECtZ40g3dDe74czSvWJ+3dj0A=");

         await enc.WriteTextAsync("1.senc", "encrypted?");

         Assert.Equal("encrypted?", await enc.ReadTextAsync("1.senc"));
      }
   }
}

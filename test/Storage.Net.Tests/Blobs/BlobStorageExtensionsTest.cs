using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Storage.Net.Blobs;
using Xunit;

namespace Storage.Net.Tests.Blobs
{
   public class BlobStorageExtensionsTest
   {
      private readonly IBlobStorage _storage = StorageFactory.Blobs.InMemory();

      [Fact]
      public async Task Write_object()
      {
         var input = new MyClass { Name = "test" };

         await _storage.WriteObjectAsync("1.json", input);

         MyClass output = await _storage.ReadObjectAsync<MyClass>("1.json");

         Assert.Equal("test", output.Name);
      }

      private class MyClass
      {
         public string Name { get; set; }
      }
   }
}

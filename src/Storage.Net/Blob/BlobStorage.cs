using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Storage.Net.Blob
{
   public class BlobStorage
   {
      private readonly IBlobStorageProvider _provider;

      public BlobStorage(IBlobStorageProvider provider)
      {
         _provider = provider ?? throw new ArgumentNullException(nameof(provider));
      }

      public async Task WriteTextAsync(string fullPath, string content)
      {
         throw new NotImplementedException();
      }

      public async Task<string> ReadTextAsync(string fullPath)
      {
         throw new NotImplementedException();
      }

      public async Task DeleteAsync(string fullPath)
      {
         throw new NotImplementedException();
      }
   }
}

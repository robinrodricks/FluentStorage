using Microsoft.ServiceFabric.Data;
using Storage.Net.Blob;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace Storage.Net.Microsoft.ServiceFabric.Blob
{
   class ServiceFabricReliableDictionaryBlobStorage : AsyncBlobStorage
   {
      public ServiceFabricReliableDictionaryBlobStorage(IReliableStateManager stateManager, string collectionName)
      {

      }

      public override Task AppendFromStreamAsync(string id, Stream chunkStream)
      {


         return base.AppendFromStreamAsync(id, chunkStream);
      }

      public override Task DeleteAsync(string id)
      {
         return base.DeleteAsync(id);
      }

      public override Task DownloadToStreamAsync(string id, Stream targetStream)
      {
         return base.DownloadToStreamAsync(id, targetStream);
      }

      public override Task<bool> ExistsAsync(string id)
      {
         return base.ExistsAsync(id);
      }

      public override Task<BlobMeta> GetMetaAsync(string id)
      {
         return base.GetMetaAsync(id);
      }

      public override Task<IEnumerable<string>> ListAsync(string prefix)
      {
         return base.ListAsync(prefix);
      }

      public override Task<Stream> OpenStreamToReadAsync(string id)
      {
         return base.OpenStreamToReadAsync(id);
      }

      public override Task UploadFromStreamAsync(string id, Stream sourceStream)
      {
         return base.UploadFromStreamAsync(id, sourceStream);
      }


   }
}

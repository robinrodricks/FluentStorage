using Microsoft.ServiceFabric.Data;
using Storage.Net.Blob;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Microsoft.ServiceFabric.Data.Collections;

namespace Storage.Net.Microsoft.ServiceFabric.Blob
{
   class ServiceFabricReliableDictionaryBlobStorage : AsyncBlobStorage
   {
      private IReliableStateManager _stateManager;
      private readonly string _collectionName;

      public ServiceFabricReliableDictionaryBlobStorage(IReliableStateManager stateManager, string collectionName)
      {
         _stateManager = stateManager ?? throw new ArgumentNullException(nameof(stateManager));
         _collectionName = collectionName ?? throw new ArgumentNullException(nameof(collectionName));
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

      public override async Task<Stream> OpenStreamToReadAsync(string id)
      {
         var collection = await _stateManager.GetOrAddAsync<IReliableDictionary<string, byte[]>>(_collectionName);

         using (var tx = new FabricTransactionManager(_stateManager))
         {
            ConditionalValue<byte[]> value = await collection.TryGetValueAsync(tx.Tx, id);

            if (!value.HasValue) throw new StorageException(ErrorCode.NotFound, null);

            return new MemoryStream(value.Value);
         }
      }

      public override async Task UploadFromStreamAsync(string id, Stream sourceStream)
      {
         var collection = await _stateManager.GetOrAddAsync<IReliableDictionary<string, byte[]>>(_collectionName);

         byte[] value = sourceStream.ToByteArray();

         using (var tx = new FabricTransactionManager(_stateManager))
         {
            await collection.AddOrUpdateAsync(tx.Tx, id, value, (k, v) => value);

            await tx.Commit();
         }
      }
   }
}

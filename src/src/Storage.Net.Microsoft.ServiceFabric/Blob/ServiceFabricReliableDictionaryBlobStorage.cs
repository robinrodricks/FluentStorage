using Microsoft.ServiceFabric.Data;
using Microsoft.ServiceFabric.Data.Collections;
using Storage.Net.Blob;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

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

      public override async Task DeleteAsync(string id)
      {
         using (var tx = await OpenCollection())
         {
            await tx.Collection.TryRemoveAsync(tx.Tx, id);

            await tx.CommitAsync();
         }
      }

      public override async Task DownloadToStreamAsync(string id, Stream targetStream)
      {
         using (var tx = await OpenCollection())
         {
            ConditionalValue<byte[]> value = await tx.Collection.TryGetValueAsync(tx.Tx, id);

            if (!value.HasValue) throw new StorageException(ErrorCode.NotFound, null);

            using (var source = new MemoryStream(value.Value))
            {
               await source.CopyToAsync(targetStream);
            }
         }
      }

      public override async Task<bool> ExistsAsync(string id)
      {
         using (var tx = await OpenCollection())
         {
            return await tx.Collection.ContainsKeyAsync(tx.Tx, id);
         }
      }

      public override Task<BlobMeta> GetMetaAsync(string id)
      {
         return base.GetMetaAsync(id);
      }

      public override async Task<IEnumerable<string>> ListAsync(string prefix)
      {
         var result = new List<string>();

         using (var tx = await OpenCollection())
         {
            IAsyncEnumerable<KeyValuePair<string, byte[]>> enumerable =
               await tx.Collection.CreateEnumerableAsync(tx.Tx);

            using (IAsyncEnumerator<KeyValuePair<string, byte[]>> enumerator = enumerable.GetAsyncEnumerator())
            {
               while (await enumerator.MoveNextAsync(CancellationToken.None))
               {
                  KeyValuePair<string, byte[]> current = enumerator.Current;

                  if (prefix == null || current.Key.StartsWith(prefix))
                  {
                     result.Add(current.Key);
                  }
               }
            }
         }

         return result;
      }

      public override async Task<Stream> OpenStreamToReadAsync(string id)
      {
         using (var tx = await OpenCollection())
         {
            ConditionalValue<byte[]> value = await tx.Collection.TryGetValueAsync(tx.Tx, id);

            if (!value.HasValue) throw new StorageException(ErrorCode.NotFound, null);

            return new MemoryStream(value.Value);
         }
      }

      public override async Task UploadFromStreamAsync(string id, Stream sourceStream)
      {
         byte[] value = sourceStream.ToByteArray();

         using (var tx = await OpenCollection())
         {
            await tx.Collection.AddOrUpdateAsync(tx.Tx, id, value, (k, v) => value);

            await tx.CommitAsync();
         }
      }

      private async Task<FabricTransactionManager<IReliableDictionary<string, byte[]>>> OpenCollection()
      {
         var collection = await _stateManager.GetOrAddAsync<IReliableDictionary<string, byte[]>>(_collectionName);

         return new FabricTransactionManager<IReliableDictionary<string, byte[]>>(_stateManager, collection);
      }
   }
}

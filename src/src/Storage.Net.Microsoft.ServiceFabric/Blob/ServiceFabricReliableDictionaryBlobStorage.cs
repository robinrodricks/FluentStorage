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

      public override async Task AppendAsync(string id, Stream sourceStream)
      {
         using (var tx = await OpenCollection())
         {
            //create a new byte array with
            byte[] extra = sourceStream.ToByteArray();
            ConditionalValue<byte[]> value = await tx.Collection.TryGetValueAsync(tx.Tx, id);
            int oldLength = value.HasValue ? value.Value.Length : 0;
            byte[] newData = new byte[oldLength + extra.Length];
            if(value.HasValue)
            {
               Array.Copy(value.Value, newData, oldLength);
            }
            Array.Copy(extra, 0, newData, oldLength, extra.Length);

            //put new array into the key
            await tx.Collection.AddOrUpdateAsync(tx.Tx, id, extra, (k, v) => extra);

            //commit the transaction
            await tx.CommitAsync();
         }
      }

      public override async Task DeleteAsync(string id)
      {
         using (var tx = await OpenCollection())
         {
            await tx.Collection.TryRemoveAsync(tx.Tx, id);

            await tx.CommitAsync();
         }
      }

      public override async Task<bool> ExistsAsync(string id)
      {
         using (var tx = await OpenCollection())
         {
            return await tx.Collection.ContainsKeyAsync(tx.Tx, id);
         }
      }

      public override async Task<BlobMeta> GetMetaAsync(string id)
      {
         using (var tx = await OpenCollection())
         {
            ConditionalValue<byte[]> value = await tx.Collection.TryGetValueAsync(tx.Tx, id);

            if (!value.HasValue) return null;

            return new BlobMeta(value.Value.Length, null);
         }
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

      public override async Task<Stream> OpenReadAsync(string id)
      {
         using (var tx = await OpenCollection())
         {
            ConditionalValue<byte[]> value = await tx.Collection.TryGetValueAsync(tx.Tx, id);

            if (!value.HasValue) throw new StorageException(ErrorCode.NotFound, null);

            return new MemoryStream(value.Value);
         }
      }

      public override async Task WriteAsync(string id, Stream sourceStream)
      {
         throw new NotImplementedException();

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

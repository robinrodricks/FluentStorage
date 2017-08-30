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
   class ServiceFabricReliableDictionaryBlobStorageProvider : IBlobStorageProvider
   {
      private IReliableStateManager _stateManager;
      private readonly string _collectionName;

      public ServiceFabricReliableDictionaryBlobStorageProvider(IReliableStateManager stateManager, string collectionName)
      {
         _stateManager = stateManager ?? throw new ArgumentNullException(nameof(stateManager));
         _collectionName = collectionName ?? throw new ArgumentNullException(nameof(collectionName));
      }

      public async Task<IEnumerable<BlobId>> ListAsync(string folderPath, string prefix, bool recurse, CancellationToken cancellationToken)
      {
         var result = new List<BlobId>();

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
                     result.Add(new BlobId(null, current.Key, BlobItemKind.File));
                  }
               }
            }
         }

         return result;
      }

      public async Task WriteAsync(string id, Stream sourceStream, bool append)
      {
         if (append)
         {
            await AppendAsync(id, sourceStream);
         }
         else
         {
            await WriteAsync(id, sourceStream);
         }
      }

      private async Task WriteAsync(string id, Stream sourceStream)
      {
         byte[] value = sourceStream.ToByteArray();

         using (var tx = await OpenCollection())
         {
            await tx.Collection.AddOrUpdateAsync(tx.Tx, id, value, (k, v) => value);

            await tx.CommitAsync();
         }
      }

      private async Task AppendAsync(string id, Stream sourceStream)
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

      public async Task<Stream> OpenReadAsync(string id)
      {
         using (var tx = await OpenCollection())
         {
            ConditionalValue<byte[]> value = await tx.Collection.TryGetValueAsync(tx.Tx, id);

            if (!value.HasValue) throw new StorageException(ErrorCode.NotFound, null);

            return new MemoryStream(value.Value);
         }
      }

      public async Task DeleteAsync(IEnumerable<string> ids)
      {
         using (var tx = await OpenCollection())
         {
            foreach (string id in ids)
            {
               await tx.Collection.TryRemoveAsync(tx.Tx, id);
            }

            await tx.CommitAsync();
         }
      }

      public async Task<IEnumerable<bool>> ExistsAsync(IEnumerable<string> ids)
      {
         var result = new List<bool>();
         using (var tx = await OpenCollection())
         {
            foreach (string id in ids)
            {
               bool exists = await tx.Collection.ContainsKeyAsync(tx.Tx, id);

               result.Add(exists);
            }
         }
         return result;
      }

      public async Task<IEnumerable<BlobMeta>> GetMetaAsync(IEnumerable<string> ids)
      {
         GenericValidation.CheckBlobId(ids);

         var result = new List<BlobMeta>();

         using (var tx = await OpenCollection())
         {
            foreach (string id in ids)
            {
               ConditionalValue<byte[]> value = await tx.Collection.TryGetValueAsync(tx.Tx, id);

               if (!value.HasValue)
               {
                  result.Add(null);
               }
               else
               {
                  var meta = new BlobMeta(value.Value.Length, null);
                  result.Add(meta);
               }
            }
         }
         return result;
      }

      private async Task<FabricTransactionManager<IReliableDictionary<string, byte[]>>> OpenCollection()
      {
         var collection = await _stateManager.GetOrAddAsync<IReliableDictionary<string, byte[]>>(_collectionName);

         return new FabricTransactionManager<IReliableDictionary<string, byte[]>>(_stateManager, collection);
      }

      public void Dispose()
      {
      }
   }
}

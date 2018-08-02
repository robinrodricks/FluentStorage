using System.Collections.Generic;
using System.IO;
using System;
using NetBox.Model;
using System.Linq;
using NetBox.IO;
using System.Threading.Tasks;
using System.Threading;
using NetBox.Extensions;
using NetBox;

namespace Storage.Net.Blob
{
   class InMemoryBlobStorage : IBlobStorage
   {
      private readonly Dictionary<BlobId, MemoryStream> _idToData = new Dictionary<BlobId, MemoryStream>();

      public Task<IReadOnlyCollection<BlobId>> ListAsync(ListOptions options, CancellationToken cancellationToken)
      {
         if (options == null) options = new ListOptions();

         options.FolderPath = StoragePath.Normalize(options.FolderPath);

         List<BlobId> matches = _idToData

            .Where(e => options.Recurse
               ? e.Key.FolderPath.StartsWith(options.FolderPath)
               : e.Key.FolderPath == options.FolderPath)

            .Select(e => e.Key)
            .Where(options.IsMatch)
            .Take(options.MaxResults == null ? int.MaxValue : options.MaxResults.Value)
            .ToList();

         return Task.FromResult((IReadOnlyCollection<BlobId>)matches);
      }

      public Task WriteAsync(string id, Stream sourceStream, bool append, CancellationToken cancellationToken)
      {
         GenericValidation.CheckBlobId(id);

         if (append)
         {
            if (!Exists(id))
            {
               Write(id, sourceStream);
            }
            else
            {
               MemoryStream ms = _idToData[id];

               byte[] part1 = ms.ToArray();
               byte[] part2 = sourceStream.ToByteArray();
               _idToData[id] = new MemoryStream(part1.Concat(part2).ToArray());
            }
         }
         else
         {
            Write(id, sourceStream);
         }

         return Task.FromResult(true);
      }

      public Task<Stream> OpenWriteAsync(string id, bool append = false, CancellationToken cancellationToken = default)
      {
         throw new NotImplementedException();
      }

      public Task<Stream> OpenReadAsync(string id, CancellationToken cancellationToken)
      {
         GenericValidation.CheckBlobId(id);

         if (!_idToData.TryGetValue(id, out MemoryStream ms)) return Task.FromResult<Stream>(null);

         ms.Seek(0, SeekOrigin.Begin);
         return Task.FromResult<Stream>(new NonCloseableStream(ms));
      }

      public Task DeleteAsync(IEnumerable<string> ids, CancellationToken cancellationToken)
      {
         GenericValidation.CheckBlobId(ids);

         foreach (string blobId in ids)
         {
            _idToData.Remove(blobId);
         }

         return Task.FromResult(true);
      }

      public Task<IEnumerable<bool>> ExistsAsync(IEnumerable<string> ids, CancellationToken cancellationToken)
      {
         var result = new List<bool>();

         foreach (string id in ids)
         {
            result.Add(_idToData.ContainsKey(id));
         }

         return Task.FromResult<IEnumerable<bool>>(result);
      }

      public Task<IEnumerable<BlobMeta>> GetMetaAsync(IEnumerable<string> ids, CancellationToken cancellationToken)
      {
         GenericValidation.CheckBlobId(ids);

         var result = new List<BlobMeta>();

         foreach (string id in ids)
         {
            if (!_idToData.TryGetValue(id, out MemoryStream ms))
            {
               result.Add(null);
            }
            else
            {
               ms.Seek(0, SeekOrigin.Begin);

               var meta = new BlobMeta(ms.Length, ms.GetHash(HashType.Md5), null);

               result.Add(meta);
            }
         }

         return Task.FromResult<IEnumerable<BlobMeta>>(result);
      }

      private void Write(string id, Stream sourceStream)
      {
         var ms = new MemoryStream(sourceStream.ToByteArray());
         _idToData[new BlobId(id, BlobItemKind.File)] = ms;
      }

      private bool Exists(string id)
      {
         GenericValidation.CheckBlobId(id);

         return _idToData.ContainsKey(id);
      }

      public void Dispose()
      {
      }

      public Task<ITransaction> OpenTransactionAsync()
      {
         return Task.FromResult(EmptyTransaction.Instance);
      }
   }
}

using System.Collections.Generic;
using System.IO;
using System;
using NetBox.Model;
using System.Linq;
using NetBox.IO;
using System.Threading.Tasks;
using System.Threading;

namespace Storage.Net.Blob
{
   class InMemoryBlobStorage : IBlobStorageProvider
   {
      private readonly Dictionary<BlobId, MemoryStream> _idToData = new Dictionary<BlobId, MemoryStream>();

      public Task<IEnumerable<BlobId>> ListAsync(string folderPath, string prefix, bool recurse, CancellationToken cancellationToken)
      {
         folderPath = StoragePath.Normalize(folderPath);

         List<BlobId> matches = _idToData

            .Where(e => recurse
               ? e.Key.FolderPath.StartsWith(folderPath)
               : e.Key.FolderPath == folderPath)

            .Where(e => prefix == null || e.Key.Id.StartsWith(prefix))

            .Select(e => e.Key)

            .ToList();

         return Task.FromResult((IEnumerable<BlobId>)matches);
      }

      public Task WriteAsync(string id, Stream sourceStream, bool append)
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

      public Task<Stream> OpenReadAsync(string id)
      {
         GenericValidation.CheckBlobId(id);

         if (!_idToData.TryGetValue(id, out MemoryStream ms)) return null;

         ms.Seek(0, SeekOrigin.Begin);
         return Task.FromResult<Stream>(new NonCloseableStream(ms));
      }

      public Task DeleteAsync(IEnumerable<string> ids)
      {
         GenericValidation.CheckBlobId(ids);

         foreach (string blobId in ids)
         {
            _idToData.Remove(blobId);
         }

         return Task.FromResult(true);
      }

      public Task<IEnumerable<bool>> ExistsAsync(IEnumerable<string> ids)
      {
         var result = new List<bool>();

         foreach (string id in ids)
         {
            result.Add(_idToData.ContainsKey(id));
         }

         return Task.FromResult<IEnumerable<bool>>(result);
      }

      public Task<IEnumerable<BlobMeta>> GetMetaAsync(IEnumerable<string> ids)
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

               var meta = new BlobMeta(ms.Length, ms.GetHash(HashType.Md5));

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
         throw new NotImplementedException();
      }

      /*


      
      public override BlobMeta GetMeta(string id)
      {
         GenericValidation.CheckBlobId(id);

         if (!_idToData.TryGetValue(id, out MemoryStream ms)) return null;

         ms.Seek(0, SeekOrigin.Begin);

         return new BlobMeta(ms.Length, ms.GetHash(HashType.Md5));
      }
      */
   }
}

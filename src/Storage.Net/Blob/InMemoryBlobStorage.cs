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
      private readonly Dictionary<string, MemoryStream> _idToData = new Dictionary<string, MemoryStream>();

      public async Task<IEnumerable<BlobId>> ListAsync(string folderPath, string prefix, bool recurse, CancellationToken cancellationToken)
      {
         throw new NotImplementedException();
      }

      public void Dispose()
      {
         throw new NotImplementedException();
      }

      /*
      public override void Append(string id, Stream sourceStream)
      {
         GenericValidation.CheckBlobId(id);

         if(!Exists(id))
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

      public override Stream OpenRead(string id)
      {
         GenericValidation.CheckBlobId(id);

         if (!_idToData.TryGetValue(id, out MemoryStream ms)) return null;

         ms.Seek(0, SeekOrigin.Begin);
         return new NonCloseableStream(ms);
      }

      public override void Write(string id, Stream sourceStream)
      {
         GenericValidation.CheckBlobId(id);

         var ms = new MemoryStream(sourceStream.ToByteArray());
         _idToData[id] = ms;
      }

      public override void Delete(string id)
      {
         GenericValidation.CheckBlobId(id);

         _idToData.Remove(id);
      }

      public override bool Exists(string id)
      {
         GenericValidation.CheckBlobId(id);

         return _idToData.ContainsKey(id);
      }

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

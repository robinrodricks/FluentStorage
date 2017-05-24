using System.Collections.Generic;
using System.IO;
using System;
using NetBox.Model;
using System.Linq;
using NetBox.IO;

namespace Storage.Net.Blob
{
   class InMemoryBlobStorage : AsyncBlobStorage
   {
      private readonly Dictionary<string, MemoryStream> _idToData = new Dictionary<string, MemoryStream>();

      public override Stream OpenAppend(string id)
      {
         GenericValidation.CheckBlobId(id);

         if(!Exists(id))
         {
            return OpenWrite(id);
         }
         else
         {
            MemoryStream ms = _idToData[id];
            ms.Seek(0, SeekOrigin.End);
            return new NonCloseableStream(ms);
         }
      }

      public override Stream OpenRead(string id)
      {
         GenericValidation.CheckBlobId(id);

         if (!_idToData.TryGetValue(id, out MemoryStream ms)) return null;

         ms.Seek(0, SeekOrigin.Begin);
         return new NonCloseableStream(ms);
      }

      public override Stream OpenWrite(string id)
      {
         GenericValidation.CheckBlobId(id);

         var ms = new MemoryStream();
         _idToData[id] = ms;
         return new NonCloseableStream(ms);
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

      public override IEnumerable<string> List(string prefix)
      {
         GenericValidation.CheckBlobPrefix(prefix);

         if (prefix == null) return _idToData.Keys.ToList();

         return _idToData.Keys.Where(k => k.StartsWith(prefix)).ToList();
      }

   }
}

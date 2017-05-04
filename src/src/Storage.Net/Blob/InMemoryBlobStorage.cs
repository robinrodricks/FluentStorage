using System.Collections.Generic;
using System.IO;
using System;
using NetBox.Model;
using System.Linq;

namespace Storage.Net.Blob
{
   class InMemoryBlobStorage : AsyncBlobStorage
   {
      class BlobTag
      {
         public byte[] Data { get; set; }

         public BlobMeta Meta { get; set; }
      }

      private readonly Dictionary<string, BlobTag> _idToTag = new Dictionary<string, BlobTag>();

      public override void AppendFromStream(string id, Stream chunkStream)
      {
         if (chunkStream == null)
            throw new ArgumentNullException(nameof(chunkStream));

         GenericValidation.CheckBlobId(id);

         if(!Exists(id))
         {
            UploadFromStream(id, chunkStream);
         }
         else
         {
            BlobTag tag = _idToTag[id];
            tag.Data = tag.Data.Concat(chunkStream.ToByteArray()).ToArray();
            tag.Meta = new BlobMeta(tag.Data.Length, tag.Data.GetHash(HashType.Md5).ToHexString());
         }
      }

      public override void Delete(string id)
      {
         GenericValidation.CheckBlobId(id);

         _idToTag.Remove(id);
      }

      public override bool Exists(string id)
      {
         GenericValidation.CheckBlobId(id);

         return _idToTag.ContainsKey(id);
      }

      public override BlobMeta GetMeta(string id)
      {
         GenericValidation.CheckBlobId(id);

         if (!_idToTag.TryGetValue(id, out BlobTag tag)) return null;

         return tag.Meta;
      }

      public override IEnumerable<string> List(string prefix)
      {
         GenericValidation.CheckBlobPrefix(prefix);

         if (prefix == null) return _idToTag.Keys.ToList();

         return _idToTag.Keys.Where(k => k.StartsWith(prefix)).ToList();
      }

      public override Stream OpenStreamToRead(string id)
      {
         GenericValidation.CheckBlobId(id);

         if (!_idToTag.TryGetValue(id, out BlobTag tag)) return null;

         return new MemoryStream(tag.Data);
      }

      public override void UploadFromStream(string id, Stream sourceStream)
      {
         if (sourceStream == null)
            throw new System.ArgumentNullException(nameof(sourceStream));

         GenericValidation.CheckBlobId(id);

         byte[] data = sourceStream.ToByteArray();
         var meta = new BlobMeta(data.Length, data.GetHash(HashType.Md5).ToHexString());
         var tag = new BlobTag
         {
            Meta = meta,
            Data = data
         };
         _idToTag[id] = tag;
      }

   }
}

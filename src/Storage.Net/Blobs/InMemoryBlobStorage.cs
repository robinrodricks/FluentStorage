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
using Storage.Net.Streaming;

namespace Storage.Net.Blobs
{
   class InMemoryBlobStorage : IBlobStorage
   {
      struct Tag
      {
         public byte[] data;
         public DateTimeOffset lastMod;
         public string md5;
      }

      private readonly Dictionary<Blob, Tag> _blobToTag = new Dictionary<Blob, Tag>();

      public Task<IReadOnlyCollection<Blob>> ListAsync(ListOptions options, CancellationToken cancellationToken)
      {
         if (options == null) options = new ListOptions();

         options.FolderPath = StoragePath.Normalize(options.FolderPath);

         List<Blob> matches = _blobToTag

            .Where(e => options.Recurse
               ? e.Key.FolderPath.StartsWith(options.FolderPath)
               : StoragePath.ComparePath(e.Key.FolderPath, options.FolderPath))

            .Select(e => e.Key)
            .Where(options.IsMatch)
            .Where(e => options.BrowseFilter == null || options.BrowseFilter(e))
            .Take(options.MaxResults == null ? int.MaxValue : options.MaxResults.Value)
            .ToList();

         return Task.FromResult((IReadOnlyCollection<Blob>)matches);
      }

      public Task WriteAsync(string fullPath, Stream sourceStream, bool append, CancellationToken cancellationToken)
      {
         GenericValidation.CheckBlobFullPath(fullPath);
         fullPath = StoragePath.Normalize(fullPath);

         if (append)
         {
            if (!Exists(fullPath))
            {
               Write(fullPath, sourceStream);
            }
            else
            {
               Tag tag = _blobToTag[fullPath];
               byte[] data = tag.data.Concat(sourceStream.ToByteArray()).ToArray();

               _blobToTag[fullPath] = ToTag(data);
            }
         }
         else
         {
            Write(fullPath, sourceStream);
         }

         return Task.FromResult(true);
      }

      public Task<Stream> OpenWriteAsync(string fullPath, bool append, CancellationToken cancellationToken)
      {
         GenericValidation.CheckBlobFullPath(fullPath);
         fullPath = StoragePath.Normalize(fullPath);

         var result = new FixedStream(new MemoryStream(), null, async fx =>
         {
            MemoryStream ms = (MemoryStream)fx.Parent;
            ms.Position = 0;
            await WriteAsync(fullPath, ms, append, cancellationToken).ConfigureAwait(false);
         });

         return Task.FromResult<Stream>(result);
      }

      public Task<Stream> OpenReadAsync(string fullPath, CancellationToken cancellationToken)
      {
         GenericValidation.CheckBlobFullPath(fullPath);
         fullPath = StoragePath.Normalize(fullPath);

         if (!_blobToTag.TryGetValue(fullPath, out Tag tag)) return Task.FromResult<Stream>(null);

         return Task.FromResult<Stream>(new NonCloseableStream(new MemoryStream(tag.data)));
      }

      public Task DeleteAsync(IEnumerable<string> fullPaths, CancellationToken cancellationToken)
      {
         GenericValidation.CheckBlobFullPaths(fullPaths);

         foreach(string path in fullPaths)
         {
            string prefix = StoragePath.Normalize(path) + StoragePath.PathSeparatorString;

            List<Blob> candidates = _blobToTag.Where(p => p.Key.FullPath.StartsWith(prefix)).Select(p => p.Key).ToList();

            foreach(Blob candidate in candidates)
            {
               _blobToTag.Remove(candidate);
            }
         }
         return Task.FromResult(true);
      }

      public Task<IReadOnlyCollection<bool>> ExistsAsync(IEnumerable<string> ids, CancellationToken cancellationToken)
      {
         var result = new List<bool>();

         foreach (string id in ids)
         {
            result.Add(_blobToTag.ContainsKey(StoragePath.Normalize(id)));
         }

         return Task.FromResult<IReadOnlyCollection<bool>>(result);
      }

      public Task<IReadOnlyCollection<Blob>> GetBlobsAsync(IEnumerable<string> fullPaths, CancellationToken cancellationToken)
      {
         GenericValidation.CheckBlobFullPaths(fullPaths);

         var result = new List<Blob>();

         foreach (string fullPath in fullPaths)
         {
            if (!_blobToTag.TryGetValue(StoragePath.Normalize(fullPath), out Tag tag))
            {
               result.Add(null);
            }
            else
            {
               var r = new Blob(fullPath)
               {
                  Size = tag.data.Length,
                  MD5 = tag.md5,
                  LastModificationTime = tag.lastMod
               };

               result.Add(r);
            }
         }

         return Task.FromResult<IReadOnlyCollection<Blob>>(result);
      }

      public Task SetBlobsAsync(IEnumerable<Blob> blobs, CancellationToken cancellationToken = default)
      {
         if(blobs == null)
            return Task.FromResult(true);

         foreach(Blob blob in blobs)
         {
            if(_blobToTag.TryGetValue(blob, out Tag tag))
            {
               _blobToTag.Remove(blob);
               _blobToTag[blob] = tag;
            }
         }

         return Task.FromResult(true);
      }

      private void Write(string fullPath, Stream sourceStream)
      {
         GenericValidation.CheckBlobFullPath(fullPath);
         fullPath = StoragePath.Normalize(fullPath);

         Tag tag = ToTag(sourceStream);

         _blobToTag[fullPath] = tag;
      }

      private static Tag ToTag(Stream s)
      {
         if (s is MemoryStream ms) ms.Position = 0;
         return ToTag(s.ToByteArray());
      }

      private static Tag ToTag(byte[] data)
      {
         var tag = new Tag();
         tag.data = data;
         tag.lastMod = DateTimeOffset.UtcNow;
         tag.md5 = tag.data.GetHash(HashType.Md5).ToHexString();
         return tag;
      }

      private bool Exists(string fullPath)
      {
         GenericValidation.CheckBlobFullPath(fullPath);

         return _blobToTag.ContainsKey(fullPath);
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

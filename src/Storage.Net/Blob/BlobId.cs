using System;
using System.Linq;

namespace Storage.Net.Blob
{
   /// <summary>
   /// Blob item description
   /// </summary>
   public class BlobId : IEquatable<BlobId>
   {
      /// <summary>
      /// Gets the kind of item
      /// </summary>
      public BlobItemKind Kind { get; private set; }

      /// <summary>
      /// Gets the folder path containing this item
      /// </summary>
      public string FolderPath { get; private set; }

      /// <summary>
      /// Gets the id of this blob, uniqueue within the folder
      /// </summary>
      public string Id { get; private set; }

      public string FullPath => StoragePath.Combine(FolderPath, Id);

      public BlobId(string fullId, BlobItemKind kind)
      {
         string path = StoragePath.Normalize(fullId);
         string[] parts = StoragePath.GetParts(path);

         Id = parts.Last();
         FolderPath = parts.Length > 1
            ? StoragePath.Combine(parts.Take(parts.Length - 1))
            : StoragePath.PathStrSeparator;

         Kind = kind;
      }

      public BlobId(string folderPath, string id, BlobItemKind kind)
      {
         Id = id ?? throw new ArgumentNullException(nameof(id));
         FolderPath = folderPath;
         Kind = kind;
      }

      public override string ToString()
      {
         string k = Kind == BlobItemKind.File ? "file" : "folder";
         
         return $"{k}: {Id}@{FolderPath}";
      }

      public bool Equals(BlobId other)
      {
         throw new NotImplementedException();
      }
   }
}

using System;
using System.Collections.Generic;
using System.Linq;

namespace Storage.Net.Blobs
{
   /// <summary>
   /// Blob item description
   /// </summary>
   public sealed class Blob : IEquatable<Blob>
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

      /// <summary>
      /// Blob size
      /// </summary>
      public long? Size { get; set; }

      /// <summary>
      /// MD5 content hash of the blob. Note that this property can be null if underlying storage has
      /// no information about the hash.
      /// </summary>
      public string MD5 { get; set;  }

      /// <summary>
      /// Last modification time when known
      /// </summary>
      public DateTimeOffset? LastModificationTime { get; set; }

      /// <summary>
      /// Gets full path to this blob which is a combination of folder path and blob name
      /// </summary>
      public string FullPath => StoragePath.Combine(FolderPath, Id);

      /// <summary>
      /// Custom provider-specific properties
      /// </summary>
      public Dictionary<string, string> Properties { get; set; }

      /// <summary>
      /// Create a new instance
      /// </summary>
      /// <param name="fullId"></param>
      /// <param name="kind"></param>
      public Blob(string fullId, BlobItemKind kind = BlobItemKind.File)
      {
         string path = StoragePath.Normalize(fullId);
         string[] parts = StoragePath.Split(path);

         Id = parts.Last();
         FolderPath = StoragePath.GetParent(path);

         Kind = kind;
      }

      /// <summary>
      /// Creates a new instance
      /// </summary>
      /// <param name="folderPath"></param>
      /// <param name="id"></param>
      /// <param name="kind"></param>
      public Blob(string folderPath, string id, BlobItemKind kind)
      {
         Id = id ?? throw new ArgumentNullException(nameof(id));
         FolderPath = folderPath;
         Kind = kind;
      }

      /// <summary>
      /// Full blob info, i.e type, id and path
      /// </summary>
      public override string ToString()
      {
         string k = Kind == BlobItemKind.File ? "file" : "folder";
         
         return $"{k}: {Id}@{FolderPath}";
      }

      /// <summary>
      /// Equality check
      /// </summary>
      /// <param name="other"></param>
      public bool Equals(Blob other)
      {
         if (ReferenceEquals(other, null)) return false;

         return
            other.FullPath == FullPath &&
            other.Kind == Kind;
      }

      /// <summary>
      /// Equality check
      /// </summary>
      /// <param name="other"></param>
      public override bool Equals(object other)
      {
         if (ReferenceEquals(other, null)) return false;
         if (ReferenceEquals(other, this)) return true;
         if (other.GetType() != typeof(Blob)) return false;

         return Equals((Blob)other);
      }

      /// <summary>
      /// Hash code calculation
      /// </summary>
      public override int GetHashCode()
      {
         return FullPath.GetHashCode() * Kind.GetHashCode();
      }

      /// <summary>
      /// Constructs a file blob by full ID
      /// </summary>
      public static implicit operator Blob(string fileId)
      {
         return new Blob(fileId, BlobItemKind.File);
      }
   }
}

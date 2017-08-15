using System;

namespace Storage.Net.Blob
{
   /// <summary>
   /// Blob item description
   /// </summary>
   public class BlobItem
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
      /// Creates an instance of BlobItem as a file in root folder
      /// </summary>
      /// <param name="id">Blob ID</param>
      public BlobItem(string id)
      {
         Id = id ?? throw new ArgumentNullException(nameof(id));
         FolderPath = "/";
         Kind = BlobItemKind.File;
      }

      public BlobItem(string folderPath, string id, BlobItemKind kind)
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
   }
}

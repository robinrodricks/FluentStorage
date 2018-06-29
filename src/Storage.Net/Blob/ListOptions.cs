using System.Collections.Generic;
using System.Linq;
using NetBox.Extensions;

namespace Storage.Net.Blob
{
   /// <summary>
   /// Options for listing storage content
   /// </summary>
   public class ListOptions
   {
      private string _prefix;

      /// <summary>
      /// Folder path to start browsing from
      /// </summary>
      public string FolderPath { get; set; }

      /// <summary>
      /// Prefix to filter the name by
      /// </summary>
      public string Prefix
      {
         get => _prefix;
         set
         {
            GenericValidation.CheckBlobPrefix(value);
            _prefix = value;
         }
      }

      /// <summary>
      /// When true, operation will recursively navigate down the folders
      /// </summary>
      public bool Recurse { get; set; }

      /// <summary>
      /// When set, limits the maximum amount of results
      /// </summary>
      public int? MaxResults { get; set; }

      /// <summary>
      /// Helper method that returns true if a <see cref="BlobId"/> matches these list options.
      /// </summary>
      public bool IsMatch(BlobId id)
      {
         return _prefix == null || id.Id.StartsWith(_prefix);
      }

      /// <summary>
      /// Only for internal use
      /// </summary>
      public bool Add(ICollection<BlobId> dest, ICollection<BlobId> src)
      {
         if(MaxResults == null || (dest.Count + src.Count < MaxResults.Value))
         {
            dest.AddRange(src);
            return false;
         }

         dest.AddRange(src.Take(MaxResults.Value - dest.Count));
         return true;
      }
   }
}

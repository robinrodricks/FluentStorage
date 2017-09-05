using System.Collections.Generic;
using System.Linq;

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

      public bool IsMatch(BlobId id)
      {
         return _prefix == null || id.Id.StartsWith(_prefix);
      }

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

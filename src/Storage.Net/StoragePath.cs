using System;
using System.Linq;

namespace Storage.Net
{
   /// <summary>
   /// Storage Path utilities
   /// </summary>
   public static class StoragePath
   {
      /// <summary>
      /// Character used to split paths 
      /// </summary>
      public const char PathSeparator = '/';

      /// <summary>
      /// Character used to split paths as a string value
      /// </summary>
      public static readonly string PathSeparatorString = new string(PathSeparator, 1);

      /// <summary>
      /// Character used to split paths 
      /// </summary>
      public static string PathStrSeparator = new string(PathSeparator, 1);

      /// <summary>
      /// Combines parts of path
      /// </summary>
      /// <param name="parts"></param>
      /// <returns></returns>
      public static string Combine(params string[] parts)
      {
         return Normalize(string.Join(PathStrSeparator, parts.Select(p => NormalizePart(p))));
      }

      /// <summary>
      /// Normalizes path
      /// </summary>
      /// <param name="path"></param>
      /// <returns></returns>
      public static string Normalize(string path)
      {
         if (path == null) return null;

         return PathSeparatorString + path.Trim(PathSeparator);
      }

      /// <summary>
      /// Normalizes path part
      /// </summary>
      /// <param name="part"></param>
      /// <returns></returns>
      public static string NormalizePart(string part)
      {
         if (part == null) throw new ArgumentNullException(nameof(part));

         return part.Trim(PathSeparator);
      }

      /// <summary>
      /// Splits path in parts
      /// </summary>
      /// <param name="path"></param>
      /// <returns></returns>
      public static string[] GetParts(string path)
      {
         if (path == null) return null;

         return path.Split(new[] { PathSeparator }, StringSplitOptions.RemoveEmptyEntries).Select(NormalizePart).ToArray();
      }
   }
}

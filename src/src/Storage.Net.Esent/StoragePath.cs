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
         return string.Join(PathStrSeparator, parts);
      }
   }
}

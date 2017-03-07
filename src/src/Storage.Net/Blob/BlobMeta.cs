namespace Storage.Net.Blob
{
   /// <summary>
   /// Contains basic metadata about a blob
   /// </summary>
   public class BlobMeta
   {
      /// <summary>
      /// Creates an instance of blob metadata
      /// </summary>
      /// <param name="size">Blob size</param>
      /// <param name="md5">MD5 hash of the blob</param>
      public BlobMeta(long size, string md5)
      {
         this.Size = size;
         this.MD5 = md5;
      }

      /// <summary>
      /// Blob size
      /// </summary>
      public long Size { get; }

      /// <summary>
      /// MD5 content hash of the blob
      /// </summary>
      public string MD5 { get; }
   }
}

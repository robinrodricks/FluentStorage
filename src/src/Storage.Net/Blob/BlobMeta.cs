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
      public BlobMeta(long size)
      {
         this.Size = size;
      }

      /// <summary>
      /// Blob size
      /// </summary>
      public long Size { get; }
   }
}

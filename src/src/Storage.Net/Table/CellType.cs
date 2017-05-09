namespace Storage.Net.Table
{
   /// <summary>
   /// Cell data type
   /// </summary>
   public enum CellType
   {
      /// <summary>
      /// String
      /// </summary>
      String,

      /// <summary>
      /// Integer
      /// </summary>
      Int,

      /// <summary>
      /// Long
      /// </summary>
      Long,

      /// <summary>
      /// Double
      /// </summary>
      Double,

      /// <summary>
      /// DateTime
      /// </summary>
      DateTime,

      /// <summary>
      /// Boolean
      /// </summary>
      Boolean,

      /// <summary>
      /// Enumeration
      /// </summary>
      Enum,

      /// <summary>
      /// Guid
      /// </summary>
      Guid,

      /// <summary>
      /// Array of bytes
      /// </summary>
      ByteArray
   }
}

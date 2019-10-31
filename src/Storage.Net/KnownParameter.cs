namespace Storage.Net
{
   /// <summary>
   /// Known parameter names enouraged to be used in connection strings
   /// </summary>
   public static class KnownParameter
   {
      /// <summary>
      /// Account or storage name
      /// </summary>
      public static readonly string AccountName = "account";

      /// <summary>
      /// Key or password
      /// </summary>
      public static readonly string KeyOrPassword = "key";

      /// <summary>
      /// Key ID
      /// </summary>
      public static readonly string KeyId = "keyId";

      /// <summary>
      /// Session token
      /// </summary>
      public static readonly string SessionToken = "st";
   }
}

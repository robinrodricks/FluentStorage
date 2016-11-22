namespace Storage.Net.FileFormats
{
   static class CsvFormat
   {
      public static readonly char[] ColumnSeparator = {','};
      public static readonly char[] NewLine = {'\r', '\n'};
      private const string ValueLeftBracket = "\"";
      private const string ValueRightBracket = "\"";
      private const string ValueEscapeFind = "\"";
      private const string ValueEscapeValue = "\"\"";
      private static readonly char[] ColumnValueLeftTrimChars = {'\"'};
      private static readonly char[] ColumnValueRightTrimChars = {'\"'};

      /// <summary>
      /// Implemented according to RFC4180 http://tools.ietf.org/html/rfc4180
      /// </summary>
      /// <param name="value"></param>
      /// <returns></returns>
      public static string EscapeValue(string value)
      {
         if(value == null)
         {
            value = string.Empty;
         }
         else
         {
            value = value

               /*
                7.   If double-quotes are used to enclose fields, then a double-quote
                     appearing inside a field must be escaped by preceding it with
                     another double quote.  For example:

                     "aaa","b""bb","ccc"
                */
               .Replace(ValueEscapeFind, ValueEscapeValue);
         }

         return ValueLeftBracket + value + ValueRightBracket;
      }

      public static string UnescapeValue(string value)
      {
         if(value == null) return null;

         value = value.TrimStart(ColumnValueLeftTrimChars).TrimEnd(ColumnValueRightTrimChars);

         return value;
      }
   }
}

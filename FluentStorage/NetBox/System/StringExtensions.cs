namespace System {
    using System.Net.Http;
    using System.Threading.Tasks;
    using NetBox;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Text.RegularExpressions;

	/// <summary>
	/// String extensions.
	/// </summary>
	public static class StringExtensions {
        private const string HtmlStripPattern = @"<(.|\n)*?>";

        static readonly char[] Invalid = Path.GetInvalidFileNameChars();

        /// <summary>
        /// Converts hex string to byte array
        /// </summary>
        /// <param name="hex"></param>
        /// <returns></returns>
        public static byte[]? FromHexToBytes(this string? hex) {
            if(hex == null)
                return null;

            byte[] raw = new byte[hex.Length / 2];
            for(int i = 0; i < raw.Length; i++) {
                try {
                    raw[i] = Convert.ToByte(hex.Substring(i * 2, 2), 16);
                } catch(FormatException) {
                    return null;
                }
            }
            return raw;
        }

        #region [ HTML Helpers ]

        /// <summary>
        /// Strips HTML string from any tags leaving text only.
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        public static string? StripHtml(this string? s) {
            if(s == null)
                return null;

            //This pattern Matches everything found inside html tags;
            //(.|\n) - > Look for any character or a new line
            // *?  -> 0 or more occurences, and make a non-greedy search meaning
            //That the match will stop at the first available '>' it sees, and not at the last one
            //(if it stopped at the last one we could have overlooked
            //nested HTML tags inside a bigger HTML tag..)

            return Regex.Replace(s, HtmlStripPattern, string.Empty);
        }

        #endregion

        #region [ Encoding ]

        /// <summary>
        /// Encodes a string to BASE64 format
        /// </summary>
        public static string? Base64Encode(this string? s) {
            if(s == null)
                return null;

            return Convert.ToBase64String(Encoding.UTF8.GetBytes(s));
        }

        /// <summary>
        /// Decodes a BASE64 encoded string
        /// </summary>
        public static string? Base64Decode(this string? s) {
            if(s == null)
                return null;

            byte[] data = Convert.FromBase64String(s);

            return Encoding.UTF8.GetString(data, 0, data.Length);
        }


        /// <summary>
        /// Decodes a BASE64 encoded string to byte array
        /// </summary>
        /// <param name="s">String to decode</param>
        /// <returns>Byte array</returns>
        public static byte[]? Base64DecodeAsBytes(this string? s) {
            if(s == null)
                return null;

            return Convert.FromBase64String(s);
        }

        /// <summary>
        /// Converts shortest guid representation back to Guid. See <see cref="GuidExtensions.ToShortest(Guid)"/>
        /// on how to convert Guid to string.
        /// </summary>
        public static Guid FromShortestGuid(this string s) {
            byte[] guidBytes = Ascii85.Instance.Decode(s, false);
            return new Guid(guidBytes);
        }

        /// <summary>
        /// URL-encodes input string
        /// </summary>
        /// <param name="value">String to encode</param>
        /// <returns>Encoded string</returns>
        public static string? UrlEncode(this string? value) {
            return WebUtility.UrlEncode(value);
        }

        /// <summary>
        /// URL-decodes input string
        /// </summary>
        /// <param name="value">String to decode</param>
        /// <returns>Decoded string</returns>
        public static string? UrlDecode(this string? value) {
            return WebUtility.UrlDecode(value);
        }

        public static byte[]? UTF8Bytes(this string? s) {
            if(s == null)
                return null;

            return Encoding.UTF8.GetBytes(s);
        }

        #endregion

        #region [ Hashing ]

        public static string? MD5(this string? s) {
            if(s == null)
                return null;

            return Encoding.UTF8.GetBytes(s).MD5().ToHexString();
        }

        public static string? SHA256(this string? s) {
            if(s == null)
                return null;

            return Encoding.UTF8.GetBytes(s).SHA256().ToHexString();
        }

        public static byte[]? HMACSHA256(this string? s, byte[] key) {
            if(s == null)
                return null;

            return Encoding.UTF8.GetBytes(s).HMACSHA256(key);
        }


        #endregion

        #region [ Stream Conversion ]

        /// <summary>
        /// Converts to MemoryStream with a specific encoding
        /// </summary>
        public static MemoryStream? ToMemoryStream(this string? s, Encoding? encoding) {
            if(s == null)
                return null;
            if(encoding == null)
                encoding = Encoding.UTF8;

            return new MemoryStream(encoding.GetBytes(s));
        }

        /// <summary>
        /// Converts to MemoryStream in UTF-8 encoding
        /// </summary>
        public static MemoryStream? ToMemoryStream(this string? s) {
            // ReSharper disable once IntroduceOptionalParameters.Global
            return ToMemoryStream(s, null);
        }

        #endregion

        #region [ String Manipulation ]

        private static bool FindTagged(ref string s, ref string startToken, ref string endToken, bool includeOuterTokens, out int startIdx, out int length) {
            int idx0 = s.IndexOf(startToken, StringComparison.Ordinal);

            if(idx0 != -1) {
                int idx1 = s.IndexOf(endToken, idx0 + startToken.Length, StringComparison.Ordinal);

                if(idx1 != -1) {
                    startIdx = includeOuterTokens ? idx0 : idx0 + startToken.Length;
                    length = includeOuterTokens
                                 ? (idx1 - idx0 + endToken.Length)
                                 : (idx1 - idx0 - startToken.Length);

                    return true;
                }
            }

            startIdx = length = -1;
            return false;
        }

        /// <summary>
        /// Looks for <paramref name="startTag"/> and <paramref name="endTag"/> followed in sequence and when found returns the text between them.
        /// </summary>
        /// <param name="s">Input string</param>
        /// <param name="startTag">Start tag</param>
        /// <param name="endTag">End tag</param>
        /// <param name="includeOuterTags">When set to true, returns the complete phrase including start and end tag value,
        /// otherwise only inner text returned</param>
        /// <returns></returns>
        public static string? FindTagged(this string s, string? startTag, string? endTag, bool includeOuterTags) {
            if(startTag == null)
                throw new ArgumentNullException(nameof(startTag));
            if(endTag == null)
                throw new ArgumentNullException(nameof(endTag));

            int start, length;
            if(FindTagged(ref s, ref startTag, ref endTag, includeOuterTags, out start, out length)) {
                return s.Substring(start, length);
            }

            return null;
        }

        /// <summary>
        /// Looks for <paramref name="startTag"/> and <paramref name="endTag"/> followed in sequence, and if found
        /// performs a replacement of text inside them with <paramref name="replacementText"/>
        /// </summary>
        /// <param name="s">Input string</param>
        /// <param name="startTag">Start tag</param>
        /// <param name="endTag">End tag</param>
        /// <param name="replacementText">Replacement text</param>
        /// <param name="replaceOuterTokens">When set to true, not only the text between tags is replaced, but the whole
        /// phrase with <paramref name="startTag"/>, text between tags and <paramref name="endTag"/></param>
        /// <returns></returns>
        public static string ReplaceTagged(this string s, string startTag, string endTag, string replacementText, bool replaceOuterTokens) {
            if(startTag == null)
                throw new ArgumentNullException(nameof(startTag));
            if(endTag == null)
                throw new ArgumentNullException(nameof(endTag));
            //if (replacementText == null) throw new ArgumentNullException("replacementText");

            int start, length;

            if(FindTagged(ref s, ref startTag, ref endTag, replaceOuterTokens, out start, out length)) {
                s = s.Remove(start, length);

                if(replacementText != null)
                    s = s.Insert(start, replacementText);
            }

            return s;
        }

        /// <summary>
        /// Converts a string with spaces to a camel case version, for example
        /// "The camel string" is converted to "TheCamelString"
        /// </summary>
        public static string? SpacedToCamelCase(this string? s) {
            if(s == null)
                return null;

            string[] parts = s.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

            var b = new StringBuilder();
            foreach(string part in parts) {
                string? uc = part.Capitalize();
                b.Append(uc);
            }
            return b.ToString();
        }

        /// <summary>
        /// Transforms the string so that the first letter is uppercase and the rest of them are lowercase
        /// </summary>
        public static string? Capitalize(this string? s) {
            if(s == null)
                return null;
            var b = new StringBuilder();

            for(int i = 0; i < s.Length; i++) {
                b.Append(i == 0 ? char.ToUpper(s[i]) : char.ToLower(s[i]));
            }

            return b.ToString();
        }

        /// <summary>
        /// Pythonic approach to slicing strings
        /// </summary>
        /// <param name="s">Input string</param>
        /// <param name="start">Is the start index to slice from. It can be either positive or negative.
        /// Negative value indicates that the index is taken from the end of the string.</param>
        /// <param name="end">Is the index to slice to. It can be either positive or negative.
        /// Negative value indicates that the index is taken from the end of the string.</param>
        /// <returns>Sliced string</returns>
        public static string? Slice(this string? s, int? start, int? end) {
            if(s == null)
                return null;
            if(start == null && end == null)
                return s;

            int si = start ?? 0;
            int ei = end ?? s.Length;

            if(si < 0) {
                si = s.Length + si;
                if(si < 0)
                    si = 0;
            }

            if(si > s.Length) {
                si = s.Length - 1;
            }

            if(ei < 0) {
                ei = s.Length + ei;
                if(ei < 0)
                    ei = 0;
            }

            if(ei > s.Length) {
                ei = s.Length;
            }

            return s.Substring(si, ei - si);
        }

        /// <summary>
        /// Splits the string into key and value using the provided delimiter values. Both key and value are trimmed as well.
        /// </summary>
        /// <param name="s">Input string. When null returns null immediately.</param>
        /// <param name="delimiter">List of delmiters between key and value. This method searches for all the provided
        /// delimiters, and splits by the first left-most one.</param>
        /// <returns>A tuple of two values where the first one is the key and second is the value. If none of the delimiters
        /// are found the second value of the tuple is null and the first value is the input string</returns>
        public static Tuple<string, string?>? SplitByDelimiter(this string? s, params string[] delimiter) {
            if(s == null)
                return null;

            string key;
            string? value;

            if(delimiter == null || delimiter.Length == 0) {
                key = s.Trim();
                value = null;
            } else {

                List<int> indexes = delimiter.Where(d => d != null).Select(d => s.IndexOf(d)).Where(d => d != -1).ToList();

                if(indexes.Count == 0) {
                    key = s.Trim();
                    value = null;
                } else {
                    int idx = indexes.OrderBy(i => i).First();
                    key = s.Substring(0, idx);
                    value = s.Substring(idx + 1);
                }
            }

            return new Tuple<string, string?>(key, value);
        }

        /// <summary>
        /// Splits text line by line and removes lines containing specific substring
        /// </summary>
        public static string RemoveLinesContaining(this string input, string substring, StringComparison stringComparison = StringComparison.CurrentCulture) {
            if(string.IsNullOrEmpty(input))
                return input;

            var result = new StringBuilder();

            using(var sr = new StringReader(input)) {
                string? line;
                while((line = sr.ReadLine()) != null) {
                    if(line.IndexOf(substring, stringComparison) != -1)
                        continue;

                    result.AppendLine(line);
                }
            }

            return result.ToString();
        }


        #endregion

        #region [ JSON ]

        /// <summary>
        /// Escapes a string for JSON encoding
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        public static string? ToEscapedJsonValueString(this string? s) {
            if(s == null)
                return null;

            string result = string.Empty;
            for(int i = 0; i < s.Length; i++) {
                char c = s[i];
                string ec;

                switch(c) {
                    case '\t':
                        ec = "\\t";
                        break;
                    case '\n':
                        ec = "\\n";
                        break;
                    case '\r':
                        ec = "\\r";
                        break;
                    case '\f':
                        ec = "\\f";
                        break;
                    case '\b':
                        ec = "\\b";
                        break;
                    case '\\':
                        ec = "\\\\";
                        break;
                    case '\u0085': // Next Line
                        ec = "\\u0085";
                        break;
                    case '\u2028': // Line Separator
                        ec = "\\u2028";
                        break;
                    case '\u2029': // Paragraph Separator
                        ec = "\\u2029";
                        break;
                    default:
                        ec = new string(c, 1);
                        break;
                }

                result += ec;
            }

            return result;
        }

        #endregion

        #region [ Networking ]

        /// <summary>
        /// Treat this string as URL and download it as stream
        /// </summary>
        /// <param name="uri"></param>
        /// <returns></returns>
        public static async Task<TempFile?> DownloadUrlToTempFile(this string? uri) {
            if(string.IsNullOrEmpty(uri))
                return null;

            var tf = new TempFile();

            using(Stream src = await new HttpClient().GetStreamAsync(uri)) {
                using(Stream tgt = File.Create(tf)) {
                    await src.CopyToAsync(tgt);
                }
            }

            return tf;
        }

        #endregion
    }
}
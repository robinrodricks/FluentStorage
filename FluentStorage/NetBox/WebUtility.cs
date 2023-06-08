namespace NetBox {
    using System;
    using System.Text;

	/// <summary>
	/// This class is ported from .NET 4.6 to support URL encoding/decoding functionality which is missing in .NET Standard
	/// </summary>
	public static class WebUtility {
        public static string? UrlEncode(string? value) {
            if(value == null)
                return null;

            byte[] bytes = Encoding.UTF8.GetBytes(value);
            byte[]? ue = UrlEncode(bytes, 0, bytes.Length, false);
            if(ue == null)
                return null;
            return Encoding.UTF8.GetString(ue);
        }

        public static string? UrlDecode(string? encodedValue) {
            if(encodedValue == null)
                return null;

            return UrlDecodeInternal(encodedValue, Encoding.UTF8);
        }

        private static byte[]? UrlEncode(byte[]? bytes, int offset, int count, bool alwaysCreateNewReturnValue) {
            byte[]? encoded = UrlEncode(bytes, offset, count);

            return (alwaysCreateNewReturnValue && (encoded != null) && (encoded == bytes))
                ? (byte[])encoded.Clone()
                : encoded;
        }

        private static byte[]? UrlEncode(byte[]? bytes, int offset, int count) {
            if(bytes == null)
                return null;

            if(!ValidateUrlEncodingParameters(bytes, offset, count)) {
                return null;
            }

            int cSpaces = 0;
            int cUnsafe = 0;

            // count them first
            for(int i = 0; i < count; i++) {
                char ch = (char)bytes[offset + i];

                if(ch == ' ')
                    cSpaces++;
                else if(!IsUrlSafeChar(ch))
                    cUnsafe++;
            }

            // nothing to expand?
            if(cSpaces == 0 && cUnsafe == 0) {
                // DevDiv 912606: respect "offset" and "count"
                if(0 == offset && bytes.Length == count) {
                    return bytes;
                } else {
                    byte[] subarray = new byte[count];
                    Buffer.BlockCopy(bytes, offset, subarray, 0, count);
                    return subarray;
                }
            }

            // expand not 'safe' characters into %XX, spaces to +s
            byte[] expandedBytes = new byte[count + (cUnsafe * 2)];
            int pos = 0;

            for(int i = 0; i < count; i++) {
                byte b = bytes[offset + i];
                char ch = (char)b;

                if(IsUrlSafeChar(ch)) {
                    expandedBytes[pos++] = b;
                } else if(ch == ' ') {
                    expandedBytes[pos++] = (byte)'+';
                } else {
                    expandedBytes[pos++] = (byte)'%';
                    expandedBytes[pos++] = (byte)IntToHex((b >> 4) & 0xf);
                    expandedBytes[pos++] = (byte)IntToHex(b & 0x0f);
                }
            }

            return expandedBytes;
        }

        private static bool ValidateUrlEncodingParameters(byte[]? bytes, int offset, int count) {
            if(bytes == null && count == 0)
                return false;
            if(bytes == null) {
                throw new ArgumentNullException("bytes");
            }
            if(offset < 0 || offset > bytes.Length) {
                throw new ArgumentOutOfRangeException("offset");
            }
            if(count < 0 || offset + count > bytes.Length) {
                throw new ArgumentOutOfRangeException("count");
            }

            return true;
        }

        private static bool IsUrlSafeChar(char ch) {
            if((ch >= 'a' && ch <= 'z') || (ch >= 'A' && ch <= 'Z') || (ch >= '0' && ch <= '9'))
                return true;

            switch(ch) {
                case '-':
                case '_':
                case '.':
                case '!':
                case '*':
                case '(':
                case ')':
                    return true;
            }

            return false;
        }

        private static char IntToHex(int n) {
            if(n <= 9)
                return (char)(n + (int)'0');
            else
                return (char)(n - 10 + (int)'A');
        }

        private static string? UrlDecodeInternal(string? value, Encoding encoding) {
            if(value == null) {
                return null;
            }

            int count = value.Length;
            UrlDecoder helper = new UrlDecoder(count, encoding);

            // go through the string's chars collapsing %XX and
            // appending each char as char, with exception of %XX constructs
            // that are appended as bytes

            for(int pos = 0; pos < count; pos++) {
                char ch = value[pos];

                if(ch == '+') {
                    ch = ' ';
                } else if(ch == '%' && pos < count - 2) {
                    int h1 = HexToInt(value[pos + 1]);
                    int h2 = HexToInt(value[pos + 2]);

                    if(h1 >= 0 && h2 >= 0) {     // valid 2 hex chars
                        byte b = (byte)((h1 << 4) | h2);
                        pos += 2;

                        // don't add as char
                        helper.AddByte(b);
                        continue;
                    }
                }

                if((ch & 0xFF80) == 0)
                    helper.AddByte((byte)ch); // 7 bit have to go as bytes because of Unicode
                else
                    helper.AddChar(ch);
            }

            return helper.GetString();
        }

        private class UrlDecoder {
            private readonly int _bufferSize;

            // Accumulate characters in a special array
            private int _numChars;
            private readonly char[] _charBuffer;

            // Accumulate bytes for decoding into characters in a special array
            private int _numBytes;
            private byte[]? _byteBuffer;

            // Encoding to convert chars to bytes
            private readonly Encoding _encoding;

            private void FlushBytes() {
                if(_numBytes > 0 && _byteBuffer != null) {
                    _numChars += _encoding.GetChars(_byteBuffer, 0, _numBytes, _charBuffer, _numChars);
                    _numBytes = 0;
                }
            }

            internal UrlDecoder(int bufferSize, Encoding encoding) {
                _bufferSize = bufferSize;
                _encoding = encoding;

                _charBuffer = new char[bufferSize];
                // byte buffer created on demand
            }

            internal void AddChar(char ch) {
                if(_numBytes > 0)
                    FlushBytes();

                _charBuffer[_numChars++] = ch;
            }

            internal void AddByte(byte b) {
                if(_byteBuffer == null)
                    _byteBuffer = new byte[_bufferSize];

                _byteBuffer[_numBytes++] = b;
            }

            internal string GetString() {
                if(_numBytes > 0)
                    FlushBytes();

                if(_numChars > 0)
                    return new string(_charBuffer, 0, _numChars);
                else
                    return string.Empty;
            }
        }

        private static int HexToInt(char h) {
            return (h >= '0' && h <= '9') ? h - '0' :
            (h >= 'a' && h <= 'f') ? h - 'a' + 10 :
            (h >= 'A' && h <= 'F') ? h - 'A' + 10 :
            -1;
        }
    }
}
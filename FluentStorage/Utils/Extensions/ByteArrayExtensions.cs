namespace FluentStorage.Utils.Extensions {
	using System;
	using Crypto = System.Security.Cryptography;

	/// <summary>
	/// Byte array extensions methods
	/// </summary>
	public static class ByteArrayExtensions {
		private static readonly char[] LowerCaseHexAlphabet = "0123456789abcdef".ToCharArray();
		private static readonly char[] UpperCaseHexAlphabet = "0123456789ABCDEF".ToCharArray();
		private static readonly Crypto.MD5 _md5 = Crypto.MD5.Create();
		private static readonly Crypto.SHA256 _sha256 = Crypto.SHA256.Create();


		/// <summary>
		/// Converts byte array to hexadecimal string
		/// </summary>
		public static string? ToHexString(this byte[]? bytes) {
			return ToHexString(bytes, true);
		}

		private static string? ToHexString(this byte[]? bytes, bool lowerCase) {
			if (bytes == null)
				return null;

			char[] alphabet = lowerCase ? LowerCaseHexAlphabet : UpperCaseHexAlphabet;

			int len = bytes.Length;
			char[] result = new char[len * 2];

			int i = 0;
			int j = 0;

			while (i < len) {
				byte b = bytes[i++];
				result[j++] = alphabet[b >> 4];
				result[j++] = alphabet[b & 0xF];
			}

			return new string(result);
		}

		public static byte[]? MD5(this byte[]? bytes) {
			if (bytes == null)
				return null;

			return _md5.ComputeHash(bytes);
		}

		public static byte[]? SHA256(this byte[]? bytes) {
			if (bytes == null)
				return null;

			return _sha256.ComputeHash(bytes);
		}

		public static byte[]? HMACSHA256(this byte[]? data, byte[] key) {
			if (data == null)
				return null;

#pragma warning disable SYSLIB0045 // Type or member is obsolete
			var alg = Crypto.KeyedHashAlgorithm.Create("HmacSHA256");
#pragma warning restore SYSLIB0045 // Type or member is obsolete
			if (alg == null)
				throw new InvalidOperationException("could not create crypto algorithm!");
			alg.Key = key;
			return alg.ComputeHash(data);
		}
	}
}
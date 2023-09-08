#if !NET16
using System;
using System.IO;
using System.Security.Cryptography;

namespace FluentStorage.Blobs.Sinks.Impl {
	/// <summary>
	/// Provides ITransformSink support for Aes encryption over the obsolete Rijndael
	/// </summary>
	public class AesSymmetricEncryptionSink : ITransformSink {
		private readonly SymmetricAlgorithm _cryptoAlgorithm;

		/// <summary>
		/// 
		/// </summary>
		public AesSymmetricEncryptionSink(string base64Key) {
			_cryptoAlgorithm = Aes.Create();
			_cryptoAlgorithm.Key = Convert.FromBase64String(base64Key);
			_cryptoAlgorithm.GenerateIV();
		}

		/// <summary>
		/// 
		/// </summary>
		public Stream OpenReadStream(string fullPath, Stream parentStream) {
			return new CryptoStream(parentStream, _cryptoAlgorithm.CreateDecryptor(), CryptoStreamMode.Read);
		}

		/// <summary>
		/// 
		/// </summary>
		public Stream OpenWriteStream(string fullPath, Stream parentStream) {
			return new CryptoStream(parentStream, _cryptoAlgorithm.CreateEncryptor(), CryptoStreamMode.Write);
		}
	}
}
#endif
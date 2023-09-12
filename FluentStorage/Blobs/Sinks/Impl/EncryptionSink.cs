#if !NET16
using System;
using System.IO;
using System.Security.Cryptography;

namespace FluentStorage.Blobs.Sinks.Impl {
	/// <summary>
	/// Abstract class to support multiple different symmetric encryption algorithsm
	/// </summary>
	public abstract class EncryptionSink : ITransformSink {
		/// <summary>
		/// the algorithm
		/// </summary>
		protected readonly SymmetricAlgorithm _cryptoAlgorithm;

		/// <summary>
		/// Base constructor for all Symmetric encryption algorithms
		/// </summary>
		/// <param name="algorithm"></param>
		/// <param name="key"></param>
		/// <param name="iv"></param>
		protected EncryptionSink(SymmetricAlgorithm algorithm, string key, string iv = null) {
			_cryptoAlgorithm = algorithm;
			_cryptoAlgorithm.Key = Convert.FromBase64String(key);
			if (!string.IsNullOrWhiteSpace(iv)) {
				_cryptoAlgorithm.IV = Convert.FromBase64String(iv);
			}
			else {
				_cryptoAlgorithm.GenerateIV(); // note this secret will change on every use of the constructor
			}
		}

		/// <summary>
		/// Use to retrieve the IV when using the single parameter constructor
		/// </summary>
		public string Secret => Convert.ToBase64String(_cryptoAlgorithm.IV);

		#region ITransformSink impls

		/// <summary>
		/// 
		/// </summary>
		public Stream OpenReadStream(string fullPath, Stream parentStream) => new CryptoStream(parentStream, _cryptoAlgorithm.CreateDecryptor(), CryptoStreamMode.Read);

		/// <summary>
		/// 
		/// </summary>
		public Stream OpenWriteStream(string fullPath, Stream parentStream) => new CryptoStream(parentStream, _cryptoAlgorithm.CreateEncryptor(), CryptoStreamMode.Write);

		#endregion
	}
}
#endif
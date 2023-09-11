#if !NET16
using System;
using System.Security.Cryptography;

namespace FluentStorage.Blobs.Sinks.Impl {
	/// <summary>
	/// Provides ITransformSink support for Rijndael encryption
	/// </summary>
	[Obsolete("Please use AesSymmetricEncryptionSink instead as Rijndael is obsolete in .Net 6 and above")]
	public class SymmetricEncryptionSink : EncryptionSink, ITransformSink {
		/// <summary>
		/// Items encrypted with this wil be unencryptable except within the same instance of the SymmetricEncryptionSink
		/// </summary>
		public SymmetricEncryptionSink(string key) : base(Rijndael.Create(), key) {
		}

		/// <summary>
		/// Items encrypted with this wil be unencryptable only with both keys the same on each instance of the SymmetricEncryptionSink
		/// </summary>
		/// <param name="key"></param>
		/// <param name="iv"></param>
		public SymmetricEncryptionSink(string key, string iv) : base(Rijndael.Create(), key, iv) {
		}
	}
}
#endif
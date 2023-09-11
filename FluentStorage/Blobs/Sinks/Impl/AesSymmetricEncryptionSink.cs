#if !NET16
using System.Security.Cryptography;

namespace FluentStorage.Blobs.Sinks.Impl {
	/// <summary>
	/// Provides ITransformSink support for Aes encryption over the obsolete Rijndael
	/// </summary>
	public class AesSymmetricEncryptionSink : EncryptionSink, ITransformSink
	{
		/// <summary>
		/// Items encrypted with this wil be unencryptable except within the same instance of the AesSymmetricEncryptionSink
		/// </summary>
		public AesSymmetricEncryptionSink(string key) : base(Aes.Create(), key) {

		}

		/// <summary>
		/// Items encrypted with this wil be unencryptable only with both keys the same on each instance of the AesSymmetricEncryptionSink
		/// </summary>
		/// <param name="key"></param>
		/// <param name="iv"></param>
		public AesSymmetricEncryptionSink(string key, string iv) : base(Aes.Create(), key, iv) {

		}
	}
}
#endif
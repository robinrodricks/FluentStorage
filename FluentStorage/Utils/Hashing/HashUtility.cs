using System;
using System.IO;
using System.Security.Cryptography;
using FluentStorage.Enums;
using FluentStorage.Utils.Extensions;

namespace FluentStorage.Utils.Hashing;

public static class HashUtility {

	/// <summary>
	/// Hash the given byte array and return the checksum as a hex string.
	/// </summary>
	public static string HashBytes(byte[] bytes, StorageHash algorithm) {
		if (bytes == null) {
			throw new ArgumentNullException(nameof(bytes));
		}

		switch (algorithm) {
			case StorageHash.MD5:
#if NET5_0_OR_GREATER
					return MD5.HashData(bytes).ToHexString();
#else
				using (var hash = MD5.Create()) {
					return hash.ComputeHash(bytes).ToHexString();
				}
#endif

			case StorageHash.SHA1:
#if NET5_0_OR_GREATER
					return SHA1.HashData(bytes).ToHexString();
#else
				using (var hash = SHA1.Create()) {
					return hash.ComputeHash(bytes).ToHexString();
				}
#endif

			case StorageHash.SHA256:
#if NET5_0_OR_GREATER
					return SHA256.HashData(bytes).ToHexString();
#else
				using (var hash = SHA256.Create()) {
					return hash.ComputeHash(bytes).ToHexString();
				}
#endif

			case StorageHash.SHA512:
#if NET5_0_OR_GREATER
					return SHA512.HashData(bytes).ToHexString();
#else
				using (var hash = SHA512.Create()) {
					return hash.ComputeHash(bytes).ToHexString();
				}
#endif

			case StorageHash.CRC32:
				return Crc32Hash.Compute(bytes).ToHexString();

			default:
				throw new NotImplementedException($"Unknown hash algorithm: {algorithm}");
		}
	}

	/// <summary>
	/// Hash the given stream and return the checksum as a hex string.
	/// </summary>
	public static string HashStream(Stream stream, StorageHash algorithm) {
		if (stream == null) {
			throw new ArgumentNullException(nameof(stream));
		}

		switch (algorithm) {
			case StorageHash.MD5:
#if NET5_0_OR_GREATER
					return MD5.HashData(stream).ToHexString();
#else
				using (var hash = MD5.Create()) {
					return hash.ComputeHash(stream).ToHexString();
				}
#endif

			case StorageHash.SHA1:
#if NET5_0_OR_GREATER
					return SHA1.HashData(stream).ToHexString();
#else
				using (var hash = SHA1.Create()) {
					return hash.ComputeHash(stream).ToHexString();
				}
#endif

			case StorageHash.SHA256:
#if NET5_0_OR_GREATER
					return SHA256.HashData(stream).ToHexString();
#else
				using (var hash = SHA256.Create()) {
					return hash.ComputeHash(stream).ToHexString();
				}
#endif

			case StorageHash.SHA512:
#if NET5_0_OR_GREATER
					return SHA512.HashData(stream).ToHexString();
#else
				using (var hash = SHA512.Create()) {
					return hash.ComputeHash(stream).ToHexString();
				}
#endif

			case StorageHash.CRC32:
				return Crc32Hash.Compute(stream.ToByteArray()).ToHexString();

			default:
				throw new NotImplementedException($"Unknown hash algorithm: {algorithm}");
		}
	}


}
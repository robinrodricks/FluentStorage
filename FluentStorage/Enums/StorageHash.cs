namespace FluentStorage.Enums;

/// <summary>
/// Hash algorithm to use in `GetObjectChecksum`
/// </summary>
public enum StorageHash {

	/// <summary>
	/// MD5 algorithm. Fast but weak. Supported by many cloud providers and FTP servers.
	/// </summary>
	MD5 = 1,

	/// <summary>
	/// CRC32 or CRC32C algorithm. Fast and strong integrity check.
	/// </summary>
	CRC32 = 2,

	/// <summary>
	/// SHA-1 algorithm. Stronger than MD5. Common on FTP servers.
	/// </summary>
	SHA1 = 3,

	/// <summary>
	/// SHA-256 algorithm. Stronger than MD5. Modern secure cryptographic hash.
	/// </summary>
	SHA256 = 4,

	/// <summary>
	/// SHA-512 algorithm. Stronger than MD5. Ultra-modern secure cryptographic hash.
	/// </summary>
	SHA512 = 5,

}
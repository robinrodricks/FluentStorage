using System.Collections.Generic;
using FluentFTP;
using FluentStorage.Enums;

namespace FluentStorage.FTP.Utils;

public static class FtpHashUtils {

	public static readonly Dictionary<FtpHashAlgorithm, StorageHash> ToFluentStorage = new Dictionary<FtpHashAlgorithm, StorageHash> {
		{ FtpHashAlgorithm.MD5, StorageHash.MD5 },
		{ FtpHashAlgorithm.CRC, StorageHash.CRC32 },
		{ FtpHashAlgorithm.SHA1, StorageHash.SHA1 },
		{ FtpHashAlgorithm.SHA256, StorageHash.SHA256 },
		{ FtpHashAlgorithm.SHA512, StorageHash.SHA512 },
	};

	public static Dictionary<StorageHash, FtpHashAlgorithm> FromFluentStorage = new Dictionary<StorageHash, FtpHashAlgorithm> {
		{ StorageHash.MD5, FtpHashAlgorithm.MD5 },
		{ StorageHash.CRC32, FtpHashAlgorithm.CRC },
		{ StorageHash.SHA1, FtpHashAlgorithm.SHA1 },
		{ StorageHash.SHA256, FtpHashAlgorithm.SHA256 },
		{ StorageHash.SHA512, FtpHashAlgorithm.SHA512 },
	};

}
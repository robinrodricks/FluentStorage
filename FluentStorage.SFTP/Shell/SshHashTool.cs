namespace FluentStorage.SFTP.Shell;

internal enum SshHashTool {
	None,

	// Unix - dedicated *sum tools (GNU coreutils)
	UnixMd5sum, UnixSha1sum, UnixSha256sum, UnixSha512sum,

	// Unix - BSD/macOS
	UnixMd5Bsd,                                  // `md5 -q`
	UnixShasum1, UnixShasum256, UnixShasum512,   // `shasum -a N`

	// Unix - openssl fallback (present almost everywhere)
	UnixOpensslMd5, UnixOpensslSha1, UnixOpensslSha256, UnixOpensslSha512,

	// Unix - CRC32
	UnixCrc32Util,      // `crc32` (libarchive-zip-perl / similar package)
	UnixPython3Crc32,   // python3 -c "...zlib.crc32..."
	UnixPythonCrc32,    // python  -c "...zlib.crc32..."

	// Windows - certutil (no native CRC32 support)
	WinCertutilMd5, WinCertutilSha1, WinCertutilSha256, WinCertutilSha512,

	// Windows - PowerShell Get-FileHash (no native CRC32 support either)
	WinPowershellMd5, WinPowershellSha1, WinPowershellSha256, WinPowershellSha512,

	// Windows - PowerShell manual CRC32 (no built-in cmdlet computes CRC32)
	WinPowershellCrc32
}
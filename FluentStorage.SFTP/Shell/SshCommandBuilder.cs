namespace FluentStorage.SFTP.Shell;

internal static class SshCommandBuilder {

	/// <summary>
	/// Builds a windows/unix command to hash with the given tool.
	/// </summary>
	public static string Build(SshHashTool tool, string remotePath) {
		switch (tool) {
			// Unix dedicated *sum tools
			case SshHashTool.UnixMd5sum: return $"md5sum {SshUtils.UnixQuote(remotePath)}";
			case SshHashTool.UnixSha1sum: return $"sha1sum {SshUtils.UnixQuote(remotePath)}";
			case SshHashTool.UnixSha256sum: return $"sha256sum {SshUtils.UnixQuote(remotePath)}";
			case SshHashTool.UnixSha512sum: return $"sha512sum {SshUtils.UnixQuote(remotePath)}";

			// BSD/macOS
			case SshHashTool.UnixMd5Bsd: return $"md5 -q {SshUtils.UnixQuote(remotePath)}";
			case SshHashTool.UnixShasum1: return $"shasum -a 1 {SshUtils.UnixQuote(remotePath)}";
			case SshHashTool.UnixShasum256: return $"shasum -a 256 {SshUtils.UnixQuote(remotePath)}";
			case SshHashTool.UnixShasum512: return $"shasum -a 512 {SshUtils.UnixQuote(remotePath)}";

			// openssl fallback
			case SshHashTool.UnixOpensslMd5: return $"openssl dgst -md5 {SshUtils.UnixQuote(remotePath)}";
			case SshHashTool.UnixOpensslSha1: return $"openssl dgst -sha1 {SshUtils.UnixQuote(remotePath)}";
			case SshHashTool.UnixOpensslSha256: return $"openssl dgst -sha256 {SshUtils.UnixQuote(remotePath)}";
			case SshHashTool.UnixOpensslSha512: return $"openssl dgst -sha512 {SshUtils.UnixQuote(remotePath)}";

			// CRC32
			case SshHashTool.UnixCrc32Util:
				return $"crc32 {SshUtils.UnixQuote(remotePath)}";
			case SshHashTool.UnixPython3Crc32:
				return "python3 -c \"import zlib,sys; f=open(sys.argv[1],'rb'); " +
				       "print(format(zlib.crc32(f.read()) & 0xFFFFFFFF,'08x'))\" " + SshUtils.UnixQuote(remotePath);
			case SshHashTool.UnixPythonCrc32:
				return "python -c \"import zlib,sys; f=open(sys.argv[1],'rb'); " +
				       "print(format(zlib.crc32(f.read()) & 0xFFFFFFFF,'08x'))\" " + SshUtils.UnixQuote(remotePath);

			// certutil
			case SshHashTool.WinCertutilMd5: return $"certutil -hashfile {SshUtils.CmdQuote(remotePath)} MD5";
			case SshHashTool.WinCertutilSha1: return $"certutil -hashfile {SshUtils.CmdQuote(remotePath)} SHA1";
			case SshHashTool.WinCertutilSha256: return $"certutil -hashfile {SshUtils.CmdQuote(remotePath)} SHA256";
			case SshHashTool.WinCertutilSha512: return $"certutil -hashfile {SshUtils.CmdQuote(remotePath)} SHA512";

			// PowerShell Get-FileHash
			case SshHashTool.WinPowershellMd5:
				return $"powershell -NoProfile -Command \"(Get-FileHash -Algorithm MD5 -LiteralPath {SshUtils.PsQuote(remotePath)}).Hash\"";
			case SshHashTool.WinPowershellSha1:
				return $"powershell -NoProfile -Command \"(Get-FileHash -Algorithm SHA1 -LiteralPath {SshUtils.PsQuote(remotePath)}).Hash\"";
			case SshHashTool.WinPowershellSha256:
				return $"powershell -NoProfile -Command \"(Get-FileHash -Algorithm SHA256 -LiteralPath {SshUtils.PsQuote(remotePath)}).Hash\"";
			case SshHashTool.WinPowershellSha512:
				return $"powershell -NoProfile -Command \"(Get-FileHash -Algorithm SHA512 -LiteralPath {SshUtils.PsQuote(remotePath)}).Hash\"";

			// Manual CRC32 in PowerShell (no built-in cmdlet supports CRC32)
			case SshHashTool.WinPowershellCrc32:
				return "powershell -NoProfile -Command \"" +
				       $"$b=[IO.File]::ReadAllBytes({SshUtils.PsQuote(remotePath)});" +
				       "$c=0xFFFFFFFF;foreach($x in $b){$c=$c -bxor $x;for($i=0;$i -lt 8;$i++){" +
				       "if(($c -band 1) -ne 0){$c=($c -shr 1) -bxor 0xEDB88320}else{$c=$c -shr 1}}};" +
				       "$c=$c -bxor 0xFFFFFFFF;'{0:x8}' -f $c\"";

			default:
				break;
			//throw new NotSupportedException($"No command mapping for {tool}.");
		}
		return null;
	}

}
using System;
using System.Text.RegularExpressions;

namespace FluentStorage.SFTP.Shell;

internal static class SshCommandParser {

	public static string Parse(SshHashTool tool, string rawOutput) {
		if (string.IsNullOrWhiteSpace(rawOutput))
			return null;
		//throw new InvalidOperationException("Hash command returned no output.");

		string output = rawOutput.Trim();

		switch (tool) {
			// "<hash>  <filename>" style output -> first token
			case SshHashTool.UnixMd5sum:
			case SshHashTool.UnixSha1sum:
			case SshHashTool.UnixSha256sum:
			case SshHashTool.UnixSha512sum:
			case SshHashTool.UnixShasum1:
			case SshHashTool.UnixShasum256:
			case SshHashTool.UnixShasum512:
			case SshHashTool.UnixCrc32Util:
				return FirstToken(output).ToLowerInvariant();

			// Bare hex hash, nothing else
			case SshHashTool.UnixMd5Bsd:
			case SshHashTool.UnixPython3Crc32:
			case SshHashTool.UnixPythonCrc32:
			case SshHashTool.WinPowershellMd5:
			case SshHashTool.WinPowershellSha1:
			case SshHashTool.WinPowershellSha256:
			case SshHashTool.WinPowershellSha512:
			case SshHashTool.WinPowershellCrc32:
				return output.ToLowerInvariant();

			// "MD5(file)= <hash>"
			case SshHashTool.UnixOpensslMd5:
			case SshHashTool.UnixOpensslSha1:
			case SshHashTool.UnixOpensslSha256:
			case SshHashTool.UnixOpensslSha512: {
				int eq = output.LastIndexOf('=');
				if (eq < 0) throw new InvalidOperationException($"Unexpected openssl output: {output}");
				return output.Substring(eq + 1).Trim().ToLowerInvariant();
			}

			// certutil multi-line output:
			//   MD5 hash of <file>:
			//   d4 1d 8c d9 8f 00 b2 04 e9 80 09 98 ec f8 42 7e
			//   CertUtil: -hashfile command completed successfully.
			case SshHashTool.WinCertutilMd5:
			case SshHashTool.WinCertutilSha1:
			case SshHashTool.WinCertutilSha256:
			case SshHashTool.WinCertutilSha512: {
				foreach (var rawLine in output.Split('\n')) {
					string line = rawLine.Trim();
					if (line.Length == 0) continue;
					if (line.StartsWith("CertUtil", StringComparison.OrdinalIgnoreCase)) continue;
					if (line.IndexOf("hash of", StringComparison.OrdinalIgnoreCase) >= 0) continue;

					string joined = line.Replace(" ", "");
					if (Regex.IsMatch(joined, "^[0-9a-fA-F]+$"))
						return joined.ToLowerInvariant();
				}
				return null;
				//throw new InvalidOperationException($"Could not parse certutil output: {output}");
			}

			default:
				return null;
			//throw new NotSupportedException($"No parser for {tool}.");
		}
		return null;
	}

	private static string FirstToken(string s) =>
		s.Split(new[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries)[0];


}
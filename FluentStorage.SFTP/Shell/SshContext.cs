using System;
using FluentStorage.Enums;
using Renci.SshNet;

namespace FluentStorage.SFTP.Shell;

/// <summary>
/// Manages a single SSH session for a connected server.
/// Detects hash utilities on a remote SSH server.
/// Can compute a hash of a remote file without downloading it.
/// </summary>
public class SshContext {

	/// <summary>
	/// Connected SSH client
	/// </summary>
	private readonly SshClient _client;

	/// <summary>
	/// Create a new instance of the SSH context which provides ability to remotely compute hashes for any file.
	/// </summary>
	public SshContext(SshClient client) {
		_client = client;
	}

	private SshClient Client() {

		if (!_client.IsConnected) {
			_client.Connect();
		}

		return _client;
	}

	// ---------------- Capability flags ----------------

	/// <summary>True if capabilities were detected.</summary>
	public bool Detected { get; private set; } = false;
	/// <summary>True if we can execute arbitrary commands on the server at all.</summary>
	public bool CanExecuteCommands { get; private set; } = false;

	/// <summary>True if the remote OS was identified as Windows.</summary>
	public bool IsWindows { get; private set; } = false;

	/// <summary>True if the remote OS was identified as Unix/Linux/macOS.</summary>
	public bool IsUnix { get; private set; } = false;

	/// <summary>True if some MD5 utility is available.</summary>
	public bool HasMd5 { get; private set; } = false;

	/// <summary>True if some SHA-1 utility is available.</summary>
	public bool HasSha1 { get; private set; } = false;

	/// <summary>True if some SHA-256 utility is available.</summary>
	public bool HasSha256 { get; private set; } = false;

	/// <summary>True if some SHA-512 utility is available.</summary>
	public bool HasSha512 { get; private set; } = false;

	/// <summary>True if some CRC32 utility is available.</summary>
	public bool HasCrc32 { get; private set; } = false;


	// ---------------- Internal tool resolution ----------------
	// We resolve which concrete tool/command style to use once, during
	// DetectCapabilities(), so GetHashUsingSsh() doesn't have to re-probe.


	private SshHashTool _md5Tool = SshHashTool.None;
	private SshHashTool _sha1Tool = SshHashTool.None;
	private SshHashTool _sha256Tool = SshHashTool.None;
	private SshHashTool _sha512Tool = SshHashTool.None;
	private SshHashTool _crc32Tool = SshHashTool.None;

	/// <summary>
	/// Probes the remote server: whether commands can be run at all, what OS it is,
	/// and which hashing utilities are installed. Populates all capability flags.
	/// Call this once (e.g. after connecting) before calling GetHashUsingSsh.
	/// </summary>
	private void DetectCapabilities() {

		if (!Detected) {
			Detected = true;

			CanExecuteCommands = Execute("echo capcheck", out string echoOut)
			                     && echoOut.Contains("capcheck");

			if (!CanExecuteCommands) {
				IsWindows = false;
				IsUnix = false;
				return;
			}

			DetectOperatingSystem();

			if (IsUnix)
				DetectUnixUtilities();
			else if (IsWindows)
				DetectWindowsUtilities();
			// else: unrecognized OS -> all Has* flags remain false

		}
	}

	private void DetectOperatingSystem() {
		// Unix: `uname -s` succeeds pretty much everywhere on Unix/Linux/macOS/BSD.
		if (Execute("uname -s", out string unameOut) && !string.IsNullOrWhiteSpace(unameOut)) {
			IsUnix = true;
			return;
		}

		// Windows via cmd.exe
		if (Execute("cmd /c ver", out string verOut)
		    && verOut.IndexOf("Microsoft", StringComparison.OrdinalIgnoreCase) >= 0) {
			IsWindows = true;
			return;
		}

		// Windows via PowerShell-only SSH subsystems (no cmd.exe access)
		if (Execute("powershell -NoProfile -Command \"$PSVersionTable.PSVersion.Major\"", out string psOut)
		    && !string.IsNullOrWhiteSpace(psOut)) {
			IsWindows = true;
		}
	}

	private void DetectUnixUtilities() {
		bool md5sum = UnixCommandExists("md5sum");
		bool sha1sum = UnixCommandExists("sha1sum");
		bool sha256sum = UnixCommandExists("sha256sum");
		bool sha512sum = UnixCommandExists("sha512sum");

		bool md5Bsd = !md5sum && UnixCommandExists("md5");          // macOS/BSD
		bool shasum = UnixCommandExists("shasum");                   // macOS fallback for sha1/256/512
		bool openssl = UnixCommandExists("openssl");

		bool crc32Util = UnixCommandExists("crc32");
		bool python3 = UnixCommandExists("python3");
		bool python = !python3 && UnixCommandExists("python");

		// MD5
		if (md5sum) { _md5Tool = SshHashTool.UnixMd5sum; HasMd5 = true; }
		else if (md5Bsd) { _md5Tool = SshHashTool.UnixMd5Bsd; HasMd5 = true; }
		else if (openssl) { _md5Tool = SshHashTool.UnixOpensslMd5; HasMd5 = true; }

		// SHA1
		if (sha1sum) { _sha1Tool = SshHashTool.UnixSha1sum; HasSha1 = true; }
		else if (shasum) { _sha1Tool = SshHashTool.UnixShasum1; HasSha1 = true; }
		else if (openssl) { _sha1Tool = SshHashTool.UnixOpensslSha1; HasSha1 = true; }

		// SHA256
		if (sha256sum) { _sha256Tool = SshHashTool.UnixSha256sum; HasSha256 = true; }
		else if (shasum) { _sha256Tool = SshHashTool.UnixShasum256; HasSha256 = true; }
		else if (openssl) { _sha256Tool = SshHashTool.UnixOpensslSha256; HasSha256 = true; }

		// SHA512
		if (sha512sum) { _sha512Tool = SshHashTool.UnixSha512sum; HasSha512 = true; }
		else if (shasum) { _sha512Tool = SshHashTool.UnixShasum512; HasSha512 = true; }
		else if (openssl) { _sha512Tool = SshHashTool.UnixOpensslSha512; HasSha512 = true; }

		// CRC32 - deliberately NOT using `cksum` here: POSIX cksum uses a different
		// CRC-32 polynomial/variant than the common CRC-32 (zip/zlib/ethernet) that
		// most callers expect, so it would silently produce "wrong" values.
		if (crc32Util) { _crc32Tool = SshHashTool.UnixCrc32Util; HasCrc32 = true; }
		else if (python3) { _crc32Tool = SshHashTool.UnixPython3Crc32; HasCrc32 = true; }
		else if (python) { _crc32Tool = SshHashTool.UnixPythonCrc32; HasCrc32 = true; }
	}

	private void DetectWindowsUtilities() {
		bool certutil = WindowsCommandExists("certutil");
		bool powershell = Execute("powershell -NoProfile -Command \"$PSVersionTable.PSVersion.Major\"", out string psOut)
		                  && !string.IsNullOrWhiteSpace(psOut);

		if (certutil) {
			_md5Tool = SshHashTool.WinCertutilMd5; HasMd5 = true;
			_sha1Tool = SshHashTool.WinCertutilSha1; HasSha1 = true;
			_sha256Tool = SshHashTool.WinCertutilSha256; HasSha256 = true;
			_sha512Tool = SshHashTool.WinCertutilSha512; HasSha512 = true;
		}

		if (powershell) {
			if (!HasMd5) { _md5Tool = SshHashTool.WinPowershellMd5; HasMd5 = true; }
			if (!HasSha1) { _sha1Tool = SshHashTool.WinPowershellSha1; HasSha1 = true; }
			if (!HasSha256) { _sha256Tool = SshHashTool.WinPowershellSha256; HasSha256 = true; }
			if (!HasSha512) { _sha512Tool = SshHashTool.WinPowershellSha512; HasSha512 = true; }

			// Neither certutil nor Get-FileHash support CRC32, so we compute it
			// manually with an inline PowerShell script. Works everywhere PowerShell
			// is available, but is slow (byte-by-byte) on very large files.
			_crc32Tool = SshHashTool.WinPowershellCrc32; HasCrc32 = true;
		}
	}



	/// <summary>
	/// Computes the hash of a remote file by its absolute path on the server, using whichever utility was found.
	/// Returns the hash as a lowercase hex string.
	/// Returns null if the hash cannot be computed.
	/// No exceptions are thrown.
	/// </summary>
	public string GetRemoteHash(string remotePath, StorageHash hash) {

		// detect which OS we are on and which hashing utilities are available
		DetectCapabilities();

		// sanity checks
		if (string.IsNullOrWhiteSpace(remotePath))
			return null;//throw new ArgumentException("remotePath is required.", nameof(remotePath));

		if (!Detected)
			return null;//throw new InvalidOperationException("Call DetectCapabilities() first.");

		if (!CanExecuteCommands)
			return null;//throw new NotSupportedException("This server does not support command execution.");

		// get a tool
		SshHashTool tool = hash switch {
			StorageHash.MD5 => _md5Tool,
			StorageHash.SHA1 => _sha1Tool,
			StorageHash.SHA256 => _sha256Tool,
			StorageHash.SHA512 => _sha512Tool,
			StorageHash.CRC32 => _crc32Tool,
			_ => SshHashTool.None
		};

		// exit if no tool found
		if (tool == SshHashTool.None)
			return null;
		//throw new NotSupportedException($"No utility available to compute {hash} on this server.");

		// build the command and exit if cannot build
		string command = SshCommandBuilder.Build(tool, remotePath);
		if (command == null) return null;

		// run it and exit if failed
		var result = _client.RunCommand(command);
		if (result.ExitStatus != 0)
			return null;
		//throw new InvalidOperationException($"Hash command failed (exit {result.ExitStatus}) for {remotePath}: {result.Error}");

		// parse it
		return SshCommandParser.Parse(tool, result.Result);
	}


	private bool UnixCommandExists(string toolName) {
		// `command -v` is POSIX and works in every Unix shell; avoids relying on `which`.
		string cmd = $"command -v {toolName} >/dev/null 2>&1 && echo FOUND";
		return Execute(cmd, out string output) && output.Contains("FOUND");
	}

	private bool WindowsCommandExists(string toolName) {
		string cmd = $"cmd /c where {toolName} 2>nul";
		return Execute(cmd, out string output)
		       && !string.IsNullOrWhiteSpace(output)
		       && output.IndexOf("Could not find", StringComparison.OrdinalIgnoreCase) < 0;
	}

	private bool Execute(string command, out string output) {

		try {
			var cmd = Client().RunCommand(command);
			output = cmd.Result ?? string.Empty;
			return cmd.ExitStatus == 0;
		}
		catch {
			output = string.Empty;
			return false;
		}
	}

}
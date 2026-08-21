namespace FluentStorage.SFTP.Shell;

/// <summary>
/// Validates the security of running an SSH command with given path.
/// Originally from FluentFTP `SanitizerModule`
/// </summary>
internal class SshSecurityValidator {

	/// <summary>
	/// Returns true if the given SFTP remote file path is "safe" to run in SSH commands.
	/// Performs many security corrections to the path to prevent path-injection and command-injection.
	/// </summary>
	public static bool IsPathSafe(string path) {
		if (string.IsNullOrEmpty(path)) {
			return true;
		}

		// Detect URL encoding
		if (path.IndexOf('%') >= 0) {
			return false;
		}

		// Remove control chars and newlines
		if (ContainsControlChars(path)) {
			return false;
		}

		// Remove unicode spoofing chars
		if (ContainsUnicodeControl(path)) {
			return false;
		}

		// All OK!
		return true;
	}

	/// <summary>Checks for any control chars, newlines and command delimiters</summary>
	private static bool ContainsControlChars(string path) {
		for (int i = 0; i < path.Length; i++) {
			char c = path[i];

			// single condition: control chars, unix-command delimiters, newlines (CR / LF)
			if (c < 32 || c == 127 || c == ';' || c == '|')
				return true;
		}
		return false;
	}


	/// <summary>Checks unicode control chars</summary>
	private static bool ContainsUnicodeControl(string path) {
		for (int i = 0; i < path.Length; i++) {
			char c = path[i];
			if ((c >= '\u202A' && c <= '\u202E') || (c >= '\u2066' && c <= '\u2069'))
				return true;
		}
		return false;
	}

}
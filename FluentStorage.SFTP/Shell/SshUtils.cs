namespace FluentStorage.SFTP.Shell;

internal static class SshUtils {

	public static string UnixQuote(string path) =>
		"'" + path.Replace("'", "'\\''") + "'";

	public static string CmdQuote(string path) =>
		"\"" + path.Replace("\"", "\\\"") + "\"";

	public static string PsQuote(string path) =>
		"'" + path.Replace("'", "''") + "'";

}
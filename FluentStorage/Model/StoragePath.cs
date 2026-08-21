using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace FluentStorage;

/// <summary>
/// A simple unified path system designed to work across all storage providers, without having to fiddle with path intracacies.
/// Suitable for all cloud storage like S3, S3-compatible, Azure Blob, GCP, R2, MinIO and more.
/// Suitable for file systems like local disk, FTP, SFTP.
/// </summary>
public static class StoragePath {
	/// <summary>
	/// Character used to split paths 
	/// </summary>
	public const char PathSeparator = '/';

	/// <summary>
	/// Character used to split paths as a string value
	/// </summary>
	public static readonly string PathSeparatorString = new string(PathSeparator, 1);

	/// <summary>
	/// Folder name for leveling up the path
	/// </summary>
	public static readonly string LevelUpFolderName = "..";


	/// <summary>
	/// Combines multiple parts of path
	/// </summary>
	public static string Combine(params string[] parts) {
		return Combine((IEnumerable<string>)parts);
	}

	/// <summary>
	/// Combines path parts using the storage path separator.
	/// Null and empty parts are ignored.
	/// Fast path for combining only parts.
	/// </summary>
	public static string Combine(string part1, string part2) {

		// fast blank handling
		if (string.IsNullOrEmpty(part1))
			return string.IsNullOrEmpty(part2) ? string.Empty : NormalizePart(part2);

		if (string.IsNullOrEmpty(part2))
			return NormalizePart(part1);

		// normalize
		string p1 = NormalizePart(part1);
		string p2 = NormalizePart(part2);

		// preserve existing behavior
		if (p1.Length == 0)
			return p2;

		if (p2.Length == 0)
			return p1;

		return p1 + PathSeparator + p2;
	}

	/// <summary>
	/// Combines path parts using the storage path separator.
	/// Null and empty parts are ignored.
	/// </summary>
	public static string Combine(IEnumerable<string> parts) {
		if (parts == null)
			return string.Empty;

		var sb = new StringBuilder();
		bool first = true;

		foreach (string part in parts) {
			if (string.IsNullOrEmpty(part))
				continue;

			string normalized = NormalizePart(part);
			if (normalized.Length == 0)
				continue;

			if (!first)
				sb.Append(PathSeparator);

			sb.Append(normalized);
			first = false;
		}

		return sb.ToString();
	}

	/// <summary>
	/// Splits a path into normalized parts.
	/// </summary>
	public static string[] Split(string path) {
		if (path == null)
			return null;

		path = Normalize(path);

		return path.Length == 0
			? Array.Empty<string>()
			: path.Split(PathSeparator);
	}

	/// <summary>
	/// Splits a path into parts WITHOUT normalization. Returns a list with the entire path if no slashes are found.
	/// </summary>
	public static string[] SplitFast(this string path) {
		if (path.Contains("/")) {
			return path.Split('/');
		}
		else if (path.Contains("\\")) {
			return path.Split('\\');
		}
		else {
			return new string[] { path };
		}
	}

	/// <summary>
	/// Gets the parent path.
	/// </summary>
	public static string GetParent(string path) {
		if (path == null)
			return null;

		path = Normalize(path);

		if (path.Length == 0)
			return null;

		int last = path.LastIndexOf(PathSeparator);

		return last < 0
			? string.Empty
			: path.Substring(0, last);
	}

	/// <summary>
	/// Normalizes any file or object path into a simple unified path system.
	/// Suitable for all cloud storage like S3, S3-compatible, Azure Blob, GCP, R2, MinIO and more.
	/// Suitable for file systems like local disk, FTP, SFTP.
	///
	/// Rules:
	/// 1. Uses '/' as the path separator.
	/// 2. Converts '\' to '/'.
	/// 3. Removes any leading and trailing separator.
	/// 4. Collapses duplicate separators.
	/// 5. Preserves '.' and '..' path segments.
	/// 6. Returns an empty string for null, empty or root paths.
	/// </summary>
	public static string Normalize(string path) {

		/*
		- null                     -> ""
		- ""                       -> ""
		- "/"                      -> ""
		- "folder"                 -> "folder"
		- "/folder/"               -> "folder"
		- "\\folder\\file.txt"     -> "folder/file.txt"
		- "folder//sub///file.txt" -> "folder/sub/file.txt"
		*/

		if (string.IsNullOrEmpty(path))
			return string.Empty;

		var sb = new StringBuilder(path.Length);

		bool previousSlash = true;

		foreach (char c in path) {
			char ch = c == '\\' ? '/' : c;

			if (ch == '/') {
				if (previousSlash)
					continue;

				previousSlash = true;
			}
			else {
				previousSlash = false;
			}

			sb.Append(ch);
		}

		if (sb.Length > 0 && sb[sb.Length - 1] == '/')
			sb.Length--;

		return sb.ToString();
	}

	/// <summary>
	/// Normalizes path part by removing any leading or trailing slashes.
	/// </summary>
	public static string NormalizePart(string part) {
		if (part == null) throw new ArgumentNullException(nameof(part));

		return part.Trim('/', '\\');
	}

	/// <summary>
	/// Checks if path is root folder path, which can be an empty string, null, or the actual root path.
	/// </summary>
	public static bool IsRootPath(string path) {
		return string.IsNullOrEmpty(path) || path == "\\" || path == "/";
	}

	/// <summary>
	/// Return the relative path of the given absolute disk path. Uses native .NET API if available.
	/// </summary>
	/// <param name="basePath">The base storage path.</param>
	/// <param name="fullPath">The full storage path to make relative.</param>
	public static string GetRelativeDiskPath(string basePath, string fullPath) {
#if NETCOREAPP2_0_OR_GREATER || NETSTANDARD2_1_OR_GREATER
		return Path.GetRelativePath(basePath, fullPath);
#else
			basePath = Path.GetFullPath(basePath);
			fullPath = Path.GetFullPath(fullPath);

			if (!basePath.EndsWith(Path.DirectorySeparatorChar.ToString()) &&
				!basePath.EndsWith(Path.AltDirectorySeparatorChar.ToString())) {
				basePath += Path.DirectorySeparatorChar;
			}

			if (!fullPath.StartsWith(basePath, StringComparison.OrdinalIgnoreCase))
				throw new ArgumentException($"'{fullPath}' is not under '{basePath}'.");

			return fullPath.Substring(basePath.Length);
#endif
	}

	/// <summary>
	/// Returns the relative path of the given cloud object path.
	/// </summary>
	/// <param name="basePath">The base storage path.</param>
	/// <param name="fullPath">The full storage path to make relative.</param>
	public static string GetRelativeCloudPath(string basePath, string fullPath) {
		basePath = Normalize(basePath);
		fullPath = Normalize(fullPath);

		if (!basePath.EndsWith("/"))
			basePath += "/";

		if (!fullPath.StartsWith(basePath, StringComparison.OrdinalIgnoreCase))
			return "";// throw new ArgumentException($"'{fullPath}' is not contained within '{basePath}'.");

		return fullPath.Substring(basePath.Length);
	}

}
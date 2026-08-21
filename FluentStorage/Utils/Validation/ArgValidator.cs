using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using FluentStorage.Model;

namespace FluentStorage.Utils.Validation;

/// <summary>
/// A collection of generic library wise validations
/// </summary>
public static class ArgValidator {
	private const int MaxBlobPrefixLength = 50;

	/// <summary>
	/// Validates prefix length
	/// </summary>
	public static void AssertPrefix(string prefix) {
		if (prefix == null) return;

		string[] parts = prefix.Split('/');

		foreach (string part in parts) {
			if (part.Length > MaxBlobPrefixLength)
				throw new ArgumentException(
					string.Format("blob prefix cannot exceed {0} characters", MaxBlobPrefixLength),
					nameof(prefix));
		}
	}

	/// <summary>
	/// Checks blob full path for generic rules
	/// </summary>
	public static void AssertFullPaths(IEnumerable<string> fullPaths) {
		if (fullPaths == null) return;

		foreach (string fullPath in fullPaths) {
			if (fullPath == null) throw new ArgumentNullException(nameof(fullPath));
		}
	}

	/// <summary>
	/// Checks blob full path for generic rules
	/// </summary>
	public static void AssertFullPaths(IEnumerable<StoreObject> blobs) {
		if (blobs == null)
			return;

		AssertFullPaths(blobs.Select(b => b.FullPath));
	}

	/// <summary>
	/// Checks source stream for generic rules
	/// </summary>
	public static void AssertInputStream(Stream inputStream) {
		if (inputStream == null) throw new ArgumentNullException(nameof(inputStream));

		try {
			long l = inputStream.Length;
		}
		catch (NotSupportedException ex) {
			throw new ArgumentException("stream must support getting a length", nameof(inputStream), ex);
		}

	}
}
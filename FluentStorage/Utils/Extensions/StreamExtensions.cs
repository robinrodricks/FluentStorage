namespace FluentStorage.Utils.Extensions;

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

/// <summary>
/// <see cref="Stream"/> extension
/// </summary>
public static class StreamExtensions {

	/// <summary>
	/// Attemps to get the size of this stream by reading the Length property, otherwise returns 0.
	/// </summary>
	public static bool TryGetSize(this Stream s, out long size) {
		try {
			size = s.Length;
			return true;
		}
		catch (NotSupportedException) {

		}
		catch (ObjectDisposedException) {

		}

		size = 0;
		return false;
	}

	/// <summary>
	/// Attemps to get the size of this stream by reading the Length property, otherwise returns 0.
	/// </summary>
	public static long? TryGetSize(this Stream s) {
		long size;
		if (TryGetSize(s, out size)) {
			return size;
		}

		return null;
	}



	/// <summary>
	/// Reads the stream until a specified sequence of bytes is reached.
	/// </summary>
	/// <returns>Bytes before the stop sequence</returns>
	public static byte[] ReadUntil(this Stream s, byte[] stopSequence) {
		byte[] buf = new byte[1];
		var result = new List<byte>(50);
		int charsMatched = 0;

		while (s.Read(buf, 0, 1) == 1) {
			byte b = buf[0];
			result.Add(b);

			if (b == stopSequence[charsMatched]) {
				if (++charsMatched == stopSequence.Length) {
					break;
				}
			}
			else {
				charsMatched = 0;
			}

		}
		return result.ToArray();
	}



	/// <summary>
	/// Reads all stream in memory and returns as byte array
	/// </summary>
	public static byte[]? ToByteArray(this Stream? stream) {
		if (stream == null)
			return null;
		using (var ms = new MemoryStream()) {
			stream.CopyTo(ms);
			return ms.ToArray();
		}
	}

	/// <summary>
	/// Converts the stream to string using specified encoding. This is done by reading the stream into
	/// byte array first, then applying specified encoding on top.
	/// </summary>
	public static string? ToString(this Stream? stream, Encoding encoding) {
		if (stream == null)
			return null;
		if (encoding == null)
			throw new ArgumentNullException(nameof(encoding));

		using (StreamReader reader = new StreamReader(stream, encoding)) {
			return reader.ReadToEnd();
		}
	}

}
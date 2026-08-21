using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace FluentStorage.Streaming;

/// <summary>
/// Fixes common issues in streams that different implementations are really silly about
/// </summary>
public class StorageSourceStream : Stream {
	private readonly Stream _origin;
	private bool _noRead = true;

	/// <summary>
	/// 
	/// </summary>
	public StorageSourceStream(Stream origin) {
		_origin = origin ?? throw new ArgumentNullException(nameof(origin));
	}

	/// <summary>
	/// No change
	/// </summary>
	public override bool CanRead => _origin.CanRead;

	/// <summary>
	/// Always true
	/// </summary>
	public override bool CanSeek => true;

	/// <summary>
	/// Always false
	/// </summary>
	public override bool CanWrite => false;

	/// <summary>
	/// No change
	/// </summary>
	public override long Length => _origin.Length;

	/// <summary>
	/// No change
	/// </summary>
	public override long Position { get => _origin.Position; set => _origin.Position = value; }

	/// <summary>
	/// No change
	/// </summary>
	public override void Flush() => _origin.Flush();

	/// <summary>
	/// see <see cref="ReadAsync(byte[], int, int, CancellationToken)"/>
	/// </summary>
	public override int Read(byte[] buffer, int offset, int count) => ReadAsync(buffer, offset, count).GetAwaiter().GetResult();

	/// <summary>
	/// No change, but remembers that read was performed
	/// </summary>
	public override Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken = default) {
		_noRead = false;

		return _origin.ReadAsync(buffer, offset, count, cancellationToken);
	}

	/// <summary>
	/// Only allows seeks to beginning if no reads were performed
	/// </summary>
	public override long Seek(long offset, SeekOrigin origin) {
		if (_noRead && offset == 0 && origin == SeekOrigin.Begin) {
			// FIX #132: Some SDKs call Seek(0, Begin) before first read. Make sure we really rewind.
			if (_origin.CanSeek) {
				return _origin.Seek(0, SeekOrigin.Begin);
			}
			return 0;
		}

		throw new NotSupportedException();
	}

	/// <summary>
	/// Change to "not supported"
	/// </summary>
	public override void SetLength(long value) => throw new NotSupportedException();

	/// <summary>
	/// Change to "not supported"
	/// </summary>
	public override void Write(byte[] buffer, int offset, int count) => throw new NotSupportedException();

	/// <summary>
	/// No change
	/// </summary>
	/// <param name="disposing"></param>
	protected override void Dispose(bool disposing) {
		_origin.Dispose();
	}
}
using System;
using System.Buffers;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using FluentStorage.Storage;

namespace FluentStorage.Streaming;

/// <summary>
/// A read-only, seekable Stream over an `IStore` object that never downloads more than
/// one buffer's worth of data at a time and never buffers the whole file.
///
/// Seeking is "free" — it only moves the logical position. Actual network activity
/// (a call to `OpenRange` happens lazily, only when a read requires
/// bytes that aren't already in the in-memory buffer.
///
/// The remote stream returned by `OpenRange` is fully drained into the buffer and
/// disposed immediately; it is never held open.
///
/// Any exception from `OpenRange` is wrapped in `IOException` except `OperationCanceledException`.
/// </summary>
public sealed class SeekableStream : Stream {
	private readonly IStore _store;
	private readonly string _path;
	private readonly int _bufferSize;
	private readonly SemaphoreSlim _gate = new(1, 1);

	private byte[] _buffer;          // rented from ArrayPool<byte>, lives for stream lifetime
	private long _bufferStart;       // file offset the buffer starts at
	private int _bufferLength;       // number of valid bytes currently in _buffer (from _bufferStart)

	private long _position;
	private long? _knownLength;      // discovered lazily once a short/empty read reveals EOF

	private bool _disposed;

	/// <param name="store">Backing store.</param>
	/// <param name="path">Full path of the object to read.</param>
	/// <param name="bufferSize">Chunk size to request from <see cref="IStore.OpenRange"/> per fetch.</param>
	/// <param name="knownLength">
	/// Optional total object length, if the caller already knows it (e.g. from metadata).
	/// If omitted, length is discovered lazily and <see cref="Length"/> / seeking from
	/// <see cref="SeekOrigin.End"/> will throw <see cref="NotSupportedException"/> until then.
	/// </param>
	public SeekableStream(IStore store, string path, int bufferSize = 65536, long? knownLength = null) {
		_store = store ?? throw new ArgumentNullException(nameof(store));
		_path = path ?? throw new ArgumentNullException(nameof(path));

		if (bufferSize <= 0)
			throw new ArgumentOutOfRangeException(nameof(bufferSize), "Buffer size must be positive.");

		_bufferSize = bufferSize;
		_buffer = ArrayPool<byte>.Shared.Rent(bufferSize);
		_bufferStart = 0;
		_bufferLength = 0;
		_position = 0;
		_knownLength = knownLength;
	}

	public override bool CanRead => true;
	public override bool CanSeek => true;
	public override bool CanWrite => false;
	public override bool CanTimeout => false;

	public override long Length {
		get {
			CheckDisposed();
			return _knownLength ?? throw new NotSupportedException(
				"Object length is not yet known. It is discovered lazily once EOF is reached by reading.");
		}
	}

	public override long Position {
		get {
			CheckDisposed();
			return _position;
		}
		set => Seek(value, SeekOrigin.Begin);
	}

	/// <summary>
	/// Only moves the logical read position. Never triggers a network request —
	/// the buffer is (re)fetched lazily on the next <see cref="Read"/>/<see cref="ReadAsync"/>.
	/// </summary>
	public override long Seek(long offset, SeekOrigin origin) {
		CheckDisposed();

		long newPosition = origin switch {
			SeekOrigin.Begin => offset,
			SeekOrigin.Current => _position + offset,
			SeekOrigin.End => _knownLength.HasValue
				? _knownLength.Value + offset
				: throw new NotSupportedException(
					"Cannot seek relative to end: object length is not yet known."),
			_ => throw new ArgumentOutOfRangeException(nameof(origin), origin, "Unrecognized SeekOrigin.")
		};

		if (newPosition < 0)
			throw new IOException("Attempted to seek before the beginning of the stream.");

		if (_knownLength.HasValue && newPosition > _knownLength.Value) {
			// Seeking past EOF is legal for Stream (matches FileStream behavior);
			// the next Read will simply return 0.
		}

		_position = newPosition;
		return _position;
	}

	public override int Read(byte[] buffer, int offset, int count)
		=> ReadAsync(buffer, offset, count, CancellationToken.None).GetAwaiter().GetResult();

#if NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
	public override int Read(Span<byte> buffer) {
		// Bridge via a temporary array only for the span overload; core logic stays array-based
		// to avoid extra allocations on the hot (array) path used by most callers.
		var rented = ArrayPool<byte>.Shared.Rent(buffer.Length);
		try {
			int read = Read(rented, 0, buffer.Length);
			rented.AsSpan(0, read).CopyTo(buffer);
			return read;
		}
		finally {
			ArrayPool<byte>.Shared.Return(rented);
		}
	}
#endif

	public override async Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken) {
		CheckDisposed();
		ValidateBufferArgs(buffer, offset, count);

		if (count == 0)
			return 0;

		if (_knownLength.HasValue && _position >= _knownLength.Value)
			return 0; // at/after EOF

		await _gate.WaitAsync(cancellationToken).ConfigureAwait(false);
		try {
			int totalCopied = 0;

			while (totalCopied < count) {
				if (_knownLength.HasValue && _position >= _knownLength.Value)
					break; // hit EOF mid-fill; return what we have

				if (!IsPositionBuffered(_position)) {
					await FillBufferAsync(_position, cancellationToken).ConfigureAwait(false);

					if (_bufferLength == 0)
						break; // OpenRange returned nothing at this offset -> EOF
				}

				long offsetIntoBuffer = _position - _bufferStart;
				int available = (int)(_bufferLength - offsetIntoBuffer);
				if (available <= 0)
					break; // shouldn't happen given IsPositionBuffered check, but guard anyway

				int toCopy = Math.Min(available, count - totalCopied);
				Buffer.BlockCopy(_buffer, (int)offsetIntoBuffer, buffer, offset + totalCopied, toCopy);

				totalCopied += toCopy;
				_position += toCopy;

				// Stop after satisfying from a single buffered chunk per loop iteration is fine;
				// loop continues naturally if more data is needed and the next chunk must be fetched.
				if (_knownLength.HasValue && _position >= _knownLength.Value)
					break;
			}

			return totalCopied;
		}
		finally {
			_gate.Release();
		}
	}

	private bool IsPositionBuffered(long position) {
		return _bufferLength > 0
		       && position >= _bufferStart
		       && position < _bufferStart + _bufferLength;
	}

	/// <summary>
	/// Fetches one chunk starting at <paramref name="position"/> from the store, replacing
	/// whatever was previously in the buffer. The remote stream is fully drained and disposed
	/// before this method returns — nothing is left open.
	/// </summary>
	private async Task FillBufferAsync(long position, CancellationToken cancellationToken) {
		Stream remote;
		try {
			remote = await _store.OpenRange(_path, position, _bufferSize, cancellationToken)
				.ConfigureAwait(false);
		}
		catch (OperationCanceledException) {
			throw;
		}
		catch (Exception ex) {
			throw new IOException($"Failed to open range at offset {position} for '{_path}'.", ex);
		}

		try {
			int filled = 0;
			while (filled < _bufferSize) {
				int read;
				try {
					read = await remote.ReadAsync(_buffer, filled, _bufferSize - filled, cancellationToken)
						.ConfigureAwait(false);
				}
				catch (OperationCanceledException) {
					throw;
				}
				catch (Exception ex) {
					throw new IOException($"Failed while reading range at offset {position} for '{_path}'.", ex);
				}

				if (read == 0)
					break; // remote exhausted (EOF)

				filled += read;
			}

			_bufferStart = position;
			_bufferLength = filled;

			if (filled == 0) {
				// Requested at/after EOF.
				_knownLength = position;
			}
			else if (filled < _bufferSize) {
				// Short read -> this chunk ran into EOF.
				_knownLength = position + filled;
			}
		}
		finally {
			// Never keep the network/storage stream open after buffering.
			remote.Dispose();
		}
	}

	public override void Flush() {
		// No-op: read-only stream, nothing to flush.
	}

	public override Task FlushAsync(CancellationToken cancellationToken) => Task.CompletedTask;

	public override void SetLength(long value)
		=> throw new NotSupportedException($"{nameof(SeekableStream)} is read-only.");

	public override void Write(byte[] buffer, int offset, int count)
		=> throw new NotSupportedException($"{nameof(SeekableStream)} is read-only.");

	private static void ValidateBufferArgs(byte[] buffer, int offset, int count) {
		if (buffer is null) throw new ArgumentNullException(nameof(buffer));
		if (offset < 0) throw new ArgumentOutOfRangeException(nameof(offset));
		if (count < 0) throw new ArgumentOutOfRangeException(nameof(count));
		if (buffer.Length - offset < count) throw new ArgumentException("Invalid offset/count for the given buffer.");
	}

	private void CheckDisposed() {
		if (_disposed)
			throw new ObjectDisposedException(nameof(SeekableStream));
	}

	protected override void Dispose(bool disposing) {
		if (_disposed)
			return;

		_disposed = true;

		if (disposing) {
			if (_buffer is not null) {
				ArrayPool<byte>.Shared.Return(_buffer);
				_buffer = null!;
			}
			_gate.Dispose();
		}

		base.Dispose(disposing);
	}
}
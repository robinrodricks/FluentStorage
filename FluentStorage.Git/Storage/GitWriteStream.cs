using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace FluentStorage.Git.Storage;

/// <summary>
/// A write stream that commits the store's changes when the stream is disposed.
/// </summary>
internal sealed class GitWriteStream : Stream {
	private readonly Stream _inner;
	private readonly Action _onDispose;

	public GitWriteStream(Stream inner, Action onDispose) {
		_inner = inner ?? throw new ArgumentNullException(nameof(inner));
		_onDispose = onDispose;
	}

	public override bool CanRead => _inner.CanRead;
	public override bool CanSeek => _inner.CanSeek;
	public override bool CanWrite => _inner.CanWrite;
	public override long Length => _inner.Length;

	public override long Position {
		get => _inner.Position;
		set => _inner.Position = value;
	}

	public override void Flush() => _inner.Flush();

	public override Task FlushAsync(CancellationToken cancellationToken) => _inner.FlushAsync(cancellationToken);

	public override int Read(byte[] buffer, int offset, int count) => _inner.Read(buffer, offset, count);

	public override int Read(Span<byte> buffer) => _inner.Read(buffer);

	public override Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
		=> _inner.ReadAsync(buffer, offset, count, cancellationToken);

	public override ValueTask<int> ReadAsync(Memory<byte> buffer, CancellationToken cancellationToken = default)
		=> _inner.ReadAsync(buffer, cancellationToken);

	public override long Seek(long offset, SeekOrigin origin) => _inner.Seek(offset, origin);

	public override void SetLength(long value) => _inner.SetLength(value);

	public override void Write(byte[] buffer, int offset, int count) => _inner.Write(buffer, offset, count);

	public override void Write(ReadOnlySpan<byte> buffer) => _inner.Write(buffer);

	public override Task WriteAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
		=> _inner.WriteAsync(buffer, offset, count, cancellationToken);

	public override ValueTask WriteAsync(ReadOnlyMemory<byte> buffer, CancellationToken cancellationToken = default)
		=> _inner.WriteAsync(buffer, cancellationToken);

	public override Task CopyToAsync(Stream destination, int bufferSize, CancellationToken cancellationToken)
		=> _inner.CopyToAsync(destination, bufferSize, cancellationToken);

	public override async ValueTask DisposeAsync() {
		await _inner.DisposeAsync().ConfigureAwait(false);
		_onDispose?.Invoke();
	}

	protected override void Dispose(bool disposing) {
		if (disposing) {
			_inner.Dispose();
			_onDispose?.Invoke();
		}

		base.Dispose(disposing);
	}
}
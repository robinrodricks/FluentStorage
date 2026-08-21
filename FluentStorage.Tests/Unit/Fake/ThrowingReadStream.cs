namespace FluentStorage.Tests.Unit.Fake;

public sealed class ThrowingReadStream : Stream {
	private readonly Exception _exception;

	public ThrowingReadStream(Exception exception) {
		_exception = exception;
	}

	public override bool CanRead => true;
	public override bool CanSeek => false;
	public override bool CanWrite => false;

	public override long Length => throw new NotSupportedException();

	public override long Position {
		get => throw new NotSupportedException();
		set => throw new NotSupportedException();
	}

	public override int Read(byte[] buffer, int offset, int count) {
		throw _exception;
	}

	public override Task<int> ReadAsync(
		byte[] buffer,
		int offset,
		int count,
		CancellationToken cancellationToken) {

		return Task.FromException<int>(_exception);
	}

#if NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
	public override ValueTask<int> ReadAsync(
		Memory<byte> buffer,
		CancellationToken cancellationToken = default) {

		return ValueTask.FromException<int>(_exception);
	}
#endif

	public override long Seek(long offset, SeekOrigin origin)
		=> throw new NotSupportedException();

	public override void SetLength(long value)
		=> throw new NotSupportedException();

	public override void Write(byte[] buffer, int offset, int count)
		=> throw new NotSupportedException();

	public override void Flush() {
	}
}
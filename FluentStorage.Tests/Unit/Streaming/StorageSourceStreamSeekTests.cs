namespace FluentStorage.Tests.Unit.Streaming;

/// <summary>
/// Unit tests for seeking a <see cref="StorageSourceStream"/>. It reports itself as seekable, and an SDK that
/// retries a failed upload rewinds the stream to send the same bytes again, so a seekable origin has to
/// support that rewind.
/// </summary>
public sealed class StorageSourceStreamSeekTests {

	/// <summary>
	/// Verifies that over a seekable origin, the stream can be rewound after it has been read, and the
	/// second read returns the same bytes - what a retried upload needs.
	/// </summary>
	[Fact]
	public void Seek_ToBeginAfterRead_WithSeekableOrigin_ReadsTheSameBytesAgain() {
		var stream = new StorageSourceStream(new MemoryStream(Encoding.UTF8.GetBytes("Hello")));

		Assert.Equal("Hello", ReadAll(stream));

		Assert.Equal(0, stream.Seek(0, SeekOrigin.Begin));
		Assert.Equal("Hello", ReadAll(stream));
	}

	/// <summary>
	/// Verifies the same rewind through <see cref="Stream.CopyToAsync(Stream)"/>, which reads through
	/// <c>ReadAsync(Memory)</c> rather than the array overload.
	/// </summary>
	[Fact]
	public async Task Seek_ToBeginAfterCopy_WithSeekableOrigin_CopiesTheSameBytesAgain() {
		var stream = new StorageSourceStream(new MemoryStream(Encoding.UTF8.GetBytes("Hello")));

		await stream.CopyToAsync(Stream.Null);
		stream.Seek(0, SeekOrigin.Begin);

		var copy = new MemoryStream();
		await stream.CopyToAsync(copy);
		Assert.Equal("Hello", Encoding.UTF8.GetString(copy.ToArray()));
	}

	/// <summary>
	/// Verifies that the rewind before the first read, which issue #132 added for SDKs that seek to the
	/// beginning before reading, still rewinds a seekable origin that was not at its start.
	/// </summary>
	[Fact]
	public void Seek_ToBeginBeforeRead_RewindsTheOrigin() {
		var origin = new MemoryStream(Encoding.UTF8.GetBytes("Hello")) { Position = 3 };
		var stream = new StorageSourceStream(origin);

		stream.Seek(0, SeekOrigin.Begin);

		Assert.Equal("Hello", ReadAll(stream));
	}

	/// <summary>
	/// Verifies that over an origin that cannot seek, a rewind after reading is still refused - there is no
	/// way to send the same bytes again.
	/// </summary>
	[Fact]
	public void Seek_ToBeginAfterRead_WithNonSeekableOrigin_Throws() {
		var stream = new StorageSourceStream(new ForwardOnlyStream(Encoding.UTF8.GetBytes("Hello")));

		ReadAll(stream);

		Assert.Throws<NotSupportedException>(() => stream.Seek(0, SeekOrigin.Begin));
	}

	private static string ReadAll(Stream stream) {
		var buffer = new byte[64];
		var result = new MemoryStream();
		int read;
		while ((read = stream.Read(buffer, 0, buffer.Length)) > 0) {
			result.Write(buffer, 0, read);
		}

		return Encoding.UTF8.GetString(result.ToArray());
	}

	/// <summary>
	/// A readable stream that cannot seek, like a network response.
	/// </summary>
	private sealed class ForwardOnlyStream(byte[] content) : Stream {
		private readonly MemoryStream _inner = new(content);

		public override bool CanRead => true;
		public override bool CanSeek => false;
		public override bool CanWrite => false;
		public override long Length => throw new NotSupportedException();
		public override long Position {
			get => throw new NotSupportedException();
			set => throw new NotSupportedException();
		}

		public override int Read(byte[] buffer, int offset, int count) => _inner.Read(buffer, offset, count);
		public override void Flush() { }
		public override long Seek(long offset, SeekOrigin origin) => throw new NotSupportedException();
		public override void SetLength(long value) => throw new NotSupportedException();
		public override void Write(byte[] buffer, int offset, int count) => throw new NotSupportedException();
	}
}

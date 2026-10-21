namespace FluentStorage.Tests.Unit.Streaming;

/// <summary>
/// Unit tests for setting <see cref="FixedStream.Position"/>, which the OpenWrite implementations rely on to
/// rewind the written content before uploading it.
/// </summary>
public sealed class FixedStreamPositionTests {

	/// <summary>
	/// Verifies that rewinding a FixedStream over a seekable stream rewinds that stream, so the content
	/// written through it can be read back from the start.
	/// </summary>
	[Fact]
	public void Position_SetToZero_RewindsSeekableParent() {
		var parent = new MemoryStream();
		var stream = new FixedStream(parent, null, (Action<FixedStream>)null);

		stream.Write(Encoding.UTF8.GetBytes("Hello"), 0, 5);
		stream.Position = 0;

		Assert.Equal(0, parent.Position);
		Assert.Equal("Hello", new StreamReader(stream).ReadToEnd());
	}

	/// <summary>
	/// Verifies the OpenWrite pattern end to end on the memory store: content written through the stream
	/// it returns is what is stored once that stream is disposed.
	/// </summary>
	[Fact]
	public async Task MemoryStore_OpenWrite_StoresWhatWasWritten() {
		IStore store = StorageFactory.InMemory();

		using (Stream stream = await store.OpenWrite("file.txt", overwrite: true)) {
			using var writer = new StreamWriter(stream);
			await writer.WriteAsync("Hello");
		}

		Assert.Equal("Hello", await store.GetText("file.txt"));
	}

	/// <summary>
	/// Verifies that a FixedStream over a stream that cannot seek still accepts a position, as it did before,
	/// rather than throwing.
	/// </summary>
	[Fact]
	public void Position_Set_WithNonSeekableParent_DoesNotThrow() {
		var parent = new ThrowingReadStream(new IOException("not read in this test"));
		var stream = new FixedStream(parent, 10, (Action<FixedStream>)null);

		stream.Position = 5;

		Assert.Equal(5, stream.Position);
	}
}

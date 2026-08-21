namespace FluentStorage.Tests.Unit.Fake;

/// <summary>
/// Simple in-memory IStore implementation for unit tests.
/// </summary>
internal sealed class FakeStore : StoreBase {

	private readonly byte[] _data;

	/// <summary>
	/// Records every OpenRange call.
	/// </summary>
	public List<OpenRangeCall> OpenRangeCalls { get; } = [];

	/// <summary>
	/// Optional exception thrown by OpenRange.
	/// </summary>
	public Exception OpenRangeException { get; set; }

	public Exception RemoteReadException { get; set; }

	public FakeStore(byte[] data = null) {
		_data = data ?? [];
	}

	public async override Task<Stream> OpenRange(
		string fullPath,
		long offset,
		long length,
		CancellationToken cancellationToken = default) {

		cancellationToken.ThrowIfCancellationRequested();

		// fake errors #1
		if (OpenRangeException != null)
			throw OpenRangeException;

		OpenRangeCalls.Add(new OpenRangeCall(fullPath, offset, length));

		// fake errors #2
		if (RemoteReadException != null)
			return new ThrowingReadStream(RemoteReadException);

		if (offset >= _data.LongLength)
			return new MemoryStream(Array.Empty<byte>(), writable: false);

		int available = (int)Math.Min(length, _data.LongLength - offset);

		var buffer = new byte[available];
		Buffer.BlockCopy(_data, (int)offset, buffer, 0, available);

		return new MemoryStream(buffer, writable: false);
	}

	public sealed record OpenRangeCall(string Path,long Offset,long Length);

}
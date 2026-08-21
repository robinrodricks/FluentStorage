using FluentStorage.Utils.Hashing;

namespace FluentStorage.Tests.Unit.Utils;

public class StreamExtensionsTest {
	[Fact]
	public void MD5_hashes_a_stream_to_the_expected_value() {
		using var stream = new MemoryStream(
			Encoding.UTF8.GetBytes("The quick brown fox jumps over the lazy dog")
		);

		var hash = HashUtility.HashStream(stream, StorageHash.MD5);

		Assert.Equal("9e107d9d372bb6826bd81d3542a419d6", hash);
	}

	[Fact]
	public void MD5_can_be_called_concurrently() {
		// Regression test for https://github.com/robinrodricks/FluentStorage/issues/72. The Stream
		// MD5 helper used a shared static HashAlgorithm instance, which is not thread-safe. Each call
		// already gets a fresh stream, so the shared hash instance was the only state being raced.
		// Two threads hash at the same time, each looping enough that they reliably overlap inside
		// the hash.
		byte[] content = Encoding.UTF8.GetBytes("The quick brown fox jumps over the lazy dog");
		string expected;
		using (var seed = new MemoryStream(content)) {
			expected = HashUtility.HashStream(seed, StorageHash.MD5);
		}

		void Hash() {
			for (var j = 0; j < 2000; j++) {
				using var stream = new MemoryStream(content);
				Assert.Equal(expected, HashUtility.HashStream(stream, StorageHash.MD5));
			}
		}

		Task.WaitAll(Task.Run(Hash), Task.Run(Hash));
	}
}
namespace FluentStorage.Tests.Unit.Utils;

public class ByteArrayExtensionsTest {
	[Theory]
	[InlineData(null, null)]
	[InlineData(new byte[] { }, "")]
	[InlineData(new byte[] { 0, 1, 2, 3, 4, 5 }, "000102030405")]
	public void ToHexString_Variable_Variable(byte[] input, string expected) {
		string actual = input.ToHexString();

		Assert.Equal(expected, actual);
	}

	[Fact]
	public void MD5_hashes_to_the_expected_value() {
		byte[] data = Encoding.UTF8.GetBytes("The quick brown fox jumps over the lazy dog");

		Assert.Equal("9e107d9d372bb6826bd81d3542a419d6", data.MD5().ToHexString());
	}

	[Fact]
	public void SHA256_hashes_to_the_expected_value() {
		byte[] data = Encoding.UTF8.GetBytes("The quick brown fox jumps over the lazy dog");

		Assert.Equal(
			"d7a8fbb307d7809469ca9abcb0082e4f8d5651e46d3cdb762d02d0bf37c9e592",
			data.SHA256().ToHexString()
		);
	}

	[Fact]
	public void Hash_helpers_can_be_called_concurrently() {
		// Regression test for https://github.com/robinrodricks/FluentStorage/issues/72. The MD5 and
		// SHA256 helpers used to share a single static HashAlgorithm instance, which is not
		// thread-safe and throws "Concurrent operations from multiple threads on this type are not
		// supported" when called concurrently, for example during concurrent InMemoryBlobStorage
		// writes. Two threads hash the same data at the same time, each looping enough that they
		// reliably overlap inside the hash. The race only trips while both threads are hashing at the
		// same moment, so a short loop can miss.
		byte[] data = Encoding.UTF8.GetBytes("The quick brown fox jumps over the lazy dog");
		byte[] expectedMd5 = data.MD5()!;
		byte[] expectedSha256 = data.SHA256()!;

		void Hash() {
			for (var j = 0; j < 2000; j++) {
				Assert.Equal(expectedMd5, data.MD5());
				Assert.Equal(expectedSha256, data.SHA256());
			}
		}

		Task.WaitAll(Task.Run(Hash), Task.Run(Hash));
	}
}
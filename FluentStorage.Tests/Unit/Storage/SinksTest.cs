namespace FluentStorage.Tests.Unit.Storage;

public abstract class SinksTest {
	private readonly IStore _storage;

	protected SinksTest(IStore storage) {
		_storage = storage;
	}

	[Theory]
	[InlineData(null)]
	[InlineData("sample")]
	[InlineData("123")]
	[InlineData("123123123123123123123123123123123123123123123123123123123123123123123123123123123123123123123123123123123123123123123123")]
	public async Task Roundtrip(string sample) {
		var randomFilename = Path.GetRandomFileName();
		await _storage.SetText(randomFilename, sample);
		Assert.Equal(sample, await _storage.GetText(randomFilename));
	}

	[Theory]
	[InlineData("sample")]
	[InlineData("123")]
	[InlineData("123123123123123123123123123123123123123123123123123123123123123123123123123123123123123123123123123123123123123123123123")]
	public async Task RoundtripStream(string sample) {
		var randomFilename = Path.GetRandomFileName();

		using (Stream source = sample.ToMemoryStream()) {
			source.Position = 0;
			await _storage.SetObject(randomFilename, source);

			using (Stream stream = new MemoryStream()) {
				await _storage.GetObject(randomFilename, stream);
				stream.Position = 0;
				var actual = FromStream(stream);
				Assert.Equal(sample, actual);
			}
		}
	}

	[Theory]
	[FileData("TestFiles", "e4124b764c1bee11a299005056370622.jpg")]
	[FileData("TestFiles", "d0d829444b1bee11a299005056370622.jpg")]
	public async Task RoundtripStreamForFile(string folder, string filename, Stream source) {
		var randomFilename = $"{Path.GetFileNameWithoutExtension(Path.GetRandomFileName())}_{filename}";
		using (source) {
			source.Position = 0;
			var expected = source.ToByteArray();
			source.Position = 0;
			await _storage.SetObject(randomFilename, source);

			using (Stream stream = new MemoryStream()) {
				await _storage.GetObject(randomFilename, stream);
				stream.Position = 0;
				var actual = stream.ToByteArray();
				Assert.Equal(expected, actual);
			}
		}
	}

	static string FromStream(Stream stream) {
		using var reader = new StreamReader(stream);
		stream.Position = 0;
		return reader.ReadToEnd();
	}
}
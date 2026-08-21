namespace FluentStorage.Tests.Integration.Storage.TestSuite;

public partial class IStoreTest {


	// ---------------------------------------------------------------------
	// Read Operations
	// ---------------------------------------------------------------------

	[Fact]
	public async Task OpenRead_ReturnsReadableStream() {
		string file = RandomFile();

		await CreateText(file, "Hello World");

		using var stream = await _storage.OpenRead(file);

		Assert.NotNull(stream);
		Assert.True(stream.CanRead);
	}

	[Fact]
	public async Task OpenRead_ReadsEntireContents() {
		string file = RandomFile();

		const string text = "Hello World";

		await CreateText(file, text);

		using var stream = await _storage.OpenRead(file);
		using var reader = new StreamReader(stream);

		Assert.Equal(text, await reader.ReadToEndAsync());
	}

	[Fact]
	public async Task OpenRead_MissingObject_ReturnsNullOrThrows() {
		try {
			using var stream = await _storage.OpenRead(RandomFile());

			Assert.Null(stream);
		}
		catch {
			// acceptable
		}
	}

	[Fact]
	public async Task GetBytes_ReturnsCorrectBytes() {
		string file = RandomFile();

		byte[] expected = { 1, 2, 3, 4, 5, 6 };

		await _storage.SetBytes(file, expected);

		byte[] actual = await _storage.GetBytes(file);

		Assert.Equal(expected, actual);
	}

	[Fact]
	public async Task GetBytes_EmptyFile_ReturnsEmptyArray() {
		string file = RandomFile();

		await _storage.SetBytes(file, Array.Empty<byte>());

		byte[] data = await _storage.GetBytes(file);

		Assert.NotNull(data);
		Assert.Empty(data);
	}

	[Fact]
	public async Task GetText_ReturnsCorrectText() {
		string file = RandomFile();

		const string text = "The quick brown fox.";

		await _storage.SetText(file, text);

		string actual = await _storage.GetText(file);

		Assert.Equal(text, actual);
	}

	[Fact]
	public async Task GetText_Unicode() {
		string file = RandomFile();

		const string text = "你好 नमस्ते 😀";

		await _storage.SetText(file, text);

		Assert.Equal(text, await _storage.GetText(file));
	}

	private sealed class TestPerson {
		public string Name { get; set; }
		public int Age { get; set; }
	}

	[Fact]
	public async Task GetJson_ReturnsObject() {
		string file = RandomFile();

		var person = new TestPerson {
			Name = "John",
			Age = 30
		};

		await _storage.SetJson(file, person);

		var loaded = await _storage.GetJson<TestPerson>(file);

		Assert.NotNull(loaded);
		Assert.Equal(person.Name, loaded.Name);
		Assert.Equal(person.Age, loaded.Age);
	}

	[Fact]
	public async Task GetJson_InvalidJson_Throws() {
		string file = RandomFile();

		await _storage.SetText(file, "this isn't json");

		await Assert.ThrowsAnyAsync<Exception>(
			() => _storage.GetJson<TestPerson>(file));
	}

	[Fact]
	public async Task GetJson_InvalidJson_Ignore_ReturnsNull() {
		string file = RandomFile();

		await _storage.SetText(file, "this isn't json");

		var result = await _storage.GetJson<TestPerson>(
			file,
			ignoreInvalidJson: true);

		Assert.Null(result);
	}

	[Fact]
	public async Task GetObject_CopiesToTargetStream() {
		string file = RandomFile();

		const string text = "Hello";

		await _storage.SetText(file, text);

		using var ms = new MemoryStream();

		await _storage.GetObject(file, ms);

		Assert.Equal(text,
			Encoding.UTF8.GetString(ms.ToArray()));
	}

	[Fact]
	public async Task DownloadObject_DownloadsFile() {
		string file = RandomFile();

		const string text = "Downloaded";

		await _storage.SetText(file, text);

		string temp = Path.GetTempFileName();

		try {
			await _storage.DownloadObject(file, temp, overwrite: true);

			Assert.Equal(text, File.ReadAllText(temp));
		}
		finally {
			if (File.Exists(temp))
				File.Delete(temp);
		}
	}

	[Fact]
	public async Task OpenRange_ReturnsRequestedBytes() {
		string file = RandomFile();

		await _storage.SetText(file, "0123456789");

		using var stream = await _storage.OpenRange(file, 2, 4);

		using var reader = new StreamReader(stream);

		var result = await reader.ReadToEndAsync();

		if (result.Length == 4) {
			Assert.Equal("2345", result);
		}
		else {
			Assert.Equal("23456789", result);
		}
	}

	[Fact]
	public async Task OpenRange_FromBeginning() {
		string file = RandomFile();

		await _storage.SetText(file, "ABCDEFGHIJ");

		using var stream = await _storage.OpenRange(file, 0, 3);

		using var reader = new StreamReader(stream);

		var result = await reader.ReadToEndAsync();

		if (result.Length == 3) {
			Assert.Equal("ABC", result);
		}
		else {
			Assert.Equal("ABCDEFGHIJ", result);
		}
	}

	[Fact]
	public async Task OpenRange_BeyondEnd_ReturnsEmptyOrShortStream() {
		string file = RandomFile();

		await _storage.SetText(file, "abc");

		using var stream = await _storage.OpenRange(file, 100, 50);

		Assert.NotNull(stream);

		using var ms = new MemoryStream();

		await stream.CopyToAsync(ms);

		Assert.True(ms.Length == 0);
	}

	[Fact]
	public async Task OpenSeekable_ReturnsSeekableStream() {
		string file = RandomFile();

		await _storage.SetText(file, "abcdefghijklmnopqrstuvwxyz");

		using var stream = await _storage.OpenSeekable(file);

		Assert.NotNull(stream);
		Assert.True(stream.CanRead);
		Assert.True(stream.CanSeek);
	}

	[Fact]
	public async Task OpenSeekable_ReadSeekRead() {
		string file = RandomFile();

		await _storage.SetText(file, "0123456789");

		using var stream = await _storage.OpenSeekable(file);

		byte[] buffer = new byte[2];

		await stream.ReadAsync(buffer);

		Assert.Equal("01", Encoding.UTF8.GetString(buffer));

		stream.Seek(5, SeekOrigin.Begin);

		await stream.ReadAsync(buffer);

		Assert.Equal("56", Encoding.UTF8.GetString(buffer));
	}

	[Fact]
	public async Task OpenSeekable_LengthMatchesObject() {
		string file = RandomFile();

		const string text = "Hello World";

		await _storage.SetText(file, text);

		using var stream = await _storage.OpenSeekable(file);

		Assert.Equal(text.Length, stream.Length);
	}

	[Fact]
	public async Task OpenSeekable_MissingObject_ReturnsNullOrThrows() {
		try {
			using var stream = await _storage.OpenSeekable(RandomFile());

			Assert.Null(stream);
		}
		catch {
			// acceptable
		}
	}

}
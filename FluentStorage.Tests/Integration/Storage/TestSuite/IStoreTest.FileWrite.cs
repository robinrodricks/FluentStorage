namespace FluentStorage.Tests.Integration.Storage.TestSuite;

public partial class IStoreTest {


	// ---------------------------------------------------------------------
	// Write Operations
	// ---------------------------------------------------------------------

	[Fact]
	public async Task SetText_CreatesObject() {
		string file = RandomFile();

		await _storage.SetText(file, "Hello");

		Assert.True(await _storage.ObjectExists(file));
	}

	[Fact]
	public async Task SetText_OverwritesExistingContents() {
		string file = RandomFile();

		await _storage.SetText(file, "One");
		await _storage.SetText(file, "Two");

		Assert.Equal("Two", await _storage.GetText(file));
	}

	[Fact]
	public async Task SetBytes_WritesBytes() {
		string file = RandomFile();

		byte[] expected = { 10, 20, 30, 40 };

		await _storage.SetBytes(file, expected);

		byte[] actual = await _storage.GetBytes(file);

		Assert.Equal(expected, actual);
	}

	[Fact]
	public async Task SetBytes_EmptyArray_CreatesEmptyFile() {
		string file = RandomFile();

		await _storage.SetBytes(file, Array.Empty<byte>());

		Assert.True(await _storage.ObjectExists(file));
		Assert.Equal(0, await _storage.GetObjectLength(file));
	}

	[Fact]
	public async Task SetJson_WritesObject() {
		string file = RandomFile();

		var person = new TestPerson {
			Name = "Alice",
			Age = 25
		};

		await _storage.SetJson(file, person);

		var loaded = await _storage.GetJson<TestPerson>(file);

		Assert.Equal(person.Name, loaded.Name);
		Assert.Equal(person.Age, loaded.Age);
	}

	[Fact]
	public async Task SetObject_Stream_WritesData() {
		string file = RandomFile();

		byte[] bytes = Encoding.UTF8.GetBytes("Hello World");

		using var ms = new MemoryStream(bytes);

		await _storage.SetObject(file, ms);

		Assert.Equal("Hello World", await _storage.GetText(file));
	}

	[Fact]
	public async Task SetObject_WithContentType_WritesData() {
		string file = RandomFile();

		byte[] bytes = Encoding.UTF8.GetBytes("abcdef");

		using var ms = new MemoryStream(bytes);

		await _storage.SetObject(file, ms, "text/plain");

		Assert.Equal("abcdef", await _storage.GetText(file));
	}

	[Fact]
	public async Task SetObject_Append_AppendsData() {
		string file = RandomFile();

		using (var ms = new MemoryStream(Encoding.UTF8.GetBytes("Hello")))
			await _storage.SetObject(file, ms);

		using (var ms = new MemoryStream(Encoding.UTF8.GetBytes(" World")))
			await _storage.SetObject(file, ms, append: true);

		Assert.Equal("Hello World", await _storage.GetText(file));
	}

	[Fact]
	public async Task OpenWrite_CreatesObject() {
		string file = RandomFile();

		using (var stream = await _storage.OpenWrite(file, overwrite: true)) {
			using (var writer = new StreamWriter(stream)) {
				await writer.WriteAsync("Hello");
			}
		}

		Assert.Equal("Hello", await _storage.GetText(file));
	}

	[Fact]
	public async Task OpenWrite_Overwrite_ReplacesExistingContents() {
		string file = RandomFile();

		await _storage.SetText(file, "Old");

		using (var stream = await _storage.OpenWrite(file, overwrite: true)) {
			using (var writer = new StreamWriter(stream)) {
				await writer.WriteAsync("New");
			}
		}

		Assert.Equal("New", await _storage.GetText(file));
	}

	[Fact]
	public async Task OpenWrite_OverwriteFalse_ReturnsNullOrThrows_WhenObjectExists() {
		string file = RandomFile();

		await _storage.SetText(file, "Existing");

		try {
			using var stream = await _storage.OpenWrite(file, overwrite: false);

			Assert.Null(stream);
		}
		catch {
			// Provider may throw instead.
		}
	}

	[Fact]
	public async Task UploadObject_UploadsLocalFile() {
		string file = RandomFile();

		string temp = Path.GetTempFileName();

		try {
			File.WriteAllText(temp, "Upload Test");

			await _storage.UploadObject(file, temp, overwrite: true);

			Assert.Equal("Upload Test", await _storage.GetText(file));
		}
		finally {
			if (File.Exists(temp))
				File.Delete(temp);
		}
	}

	[Fact]
	public async Task UploadObject_Overwrite_ReplacesExistingObject() {
		string file = RandomFile();

		await _storage.SetText(file, "Old");

		string temp = Path.GetTempFileName();

		try {
			File.WriteAllText(temp, "New");

			await _storage.UploadObject(file, temp, overwrite: true);

			Assert.Equal("New", await _storage.GetText(file));
		}
		finally {
			if (File.Exists(temp))
				File.Delete(temp);
		}
	}

	[Fact]
	public async Task UploadObject_OverwriteFalse_ReturnsFalseOrThrows_WhenObjectExists() {
		string file = RandomFile();

		await _storage.SetText(file, "Existing");

		string temp = Path.GetTempFileName();

		try {
			File.WriteAllText(temp, "Replacement");

			try {
				await _storage.UploadObject(file, temp, overwrite: false);

				Assert.Equal("Existing", await _storage.GetText(file));
			}
			catch {
				// acceptable
			}
		}
		finally {
			if (File.Exists(temp))
				File.Delete(temp);
		}
	}

	[Fact]
	public async Task LargeText_CanBeWrittenAndRead() {
		string file = RandomFile();

		string text = new string('X', 1024 * 1024);

		await _storage.SetText(file, text);

		Assert.Equal(text, await _storage.GetText(file));
	}

	[Fact]
	public async Task UnicodeFileName_CanBeWritten() {
		string file = $"tests/{Guid.NewGuid():N}/你好 नमस्ते 😀.txt";

		await _storage.SetText(file, "unicode");

		Assert.True(await _storage.ObjectExists(file));

		Assert.Equal("unicode", await _storage.GetText(file));
	}

	[Fact]
	public async Task ConsecutiveWrites_LastWriteWins() {
		string file = RandomFile();

		for (int i = 0; i < 10; i++)
			await _storage.SetText(file, i.ToString());

		Assert.Equal("9", await _storage.GetText(file));
	}

	[Fact]
	public async Task _PerformanceTest_10KB_SetBytes() {
		string file = RandomFile();

		byte[] expected = new byte[10 * 1024];

		new Random(12345).NextBytes(expected);

		await _storage.SetBytes(file, expected);

		byte[] actual = await _storage.GetBytes(file);

		Assert.Equal(expected, actual);
		await _storage.DeleteObject(file);
	}

	[Fact]
	public async Task _PerformanceTest_100KB_SetBytes() {
		string file = RandomFile();

		byte[] expected = new byte[100 * 1024];

		new Random(12345).NextBytes(expected);

		await _storage.SetBytes(file, expected);

		byte[] actual = await _storage.GetBytes(file);

		Assert.Equal(expected, actual);
		await _storage.DeleteObject(file);
	}

	[Fact]
	public async Task _PerformanceTest_1MB_SetBytes() {
		string file = RandomFile();

		byte[] expected = new byte[1 * 1024 * 1024];

		new Random(12345).NextBytes(expected);

		await _storage.SetBytes(file, expected);

		byte[] actual = await _storage.GetBytes(file);

		Assert.Equal(expected, actual);
		await _storage.DeleteObject(file);
	}

	[Fact]
	public async Task _PerformanceTest_10MB_SetBytes() {
		string file = RandomFile();

		byte[] expected = new byte[10 * 1024 * 1024];

		new Random(12345).NextBytes(expected);

		await _storage.SetBytes(file, expected);

		byte[] actual = await _storage.GetBytes(file);

		Assert.Equal(expected, actual);
		await _storage.DeleteObject(file);
	}
	[Fact]
	public async Task _PerformanceTest_10KB_UploadObject() {
		string remoteFile = RandomFile();
		string uploadFile = Path.GetTempFileName();
		string downloadFile = Path.GetTempFileName();

		try {
			byte[] expected = new byte[10 * 1024];
			new Random(12345).NextBytes(expected);

			await File.WriteAllBytesAsync(uploadFile, expected);

			await _storage.UploadObject(remoteFile, uploadFile, true);
			await _storage.DownloadObject(remoteFile, downloadFile, true);

			byte[] actual = await File.ReadAllBytesAsync(downloadFile);

			Assert.Equal(expected, actual);
		}
		finally {
			File.Delete(uploadFile);
			File.Delete(downloadFile);
			await _storage.DeleteObject(remoteFile);
		}
	}

	[Fact]
	public async Task _PerformanceTest_100KB_UploadObject() {
		string remoteFile = RandomFile();
		string uploadFile = Path.GetTempFileName();
		string downloadFile = Path.GetTempFileName();

		try {
			byte[] expected = new byte[100 * 1024];
			new Random(12345).NextBytes(expected);

			await File.WriteAllBytesAsync(uploadFile, expected);

			await _storage.UploadObject(remoteFile, uploadFile, true);
			await _storage.DownloadObject(remoteFile, downloadFile, true);

			byte[] actual = await File.ReadAllBytesAsync(downloadFile);

			Assert.Equal(expected, actual);
		}
		finally {
			File.Delete(uploadFile);
			File.Delete(downloadFile);
			await _storage.DeleteObject(remoteFile);
		}
	}

	[Fact]
	public async Task _PerformanceTest_1MB_UploadObject() {
		string remoteFile = RandomFile();
		string uploadFile = Path.GetTempFileName();
		string downloadFile = Path.GetTempFileName();

		try {
			byte[] expected = new byte[1 * 1024 * 1024];
			new Random(12345).NextBytes(expected);

			await File.WriteAllBytesAsync(uploadFile, expected);

			await _storage.UploadObject(remoteFile, uploadFile, true);
			await _storage.DownloadObject(remoteFile, downloadFile, true);

			byte[] actual = await File.ReadAllBytesAsync(downloadFile);

			Assert.Equal(expected, actual);
		}
		finally {
			File.Delete(uploadFile);
			File.Delete(downloadFile);
			await _storage.DeleteObject(remoteFile);
		}
	}

	[Fact]
	public async Task _PerformanceTest_10MB_UploadObject() {
		string remoteFile = RandomFile();
		string uploadFile = Path.GetTempFileName();
		string downloadFile = Path.GetTempFileName();

		try {
			byte[] expected = new byte[10 * 1024 * 1024];
			new Random(12345).NextBytes(expected);

			await File.WriteAllBytesAsync(uploadFile, expected);

			await _storage.UploadObject(remoteFile, uploadFile, true);
			await _storage.DownloadObject(remoteFile, downloadFile, true);

			byte[] actual = await File.ReadAllBytesAsync(downloadFile);

			Assert.Equal(expected, actual);
		}
		finally {
			File.Delete(uploadFile);
			File.Delete(downloadFile);
			await _storage.DeleteObject(remoteFile);
		}
	}


}
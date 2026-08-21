namespace FluentStorage.Tests.Unit.Storage;

public class BlobStorageExtensionsTest {
	private readonly IStore _storage = StorageFactory.InMemory();

	[Fact]
	public async Task Write_read_object() {
		var input = new MyClass { Name = "test" };

		await _storage.SetJson("1.json", input);

		MyClass output = await _storage.GetJson<MyClass>("1.json");

		Assert.Equal("test", output.Name);
	}

	[Fact]
	public async Task Read_non_existing_object_returns_Default() {
		MyClass output = await _storage.GetJson<MyClass>("1.json");

		Assert.Null(output);
	}

	[Fact]
	public async Task Read_damaged_json_with_ignore_returns_Default() {
		await _storage.SetText("1.json", "not a json");

		MyClass output = await _storage.GetJson<MyClass>("1.json", true);

		Assert.Null(output);
	}

	[Fact]
	public async Task Read_damaged_json_without_ignore_returns_Default() {
		await _storage.SetText("1.json", "not a json");

		await Assert.ThrowsAsync<JsonException>(() => _storage.GetJson<MyClass>("1.json", false));
	}


	private class MyClass {
		public string Name { get; set; }
	}
}
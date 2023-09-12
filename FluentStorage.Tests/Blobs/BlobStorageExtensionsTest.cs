using System.Text.Json;
using System.Threading.Tasks;
using FluentStorage.Blobs;
using Xunit;

namespace FluentStorage.Tests.Blobs {
	public class BlobStorageExtensionsTest {
		private readonly IBlobStorage _storage = StorageFactory.Blobs.InMemory();

		[Fact]
		public async Task Write_read_object() {
			var input = new MyClass { Name = "test" };

			await _storage.WriteJsonAsync("1.json", input);

			MyClass output = await _storage.ReadJsonAsync<MyClass>("1.json");

			Assert.Equal("test", output.Name);
		}

		[Fact]
		public async Task Read_non_existing_object_returns_Default() {
			MyClass output = await _storage.ReadJsonAsync<MyClass>("1.json");

			Assert.Null(output);
		}

		[Fact]
		public async Task Read_damaged_json_with_ignore_returns_Default() {
			await _storage.WriteTextAsync("1.json", "not a json");

			MyClass output = await _storage.ReadJsonAsync<MyClass>("1.json", true);

			Assert.Null(output);
		}

		[Fact]
		public async Task Read_damaged_json_without_ignore_returns_Default() {
			await _storage.WriteTextAsync("1.json", "not a json");

			await Assert.ThrowsAsync<JsonException>(() => _storage.ReadJsonAsync<MyClass>("1.json", false));
		}


		private class MyClass {
			public string Name { get; set; }
		}
	}
}

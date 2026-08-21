namespace FluentStorage.Tests.Integration.Storage.TestSuite;

/// <summary>
/// Massive test case suite to test object and directory manipulation for the given provider.
/// Should work on disk providers and cloud storage providers.
/// </summary>
public partial class IStoreTest : IAsyncLifetime {
	private readonly IStore _storage;
	private readonly string _blobPrefix;
	private readonly StoreFixture _fixture;

	public IStoreTest(StoreFixture fixture) {
		_storage = fixture.Storage;
		_blobPrefix = fixture.BlobPrefix;
		_fixture = fixture;
	}

	public Task InitializeAsync() {
		return _fixture.InitAsync();
	}

	public Task DisposeAsync() {
		return _fixture.DisposeAsync();
	}

	// ---------------------------------------------------------------------
	// Reused Utils
	// ---------------------------------------------------------------------

	private async Task CreateText(string path, string text = null) {
		await _storage.SetText(path, text ?? Guid.NewGuid().ToString());
	}

	private static string RandomName()=> Guid.NewGuid().ToString("N");

	private static string RandomFolder()=> $"tests/{Guid.NewGuid():N}";

	private static string RandomFile()=> $"tests/{Guid.NewGuid():N}.txt";

	private async Task<string> GetRandomStreamIdAsync(string prefix = null) {
		string id = RandomBlobPath();
		if (prefix != null)
			id = prefix + "/" + id;

		using (Stream s = "kjhlkhlkhlkhlkh".ToMemoryStream()) {
			await _storage.SetObject(id, s);
		}

		return id;
	}
	private static string RandomRemoteFolder() => $"tests/{Guid.NewGuid():N}";

	private static string RandomLocalFolder() {
		string path = Path.Combine(Path.GetTempPath(),"StorageTests",Guid.NewGuid().ToString("N"));

		Directory.CreateDirectory(path);

		return path;
	}






}
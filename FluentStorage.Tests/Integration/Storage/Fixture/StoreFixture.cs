namespace FluentStorage.Tests.Integration.Storage.Fixture;

public abstract class StoreFixture : IDisposable {

	private bool _initialised;

	protected StoreFixture(string blobPrefix = null) {
		Storage = CreateStorage(TestConfigLoader.Config);
		BlobPrefix = blobPrefix;
	}

	protected abstract IStore CreateStorage(TestConfig settings);

	public IStore Storage { get; private set; }
	public string BlobPrefix { get; }

	public async Task InitAsync() {
		if (_initialised)
			return;

		// drop all blobs in test storage
		// FIX: do not run on SFTP and other sensitive providers
		if (BlobPrefix != null) {
			List<StoreObject> topLevel = (await Storage.ListDirectory(BlobPrefix, false)).ToList();
			try {
				await Storage.DeleteObjects(topLevel.Select(f => f.FullPath));
			}
			catch {}
		}

		_initialised = true;
	}

	public Task DisposeAsync() {
		return Task.CompletedTask;
	}

	public void Dispose() {
		Storage.Dispose();
	}
}
namespace FluentStorage.Tests.Integration.Storage;

public class AzureBlobFixture : StoreFixture {
	public AzureBlobFixture() : base("lakeyv12") {
	}

	protected override IStore CreateStorage(TestConfig settings) {

		// make sure required config properties are filled
		if (string.IsNullOrEmpty(TestConfigLoader.Config.AzureStorageName)) {
			throw new Exception("Required setting `AzureStorageName` is blank!");
		}
		if (string.IsNullOrEmpty(TestConfigLoader.Config.AzureStorageKey)) {
			throw new Exception("Required setting `AzureStorageKey` is blank!");
		}

		return AzureBlobStorage.FromSharedKey(settings.AzureStorageName, settings.AzureStorageKey);
	}
}

public class AzureBlobTest : IStoreTest, IClassFixture<AzureBlobFixture> {
	public AzureBlobTest(AzureBlobFixture fixture) : base(fixture) {
	}
}
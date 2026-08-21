namespace FluentStorage.Tests.Integration.Storage;

public class AzureDataLakeFixture : StoreFixture {
	public AzureDataLakeFixture() : base("integration") {

	}

	protected override IStore CreateStorage(TestConfig settings) {

		// make sure required config properties are filled
		if (string.IsNullOrEmpty(TestConfigLoader.Config.AzureDataLakeStorageName)) {
			throw new Exception("Required setting `AzureDataLakeStorageName` is blank!");
		}
		if (string.IsNullOrEmpty(TestConfigLoader.Config.AzureDataLakeStorageKey)) {
			throw new Exception("Required setting `AzureDataLakeStorageKey` is blank!");
		}

		return AzureDataLakeStorage.FromSharedKey(
			settings.AzureDataLakeStorageName,
			settings.AzureDataLakeStorageKey);
	}
}

public class AzureDataLakeTest : IStoreTest, IClassFixture<AzureDataLakeFixture> {
	public AzureDataLakeTest(AzureDataLakeFixture fixture) : base(fixture) {
	}
}
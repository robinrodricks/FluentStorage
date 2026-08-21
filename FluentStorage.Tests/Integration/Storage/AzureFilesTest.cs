namespace FluentStorage.Tests.Integration.Storage;

public class AzureFilesFixture : StoreFixture {
	public AzureFilesFixture() : base("testshare") {

	}

	protected override IStore CreateStorage(TestConfig settings) {

		// make sure required config properties are filled
		if (string.IsNullOrEmpty(TestConfigLoader.Config.AzureStorageName)) {
			throw new Exception("Required setting `AzureStorageName` is blank!");
		}
		if (string.IsNullOrEmpty(TestConfigLoader.Config.AzureStorageKey)) {
			throw new Exception("Required setting `AzureStorageKey` is blank!");
		}

		return AzureFilesStorage.FromCredentials(settings.AzureStorageName, settings.AzureStorageKey);
	}
}

public class AzureFilesTest : IStoreTest, IClassFixture<AzureFilesFixture> {
	public AzureFilesTest(AzureFilesFixture fixture) : base(fixture) {

	}
}
namespace FluentStorage.Tests.Integration.Storage;

public class LocalDiskTestFixture : StoreFixture {
	protected override IStore CreateStorage(TestConfig settings) {

		// make sure required config properties are filled
		if (string.IsNullOrEmpty(TestConfigLoader.Config.LocalDiskPath)) {
			throw new Exception("Required setting `LocalDiskPath` is blank!");
		}

		// calc the path of the test directory on disk
		var testName = "TEST-" + DateTime.UtcNow.ToString("yyyy-MM-dd-HH-mm-ss-fffZ");
		var testDir = Path.Combine(TestConfigLoader.Config.LocalDiskPath, testName);

		return StorageFactory.Disk(testDir);
	}
}

public class LocalDiskTest : IStoreTest, IClassFixture<LocalDiskTestFixture> {
	public LocalDiskTest(LocalDiskTestFixture fixture) : base(fixture) {
	}
}
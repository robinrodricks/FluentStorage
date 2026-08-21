namespace FluentStorage.Tests.Integration.Storage;

public class GcpFixture : StoreFixture {
	protected override IStore CreateStorage(TestConfig settings) {

		// make sure required config properties are filled
		if (string.IsNullOrEmpty(TestConfigLoader.Config.GcpBucket)) {
			throw new Exception("Required setting `GcpBucketName` is blank!");
		}
		if (string.IsNullOrEmpty(TestConfigLoader.Config.GcpJsonKey)) {
			throw new Exception("Required setting `GcpJsonKey` is blank!");
		}

		return GoogleCloudStorage.FromJson(
			settings.GcpBucket,
			settings.GcpJsonKey,
			true);
	}
}

public class GcpTest : IStoreTest, IClassFixture<GcpFixture> {
	public GcpTest(GcpFixture fixture) : base(fixture) {

	}
}
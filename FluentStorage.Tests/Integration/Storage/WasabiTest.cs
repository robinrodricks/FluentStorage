namespace FluentStorage.Tests.Integration.Storage;

public class WasabiFixture : StoreFixture {
	protected override IStore CreateStorage(TestConfig settings) {

		if (string.IsNullOrEmpty(TestConfigLoader.Config.WasabiAccessKey))
			throw new Exception("Required setting `WasabiAccessKeyId` is blank!");

		if (string.IsNullOrEmpty(TestConfigLoader.Config.WasabiSecretKey))
			throw new Exception("Required setting `WasabiSecretAccessKey` is blank!");

		if (string.IsNullOrEmpty(TestConfigLoader.Config.WasabiBucket))
			throw new Exception("Required setting `WasabiBucketName` is blank!");

		if (string.IsNullOrEmpty(TestConfigLoader.Config.WasabiServiceUrl))
			throw new Exception("Required setting `WasabiServiceUrl` is blank!");

		return WasabiStorage.FromCredentials(
			settings.WasabiAccessKey,
			settings.WasabiSecretKey,
			settings.WasabiBucket,
			settings.WasabiServiceUrl);
	}
}

public class WasabiTest : IStoreTest, IClassFixture<WasabiFixture> {
	public WasabiTest(WasabiFixture fixture) : base(fixture) {
	}
}
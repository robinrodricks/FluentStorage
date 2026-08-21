namespace FluentStorage.Tests.Integration.Storage;

public class DigitalOceanFixture : StoreFixture {
	protected override IStore CreateStorage(TestConfig settings) {

		if (string.IsNullOrEmpty(TestConfigLoader.Config.DoAccessKey))
			throw new Exception("Required setting `DigitalOceanAccessKeyId` is blank!");

		if (string.IsNullOrEmpty(TestConfigLoader.Config.DoSecretKey))
			throw new Exception("Required setting `DigitalOceanSecretAccessKey` is blank!");

		if (string.IsNullOrEmpty(TestConfigLoader.Config.DoBucket))
			throw new Exception("Required setting `DigitalOceanBucketName` is blank!");

		if (string.IsNullOrEmpty(TestConfigLoader.Config.DoRegion))
			throw new Exception("Required setting `DigitalOceanRegion` is blank!");

		return DigitalOceanSpacesStorage.FromCredentials(
			settings.DoAccessKey,
			settings.DoSecretKey,
			settings.DoBucket,
			settings.DoRegion);
	}
}

public class DigitalOceanTest : IStoreTest, IClassFixture<DigitalOceanFixture> {
	public DigitalOceanTest(DigitalOceanFixture fixture) : base(fixture) {
	}
}
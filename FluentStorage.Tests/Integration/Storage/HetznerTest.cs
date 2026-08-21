using FluentStorage.AWS.Factory;

namespace FluentStorage.Tests.Integration.Storage;

public class HetznerFixture : StoreFixture {
	protected override IStore CreateStorage(TestConfig settings) {

		if (string.IsNullOrEmpty(TestConfigLoader.Config.HetznerAccessKey))
			throw new Exception("Required setting `HetznerAccessKeyId` is blank!");

		if (string.IsNullOrEmpty(TestConfigLoader.Config.HetznerSecretKey))
			throw new Exception("Required setting `HetznerSecretAccessKey` is blank!");

		if (string.IsNullOrEmpty(TestConfigLoader.Config.HetznerBucket))
			throw new Exception("Required setting `HetznerBucketName` is blank!");

		if (string.IsNullOrEmpty(TestConfigLoader.Config.HetznerRegion))
			throw new Exception("Required setting `HetznerRegion` is blank!");

		return HetznerStorage.FromCredentials(
			settings.HetznerAccessKey,
			settings.HetznerSecretKey,
			settings.HetznerBucket,
			settings.HetznerRegion);
	}
}

public class HetznerTest : IStoreTest, IClassFixture<HetznerFixture> {
	public HetznerTest(HetznerFixture fixture) : base(fixture) {
	}
}
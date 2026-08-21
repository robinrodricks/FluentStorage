namespace FluentStorage.Tests.Integration.Storage;

public class VultrFixture : StoreFixture {
	protected override IStore CreateStorage(TestConfig settings) {

		if (string.IsNullOrEmpty(TestConfigLoader.Config.VultrAccessKey))
			throw new Exception("Required setting `VultrAccessKeyId` is blank!");

		if (string.IsNullOrEmpty(TestConfigLoader.Config.VultrSecretKey))
			throw new Exception("Required setting `VultrSecretAccessKey` is blank!");

		if (string.IsNullOrEmpty(TestConfigLoader.Config.VultrBucket))
			throw new Exception("Required setting `VultrBucketName` is blank!");

		if (string.IsNullOrEmpty(TestConfigLoader.Config.VultrHostName))
			throw new Exception("Required setting `VultrHostName` is blank!");

		return VultrStorage.FromCredentials(
			settings.VultrAccessKey,
			settings.VultrSecretKey,
			settings.VultrBucket,
			settings.VultrHostName);
	}
}

public class VultrTest : IStoreTest, IClassFixture<VultrFixture> {
	public VultrTest(VultrFixture fixture) : base(fixture) {
	}
}
namespace FluentStorage.Tests.Integration.Storage;

public class AlibabaFixture : StoreFixture {
	protected override IStore CreateStorage(TestConfig settings) {

		if (string.IsNullOrEmpty(TestConfigLoader.Config.AlibabaEndpoint))
			throw new Exception("Required setting `AlibabaEndpoint` is blank!");

		if (string.IsNullOrEmpty(TestConfigLoader.Config.AlibabaBucket))
			throw new Exception("Required setting `AlibabaBucketName` is blank!");

		if (string.IsNullOrEmpty(TestConfigLoader.Config.AlibabaAccessKey))
			throw new Exception("Required setting `AlibabaAccessKeyId` is blank!");

		if (string.IsNullOrEmpty(TestConfigLoader.Config.AlibabaSecretKey))
			throw new Exception("Required setting `AlibabaAccessKeySecret` is blank!");

		return AlibabaStorage.FromCredentials(
			settings.AlibabaEndpoint,
			settings.AlibabaBucket,
			settings.AlibabaAccessKey,
			settings.AlibabaSecretKey);
	}
}

public class AlibabaTest : IStoreTest, IClassFixture<AlibabaFixture> {
	public AlibabaTest(AlibabaFixture fixture) : base(fixture) {
	}
}
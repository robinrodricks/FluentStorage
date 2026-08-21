namespace FluentStorage.Tests.Integration.Storage;

public class MinioFixture : StoreFixture {
	protected override IStore CreateStorage(TestConfig settings) {

		if (string.IsNullOrEmpty(TestConfigLoader.Config.MinioEndpoint))
			throw new Exception("Required setting `MinioEndpoint` is blank!");

		if (string.IsNullOrEmpty(TestConfigLoader.Config.MinioAccessKey))
			throw new Exception("Required setting `MinioAccessKey` is blank!");

		if (string.IsNullOrEmpty(TestConfigLoader.Config.MinioSecretKey))
			throw new Exception("Required setting `MinioSecretKey` is blank!");

		if (string.IsNullOrEmpty(TestConfigLoader.Config.MinioBucket))
			throw new Exception("Required setting `MinioBucketName` is blank!");

		return MinioStorage.FromCredentials(
			settings.MinioEndpoint,
			settings.MinioAccessKey,
			settings.MinioSecretKey,
			settings.MinioBucket,
			settings.MinioSsl,
			settings.MinioRegion);
	}
}

public class MinioTest : IStoreTest, IClassFixture<MinioFixture> {
	public MinioTest(MinioFixture fixture) : base(fixture) {
	}
}
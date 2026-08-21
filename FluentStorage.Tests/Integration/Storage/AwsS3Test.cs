namespace FluentStorage.Tests.Integration.Storage;

public class AwsS3Fixture : StoreFixture {
	protected override IStore CreateStorage(TestConfig settings) {

		// make sure required config properties are filled
		if (string.IsNullOrEmpty(TestConfigLoader.Config.AwsAccessKey)) {
			throw new Exception("Required setting `AwsAccessKeyId` is blank!");
		}
		if (string.IsNullOrEmpty(TestConfigLoader.Config.AwsSecretKey)) {
			throw new Exception("Required setting `AwsSecretAccessKey` is blank!");
		}
		if (string.IsNullOrEmpty(TestConfigLoader.Config.AwsBucket)) {
			throw new Exception("Required setting `AwsBucketName` is blank!");
		}
		if (string.IsNullOrEmpty(TestConfigLoader.Config.AwsBucketRegion)) {
			throw new Exception("Required setting `AwsBucketRegion` is blank!");
		}

		return AwsS3Storage.FromCredentials(
			settings.AwsAccessKey,
			settings.AwsSecretKey,
			null,
			settings.AwsBucket,
			settings.AwsBucketRegion);
	}
}

public class AwsS3Test : IStoreTest, IClassFixture<AwsS3Fixture> {
	public AwsS3Test(AwsS3Fixture fixture) : base(fixture) {
	}
}
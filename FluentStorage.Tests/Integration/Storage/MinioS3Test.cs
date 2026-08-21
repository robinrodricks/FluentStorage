namespace FluentStorage.Tests.Integration.Storage;

public class MinioS3Fixture : StoreFixture {
	protected override IStore CreateStorage(TestConfig settings) {

		if (string.IsNullOrEmpty(TestConfigLoader.Config.MinioS3AccessKey))
			throw new Exception("Required setting `MinioS3AccessKeyId` is blank!");

		if (string.IsNullOrEmpty(TestConfigLoader.Config.MinioS3SecretKey))
			throw new Exception("Required setting `MinioS3SecretAccessKey` is blank!");

		if (string.IsNullOrEmpty(TestConfigLoader.Config.MinioS3Bucket))
			throw new Exception("Required setting `MinioS3BucketName` is blank!");

		if (string.IsNullOrEmpty(TestConfigLoader.Config.MinioS3AwsRegion))
			throw new Exception("Required setting `MinioS3AwsRegion` is blank!");

		if (string.IsNullOrEmpty(TestConfigLoader.Config.MinioS3ServerUrl))
			throw new Exception("Required setting `MinioS3ServerUrl` is blank!");

		return MinioS3Storage.FromCredentials(
			settings.MinioS3AccessKey,
			settings.MinioS3SecretKey,
			settings.MinioS3Bucket,
			settings.MinioS3AwsRegion,
			settings.MinioS3ServerUrl);
	}
}

public class MinioS3Test : IStoreTest, IClassFixture<MinioS3Fixture> {
	public MinioS3Test(MinioS3Fixture fixture) : base(fixture) {
	}
}
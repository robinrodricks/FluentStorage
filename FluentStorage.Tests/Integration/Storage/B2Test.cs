namespace FluentStorage.Tests.Integration.Storage;

public class B2Fixture : StoreFixture {
	protected override IStore CreateStorage(TestConfig settings) {

		if (string.IsNullOrEmpty(TestConfigLoader.Config.B2AccessKey))
			throw new Exception("Required setting `B2AccessKeyId` is blank!");

		if (string.IsNullOrEmpty(TestConfigLoader.Config.B2SecretKey))
			throw new Exception("Required setting `B2SecretAccessKey` is blank!");

		if (string.IsNullOrEmpty(TestConfigLoader.Config.B2Bucket))
			throw new Exception("Required setting `B2BucketName` is blank!");

		if (string.IsNullOrEmpty(TestConfigLoader.Config.B2Region))
			throw new Exception("Required setting `B2Region` is blank!");

		return BackblazeB2Storage.FromCredentials(
			settings.B2AccessKey,
			settings.B2SecretKey,
			settings.B2Bucket,
			settings.B2Region);
	}
}

public class B2Test : IStoreTest, IClassFixture<B2Fixture> {
	public B2Test(B2Fixture fixture) : base(fixture) {
	}
}
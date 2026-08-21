namespace FluentStorage.Tests.Integration.Storage;

public class MongoFixture : StoreFixture {
	protected override IStore CreateStorage(TestConfig settings) {

		if (string.IsNullOrEmpty(TestConfigLoader.Config.MongoHost))
			throw new Exception("Required setting `MongoHost` is blank!");

		if (TestConfigLoader.Config.MongoPort == 0)
			throw new Exception("Required setting `MongoPort` is blank!");

		if (string.IsNullOrEmpty(TestConfigLoader.Config.MongoUsername))
			throw new Exception("Required setting `MongoUsername` is blank!");

		if (string.IsNullOrEmpty(TestConfigLoader.Config.MongoPassword))
			throw new Exception("Required setting `MongoPassword` is blank!");

		if (string.IsNullOrEmpty(TestConfigLoader.Config.MongoDatabase))
			throw new Exception("Required setting `MongoDatabaseName` is blank!");

		return MongoGridStorage.FromCredentials(
			settings.MongoHost,
			settings.MongoPort,
			settings.MongoUsername,
			settings.MongoPassword,
			settings.MongoDatabase,
			settings.MongoBucket,
			settings.MongoAuthDatabase,
			settings.MongoSsl);
	}
}

public class MongoTest : IStoreTest, IClassFixture<MongoFixture> {
	public MongoTest(MongoFixture fixture) : base(fixture) {
	}
}
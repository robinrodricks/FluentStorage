namespace FluentStorage.Tests.Integration.Storage;

public class FtpTestFixture : StoreFixture {
	protected override IStore CreateStorage(TestConfig settings) {

		// make sure required config properties are filled
		if (string.IsNullOrEmpty(TestConfigLoader.Config.FtpHost)) {
			throw new Exception("Required setting `FtpHost` is blank!");
		}
		if (string.IsNullOrEmpty(TestConfigLoader.Config.FtpUsername)) {
			throw new Exception("Required setting `FtpUsername` is blank!");
		}
		if (string.IsNullOrEmpty(TestConfigLoader.Config.FtpPassword)) {
			throw new Exception("Required setting `FtpPassword` is blank!");
		}

		return FtpStorage.FromCredentials(settings.FtpHost,
			new System.Net.NetworkCredential(settings.FtpUsername, settings.FtpPassword),
			FluentFTP.FtpDataConnectionType.AutoActive);
	}
}

public class FtpTest : IStoreTest, IClassFixture<FtpTestFixture> {
	public FtpTest(FtpTestFixture fixture) : base(fixture) {
	}
}
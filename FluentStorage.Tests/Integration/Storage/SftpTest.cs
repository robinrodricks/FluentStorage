using Renci.SshNet;

namespace FluentStorage.Tests.Integration.Storage;

public class SftpTestFixture : StoreFixture {
	protected override IStore CreateStorage(TestConfig settings) {

		// make sure required config properties are filled
		if (string.IsNullOrEmpty(TestConfigLoader.Config.SftpHost)) {
			throw new Exception("Required setting `SftpHost` is blank!");
		}
		if (TestConfigLoader.Config.SftpPort == 0) {
			throw new Exception("Required setting `SftpPort` is blank!");
		}
		if (string.IsNullOrEmpty(TestConfigLoader.Config.SftpUser)) {
			throw new Exception("Required setting `SftpUser` is blank!");
		}
		if (string.IsNullOrEmpty(TestConfigLoader.Config.SftpPrivateKeyPath)) {
			throw new Exception("Required setting `SftpPrivateKeyPath` is blank!");
		}
		if (string.IsNullOrEmpty(TestConfigLoader.Config.SftpPassphrase)) {
			throw new Exception("Required setting `SftpPassphrase` is blank!");
		}

		// By default the SSH private key of the PC is stored in `C:\Users\[USER]\.ssh\id_ed[`.
		string privateKey = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), settings.SftpPrivateKeyPath);

		// connect using SSH private key
		var keyFile = new PrivateKeyFile(privateKey, settings.SftpPassphrase);

		// new SFTP connection
		var connInfo = new PrivateKeyConnectionInfo(settings.SftpHost, settings.SftpPort, settings.SftpUser, keyFile);
		return SftpStorage.FromConnectionInfo(connInfo);
	}
}

public class SftpTest : IStoreTest, IClassFixture<SftpTestFixture> {
	public SftpTest(SftpTestFixture fixture) : base(fixture) {
	}
}
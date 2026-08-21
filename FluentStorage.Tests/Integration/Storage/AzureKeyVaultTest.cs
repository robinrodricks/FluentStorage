namespace FluentStorage.Tests.Integration.Storage;

public class AzureKeyVaultFixture : StoreFixture {
	protected override IStore CreateStorage(TestConfig settings) {

		// make sure required config properties are filled
		if (string.IsNullOrEmpty(TestConfigLoader.Config.AzureKeyVaultUri?.AbsoluteUri)) {
			throw new Exception("Required setting `AzureKeyVaultUri` is blank!");
		}
		if (string.IsNullOrEmpty(TestConfigLoader.Config.AzureTenantId)) {
			throw new Exception("Required setting `AzureTenantId` is blank!");
		}
		if (string.IsNullOrEmpty(TestConfigLoader.Config.AzureClientId)) {
			throw new Exception("Required setting `AzureClientId` is blank!");
		}
		if (string.IsNullOrEmpty(TestConfigLoader.Config.AzureClientSecret)) {
			throw new Exception("Required setting `AzureClientSecret` is blank!");
		}

		return AzureKeyVaultStorage.FromCredentials(
			settings.AzureKeyVaultUri,
			settings.AzureTenantId,
			settings.AzureClientId,
			settings.AzureClientSecret);
	}
}

public class AzureKeyVaultTest : IStoreTest, IClassFixture<AzureKeyVaultFixture> {
	public AzureKeyVaultTest(AzureKeyVaultFixture fixture) : base(fixture) {
	}
}
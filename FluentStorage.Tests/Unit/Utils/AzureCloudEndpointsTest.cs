using Azure.Identity;
using FluentStorage.Azure;

namespace FluentStorage.Tests.Unit.Utils;

public class AzureCloudEndpointsTest {

	[Theory]
	[InlineData(AzureCloudEnvironment.Global, "core.windows.net")]
	[InlineData(AzureCloudEnvironment.China, "core.chinacloudapi.cn")]
	[InlineData(AzureCloudEnvironment.USGovernment, "core.usgovcloudapi.net")]
	public void GetBlobEndpoint_VariousEnvironments_ReturnsExpected(AzureCloudEnvironment env, string expected) {
		string result = AzureCloudEndpoints.GetBlobEndpoint(env);
		Assert.Equal(expected, result);
	}

	[Theory]
	[InlineData(AzureCloudEnvironment.Global, "dfs.core.windows.net")]
	[InlineData(AzureCloudEnvironment.China, "dfs.core.chinacloudapi.cn")]
	[InlineData(AzureCloudEnvironment.USGovernment, "dfs.core.usgovcloudapi.net")]
	public void GetDataLakeEndpoint_VariousEnvironments_ReturnsExpected(AzureCloudEnvironment env, string expected) {
		string result = AzureCloudEndpoints.GetDataLakeEndpoint(env);
		Assert.Equal(expected, result);
	}

	[Theory]
	[InlineData(AzureCloudEnvironment.Global)]
	[InlineData(AzureCloudEnvironment.China)]
	[InlineData(AzureCloudEnvironment.USGovernment)]
	public void GetAuthorityEndpoint_VariousEnvironments_ReturnsExpectedUris(AzureCloudEnvironment env) {
		Uri result = AzureCloudEndpoints.GetAuthorityEndpoint(env);

		Uri expected = env switch {
			AzureCloudEnvironment.China => AzureAuthorityHosts.AzureChina,
			AzureCloudEnvironment.USGovernment => AzureAuthorityHosts.AzureGovernment,
			_ => AzureAuthorityHosts.AzurePublicCloud
		};

		Assert.Equal(expected, result);
	}
}
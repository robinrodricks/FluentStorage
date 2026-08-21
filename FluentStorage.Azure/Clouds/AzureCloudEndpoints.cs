using System;
using System.Collections.Generic;
using Azure.Identity;

namespace FluentStorage.Azure;

/// <summary>
/// Provides Azure service endpoint hostnames and authority URIs for different cloud environments.
/// </summary>
public static class AzureCloudEndpoints {

	/// <summary>
	/// Container for Blob storage endpoint suffixes.
	/// </summary>
	private static Dictionary<AzureCloudEnvironment, string> _blobUrls = new(){
		{AzureCloudEnvironment.Global, "core.windows.net"},
		{AzureCloudEnvironment.USGovernment, "core.usgovcloudapi.net"},
		{AzureCloudEnvironment.China, "core.chinacloudapi.cn"},
	};

	/// <summary>
	/// Container for Data Lake (ADLS Gen2) endpoint suffixes.
	/// </summary>
	private static Dictionary<AzureCloudEnvironment, string> _dataLakeUrls = new(){
		{AzureCloudEnvironment.Global, "dfs.core.windows.net"},
		{AzureCloudEnvironment.USGovernment, "dfs.core.usgovcloudapi.net"},
		{AzureCloudEnvironment.China, "dfs.core.chinacloudapi.cn"},
	};

	/// <summary>
	/// Container for Azure authority (token issuer) endpoints.
	/// </summary>
	private static Dictionary<AzureCloudEnvironment, Uri> _authorityUris = new(){
		{AzureCloudEnvironment.Global, AzureAuthorityHosts.AzurePublicCloud},
		{AzureCloudEnvironment.USGovernment, AzureAuthorityHosts.AzureGovernment},
		{AzureCloudEnvironment.China, AzureAuthorityHosts.AzureChina},
	};

	/// <summary>
	/// Gets the Blob storage endpoint suffix for the specified Azure cloud environment.
	/// </summary>
	/// <param name="environment">The Azure cloud environment.</param>
	/// <returns>The Blob endpoint suffix (e.g. "core.windows.net").</returns>
	public static string GetBlobEndpoint(AzureCloudEnvironment environment) => _blobUrls[environment];

	/// <summary>
	/// Gets the Data Lake endpoint suffix for the specified Azure cloud environment.
	/// </summary>
	/// <param name="environment">The Azure cloud environment.</param>
	/// <returns>The Data Lake endpoint suffix (e.g. "dfs.core.windows.net").</returns>
	public static string GetDataLakeEndpoint(AzureCloudEnvironment environment) => _dataLakeUrls[environment];

	/// <summary>
	/// Gets the authority (STS) endpoint URI for the specified Azure cloud environment.
	/// </summary>
	/// <param name="environment">The Azure cloud environment.</param>
	/// <returns>The authority endpoint URI.</returns>
	public static Uri GetAuthorityEndpoint(AzureCloudEnvironment environment) => _authorityUris[environment];
}
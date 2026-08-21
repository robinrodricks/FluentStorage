namespace FluentStorage.Azure;

/// <summary>
/// Azure cloud environments with their respective endpoints
/// </summary>
public enum AzureCloudEnvironment {
	/// <summary>
	/// Azure Global Cloud (default)
	/// Is also known as Public Cloud
	/// </summary>
	Global,

	/// <summary>
	/// Azure China Cloud (operated by 21Vianet)
	/// </summary>
	China,

	/// <summary>
	/// Azure US Government Cloud
	/// </summary>
	USGovernment,

	/// <summary>
	/// Azure Germany Cloud
	/// </summary>
	Germany
}
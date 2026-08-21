namespace FluentStorage.ConnectionStrings;

/// <summary>
/// Parameter names that are supported in FluentStorage connection strings
/// </summary>
public static class ConnectionStringParam {
	/// <summary>
	/// Indicates that this connection string is native
	/// </summary>
	public static string Native = "native";

	/// <summary>
	/// Account or storage name
	/// </summary>
	public static readonly string AccountName = "account";

	/// <summary>
	/// Key or password
	/// </summary>
	public static readonly string KeyOrPassword = "key";

	/// <summary>
	/// Key ID
	/// </summary>
	public static readonly string KeyId = "keyId";

	/// <summary>
	/// Session token
	/// </summary>
	public static readonly string SessionToken = "st";

	/// <summary>
	/// Name of a local profile
	/// </summary>
	public static readonly string LocalProfileName = "profile";

	/// <summary>
	/// Bucket name
	/// </summary>
	public static readonly string BucketName = "bucket";

	/// <summary>
	/// Region
	/// </summary>
	public static readonly string Region = "region";

	/// <summary>
	/// Host Name
	/// </summary>
	public static readonly string HostName = "hostname";

	/// <summary>
	/// Service URL
	/// </summary>
	public static readonly string ServiceUrl = "serviceUrl";

	/// <summary>
	/// Account ID
	/// </summary>
	public static readonly string AccountId = "accountId";

	/// <summary>
	/// Use Development Storage
	/// </summary>
	public static readonly string UseDevelopmentStorage = "development";

	/// <summary>
	/// Vault URI
	/// </summary>
	public static readonly string VaultUri = "vaultUri";

	/// <summary>
	/// Tenant ID
	/// </summary>
	public static readonly string TenantId = "tenantId";

	/// <summary>
	/// ClientId
	/// </summary>
	public static readonly string ClientId = "principalId";

	/// <summary>
	/// ClientSecret
	/// </summary>
	public static readonly string ClientSecret = "principalSecret";

	/// <summary>
	/// MsiEnabled
	/// </summary>
	public static readonly string MsiEnabled = "msi";

	/// <summary>
	/// IsLocalEmulator
	/// </summary>
	public static readonly string IsLocalEmulator = "emu";
}
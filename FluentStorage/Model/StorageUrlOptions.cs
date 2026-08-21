using System;
using FluentStorage.Enums;

namespace FluentStorage.Model;

public sealed class StorageUrlOptions {

	/// <summary>
	/// [S3][Azure]
	/// Permissions granted by the URL.
	/// </summary>
	public StorageUrlPermissions Permissions { get; set; } =
		StorageUrlPermissions.Read;

	/// <summary>
	/// [S3][Azure]
	/// How long the URL should remain valid.
	/// </summary>
	public TimeSpan ExpiresIn { get; set; } = TimeSpan.FromHours(24);

	/// <summary>
	/// [Azure]
	/// Optional start time. If null, the URL becomes valid immediately.
	/// </summary>
	public DateTimeOffset? StartsOn { get; set; }

	/// <summary>
	/// [S3][Azure]
	/// Restrict access to HTTPS only.
	/// </summary>
	public bool RequireHttps { get; set; } = true;

	/// <summary>
	/// [S3][Azure]
	/// Optional IP address or CIDR range restriction.
	/// </summary>
	public string? IpRange { get; set; }

	/// <summary>
	/// [S3][Azure]
	/// Optional response Content-Type override.
	/// </summary>
	public string? ContentType { get; set; }

	/// <summary>
	/// [S3][Azure]
	/// Optional response Content-Disposition override.
	/// Example: attachment; filename="photo.jpg"
	/// </summary>
	public string? ContentDisposition { get; set; }

	/// <summary>
	/// [S3][Azure]
	/// Optional response Cache-Control override.
	/// </summary>
	public string? CacheControl { get; set; }

	/// <summary>
	/// [S3][Azure]
	/// Optional response Content-Encoding override.
	/// </summary>
	public string? ContentEncoding { get; set; }

	/// <summary>
	/// [S3][Azure]
	/// Optional response Content-Language override.
	/// </summary>
	public string? ContentLanguage { get; set; }

	/// <summary>
	/// [Azure]
	/// Azure Stored Access Policy identifier.
	/// </summary>
	public string? StoredAccessPolicy { get; set; }

	/// <summary>
	/// [Azure]
	/// Specifies how the URL should be signed.
	/// </summary>
	public StorageUrlSigning SigningMethod { get; set; } =
		StorageUrlSigning.Default;

	/// <summary>
	/// [Azure]
	/// Restricts the allowed transport protocols.
	/// </summary>
	public StorageUrlProtocol Protocol { get; set; } =
		StorageUrlProtocol.Https;

	/// <summary>
	/// [Azure]
	/// Azure SAS service version. Leave null to use the SDK default.
	/// </summary>
	public string? SignedVersion { get; set; }

	/// <summary>
	/// [Azure]
	/// Correlation identifier for User Delegation SAS.
	/// </summary>
	public string? CorrelationId { get; set; }

	/// <summary>
	/// [Azure]
	/// Encryption scope to use when accessing the resource.
	/// </summary>
	public string? EncryptionScope { get; set; }
}
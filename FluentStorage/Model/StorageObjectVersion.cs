using System;

namespace FluentStorage.Model;

/// <summary>
/// Represents a specific version of an object.
/// </summary>
public class StorageObjectVersion {

	/// <summary>The provider-specific version identifier.</summary>
	public string VersionId { get; set; } = "";

	/// <summary>True if this is the latest/current version.</summary>
	public bool IsCurrent { get; set; }

	/// <summary>The date and time this version was created.</summary>
	public DateTime DateCreated { get; set; }

	/// <summary>The length of the object in bytes.</summary>
	public long Length { get; set; }

	/// <summary>The object's ETag, if available.</summary>
	public string? ETag { get; set; }
}
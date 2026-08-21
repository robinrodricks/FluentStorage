using System;

namespace FluentStorage.Model;

/// <summary>
/// Represents the retention policy applied to an object.
/// </summary>
public class StorageRetentionPolicy {
	/// <summary>The date and time until which the object is protected.</summary>
	public DateTimeOffset RetainUntilUtc { get; set; }

	/// <summary>True if the retention period cannot be shortened.</summary>
	public bool IsLocked { get; set; }
}
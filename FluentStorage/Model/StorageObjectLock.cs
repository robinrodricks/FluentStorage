using System;
using FluentStorage.Enums;

namespace FluentStorage.Model;

/// <summary>
/// Represents the object lock configuration.
/// </summary>
public sealed class StorageObjectLock {
	/// <summary>The lock mode.</summary>
	public StorageLockMode Mode { get; set; }

	/// <summary>The date and time until which the object is locked.</summary>
	public DateTimeOffset LockedUntilUtc { get; set; }

	/// <summary>True if a legal hold is applied.</summary>
	public bool LegalHold { get; set; }
}
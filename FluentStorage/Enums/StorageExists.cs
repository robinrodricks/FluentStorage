namespace FluentStorage.Enums;

/// <summary>
/// Determines what to do when the destination file/object already exists.
/// </summary>
public enum StorageExists {
	/// <summary>
	/// Throw an exception if the object already exists.
	/// </summary>
	Throw,

	/// <summary>
	/// Skip the transfer if the object already exists. [FAST]
	/// </summary>
	Skip,

	/// <summary>
	/// Replace the object if it already exists. [SLOW]
	/// </summary>
	Overwrite,

	/// <summary>
	/// Replace the object if the object's length or last modified timestamp is different [FASTEST].
	/// </summary>
	OverwriteByTimestamp,

	/// <summary>
	/// Replace the object if the object's length or checksum is different [FAST].
	/// </summary>
	OverwriteByChecksum,
}